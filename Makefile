UNITY_YEAR ?= 2021
UNITY_VERSION_2021 ?= 2021.3.37f1
UNITY_VERSION_2022 ?= 2022.3.56f1
UNITY_VERSION_2023 ?= 2023.2.20f1
UNITY_VERSION = $(UNITY_VERSION_$(UNITY_YEAR))
UNITY_PATH ?= /Applications/Unity/Hub/Editor/$(UNITY_VERSION)/Unity.app/Contents/MacOS/Unity
BATCH_ARGS ?= -batchmode -logFile - -nographics -timestamps
BUILD_METHOD = EmbraceSDK.CIPublishTool.ExportUnityPackage
BUILD_PROJECT ?= ./UnityProjects/$(UNITY_YEAR)

UNITY_SDK_VERSION = $(shell jq -r '.version' io.embrace.sdk/package.json)
UNITY_SDK_UNITYPACKAGE = build/EmbraceSDK_$(UNITY_SDK_VERSION).unitypackage
UNITY_SDK_XCFRAMEWORK = build/EmbraceUnityiOS.xcframework

APPLE_SDK_VERSION ?= 6.6.0
APPLE_SDK_DIR = build/embrace_$(APPLE_SDK_VERSION)
APPLE_SDK_ZIP = build/embrace_$(APPLE_SDK_VERSION).zip

# Build an intermediate xcarchive, which is used to create the final xcframework.
# Arguments:
# $(1) - destination (e.g. generic/platform=iOS)
# $(2) - output path (e.g. build/EmbraceUnityiOS)
define build_xcarchive
	echo "Building xcarchive for $(1) -> $(2)..."
	env IS_ARCHIVE=1 xcodebuild \
		archive \
		-project "./Embrace Unity iOS Interface/EmbraceUnityiOS.xcodeproj" \
		-scheme "EmbraceUnityiOS" \
		-destination "$(1)" \
		-archivePath "$(2)" \
		SKIP_INSTALL=NO \
		BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
		ONLY_ACTIVE_ARCH=NO \
		| xcpretty
endef

# Run Unity tests.
# Arguments:
# $(1) - Unity project year (e.g. 2021)
# $(2) - Unity version (e.g. 2021.3.37f1)
# $(3) - build target (e.g. ios)
# $(4) - test platform (e.g. editmode)
define run_tests
	/Applications/Unity/Hub/Editor/$(2)/Unity.app/Contents/MacOS/Unity \
		$(BATCH_ARGS) \
		-buildTarget $(3) \
		-projectPath ./UnityProjects/$(1) \
		-runTests \
		-testPlatform $(4) \
		-testResults $(PWD)/build/test-$(1)-$(3)-$(4)/test-results.xml \
		-debugCodeOptimization \
		-enableCodeCoverage \
		-coverageResultsPath $(PWD)/build/test-$(1)-$(3)-$(4)/coverage-results \
		-coverageOptions "generateAdditionalMetrics;generateBadgeReport;generateHtmlReport;assemblyFilters:+Embrace,+Embrace.*" \
		| awk '{ print "[$(1)-$(3)-$(4)] " $$0 }'
endef

# Run all tests for a given Unity project year.
# Arguments:
# $(1) - Unity project year (e.g. 2021)
define run_tests_year
	-$(call run_tests,$(1),$(UNITY_VERSION_$(1)),osxuniversal,editmode)
	-$(call run_tests,$(1),$(UNITY_VERSION_$(1)),ios,editmode)
	-$(call run_tests,$(1),$(UNITY_VERSION_$(1)),ios,playmode)
	-$(call run_tests,$(1),$(UNITY_VERSION_$(1)),android,editmode)
	-$(call run_tests,$(1),$(UNITY_VERSION_$(1)),android,playmode)
endef

.PHONY: build build_ios_dependencies clean download_apple_sdk github_env_vars install_ios_dependencies test test_all version

build: $(UNITY_SDK_UNITYPACKAGE)

build_ios_dependencies: $(APPLE_SDK_DIR) $(UNITY_SDK_XCFRAMEWORK)

