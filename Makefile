UNITY_YEAR ?= 2021
UNITY_VERSION_2021 ?= 2021.3.37f1
UNITY_VERSION_2022 ?= 2022.3.56f1
UNITY_VERSION_2023 ?= 2023.2.20f1
UNITY_VERSION = $(UNITY_VERSION_$(UNITY_YEAR))
UNITY_PATH ?= /Applications/Unity/Hub/Editor/$(UNITY_VERSION)/Unity.app/Contents/MacOS/Unity
BATCH_ARGS ?= -batchmode -logFile - -nographics -timestamps
BUILD_METHOD = EmbraceSDK.CIPublishTool.ExportUnityPackage
BUILD_PROJECT := $(or $(BUILD_PROJECT),./UnityProjects/$(UNITY_YEAR))

UNITY_SDK_VERSION = $(shell jq -r '.version' io.embrace.sdk/package.json)
UNITY_SDK_UNITYPACKAGE = build/EmbraceSDK_$(UNITY_SDK_VERSION).unitypackage

APPLE_SDK_VERSION ?= $(shell jq -r '.pins[] | select(.identity == "embrace-apple-sdk") | .state.version' Package.resolved)
APPLE_SDK_DIR = build/embrace_$(APPLE_SDK_VERSION)
APPLE_SDK_ZIP = build/embrace_$(APPLE_SDK_VERSION).zip

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

.PHONY: build clean download_apple_sdk github_env_vars install_ios_dependencies test test_all version

build: $(UNITY_SDK_UNITYPACKAGE)

clean:
	-rm -r build

# Print the environment variables, for writing to the GitHub action environment.
github_env_vars:
	@echo "APPLE_SDK_VERSION=$(APPLE_SDK_VERSION)"
	@echo "BUILD_METHOD=$(BUILD_METHOD)"
	@echo "BUILD_PROJECT=$(BUILD_PROJECT)"
	@echo "GAMECI_UNITYPACKAGE=$(BUILD_PROJECT)/$(notdir $(UNITY_SDK_UNITYPACKAGE))"
	@echo "UNITY_SDK_VERSION=$(UNITY_SDK_VERSION)"
	@echo "UNITY_VERSION=$(UNITY_VERSION)"
	@echo "UNITY_VERSION_2021=$(UNITY_VERSION_2021)"
	@echo "UNITY_VERSION_2022=$(UNITY_VERSION_2022)"
	@echo "UNITY_VERSION_2023=$(UNITY_VERSION_2023)"

# Install the Embrace Apple SDK dependencies into the Unity project. This is
# run before building the Unity package.
install_ios_dependencies: $(APPLE_SDK_DIR)
	-rm ./io.embrace.sdk/iOS/embrace_symbol_upload.darwin
	-rm ./io.embrace.sdk/iOS/run.sh
	mkdir -p ./io.embrace.sdk/iOS/
	cp $(APPLE_SDK_DIR)/embrace_symbol_upload.darwin $(APPLE_SDK_DIR)/run.sh ./io.embrace.sdk/iOS/

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
	unzip -q -o $(APPLE_SDK_ZIP) -d $(APPLE_SDK_DIR)

# Build the Unity package for the Embrace Unity SDK.
$(UNITY_SDK_UNITYPACKAGE): install_ios_dependencies
	$(UNITY_PATH) $(BATCH_ARGS) \
		-buildTarget android \
		-projectPath $(BUILD_PROJECT) \
		-quit \
		-executeMethod $(BUILD_METHOD)
	mv -v $(BUILD_PROJECT)/$(notdir $@) $@