download_apple_sdk: $(APPLE_SDK_DIR)

clean:
	-rm -r build

# Print the environment variables, for writing to the GitHub action environment.
github_env_vars:
	@echo "APPLE_SDK_VERSION=$(APPLE_SDK_VERSION)"
	@echo "BUILD_METHOD=$(BUILD_METHOD)"
	@echo "BUILD_PROJECT=$(BUILD_PROJECT)"
	@echo "BUILD_UNITYPACKAGE=$(BUILD_PROJECT)/$(notdir $(UNITY_SDK_UNITYPACKAGE))"
	@echo "UNITY_SDK_VERSION=$(UNITY_SDK_VERSION)"
	@echo "UNITY_VERSION=$(UNITY_VERSION)"
	@echo "UNITY_VERSION_2021=$(UNITY_VERSION_2021)"
	@echo "UNITY_VERSION_2022=$(UNITY_VERSION_2022)"
	@echo "UNITY_VERSION_2023=$(UNITY_VERSION_2023)"

# Install the Embrace Apple SDK dependencies and built Unity xcframework into
# the Unity project. This is run before building the Unity package.
install_ios_dependencies: $(APPLE_SDK_DIR) $(UNITY_SDK_XCFRAMEWORK)
	-rm -r ./io.embrace.sdk/iOS/xcframeworks/*.xcframework
	-rm ./io.embrace.sdk/iOS/embrace_symbol_upload.darwin
	-rm ./io.embrace.sdk/iOS/run.sh
	cp -rv $(APPLE_SDK_DIR)/xcframeworks/*.xcframework ./io.embrace.sdk/iOS/xcframeworks/
	cp -v $(APPLE_SDK_DIR)/embrace_symbol_upload.darwin $(APPLE_SDK_DIR)/run.sh ./io.embrace.sdk/iOS/
	cp -rv $(UNITY_SDK_XCFRAMEWORK) ./io.embrace.sdk/iOS/xcframeworks/

test:
	$(call run_tests_year,$(UNITY_YEAR))

test_all:
	$(call run_tests_year,2021)
	$(call run_tests_year,2022)
	$(call run_tests_year,2023)

version:
	@echo $(UNITY_SDK_VERSION)

# Download the Embrace Apple SDK release from GitHub.
$(APPLE_SDK_ZIP):
	gh release download "$(APPLE_SDK_VERSION)" --repo embrace-io/embrace-apple-sdk --pattern 'embrace_$(APPLE_SDK_VERSION).zip' --dir ./build --clobber

# Unzip the Embrace Apple SDK release.
$(APPLE_SDK_DIR): $(APPLE_SDK_ZIP)
	unzip -o $(APPLE_SDK_ZIP) -d $(APPLE_SDK_DIR)

# Build the XCFramework for the Unity Swift wrapper around the Embrace Apple SDK.
$(UNITY_SDK_XCFRAMEWORK): $(APPLE_SDK_DIR)
	-rm -r './Embrace Unity iOS Interface/xcframeworks'
	cp -rv $(APPLE_SDK_DIR)/xcframeworks './Embrace Unity iOS Interface/xcframeworks'
	$(call build_xcarchive,generic/platform=iOS,$(basename $@))
	$(call build_xcarchive,generic/platform=iOS Simulator,$(basename $@)-simulator)
	xcodebuild \
		-create-xcframework \
		-archive $(basename $@).xcarchive \
		-framework EmbraceUnityiOS.framework \
		-archive $(basename $@)-simulator.xcarchive \
		-framework EmbraceUnityiOS.framework \
		-output $@ \
		| xcpretty
	rm -r $(basename $@).xcarchive
	rm -r $(basename $@)-simulator.xcarchive

# Build the Unity package for the Embrace Unity SDK.
$(UNITY_SDK_UNITYPACKAGE): install_ios_dependencies
	$(UNITY_PATH) $(BATCH_ARGS) \
		-buildTarget android \
		-projectPath $(BUILD_PROJECT) \
		-quit \
		-executeMethod $(BUILD_METHOD)
	mv -v $(BUILD_PROJECT)/$(notdir $@) $@
