UNITY_VERSION ?= 2021.3.37f1
UNITY_PATH ?= /Applications/Unity/Hub/Editor/$(UNITY_VERSION)/Unity.app/Contents/MacOS/Unity
BATCH_ARGS ?= -batchmode -logFile - -nographics -timestamps
BUILD_METHOD = EmbraceSDK.CIPublishTool.ExportUnityPackage
BUILD_PROJECT ?= ./UnityProjects/2021

APPLE_SDK_VERSION = 6.6.0
PACKAGE_VERSION = $(shell jq -r '.version' io.embrace.sdk/package.json)
PACKAGE_FILE = EmbraceSDK_$(PACKAGE_VERSION).unitypackage

define build_archive
	xcodebuild \
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

define run_tests
	$(UNITY_PATH) $(BATCH_ARGS) \
		-buildTarget $(1) \
		-projectPath $(BUILD_PROJECT) \
		-runTests \
		-testPlatform $(2) \
		-testResults ./build/test-results-$(1)-$(2).xml
		-enableCodeCoverage \
		-debugCodeOptimization \
		-coverageOptions "generateAdditionalMetrics;generateHtmlReport;generateBadgeReport;assemblyFilters:+Embrace,+Embrace.*
endef

.PHONY: all clean update_apple_sdk

all: build/$(PACKAGE_FILE)

build/$(PACKAGE_FILE): build/EmbraceUnityiOS.xcframework
	-rm -r ./io.embrace.sdk/iOS/xcframeworks/*.xcframework
	cp -r ./"Embrace Unity iOS Interface"/xcframeworks/*.xcframework ./io.embrace.sdk/iOS/xcframeworks/
	cp -r ./io.embrace.sdk $(BUILD_PROJECT)/Packages
	$(UNITY_PATH) $(BATCH_ARGS) -buildTarget android -projectPath $(BUILD_PROJECT) -quit -executeMethod $(BUILD_METHOD)
	mv $(BUILD_PROJECT)/$(PACKAGE_FILE) $@

build/EmbraceUnityiOS.xcarchive: export IS_ARCHIVE = 1
build/EmbraceUnityiOS.xcarchive:
	$(call build_archive,generic/platform=iOS,$(basename $@))

build/EmbraceUnityiOS-simulator.xcarchive: export IS_ARCHIVE = 1
build/EmbraceUnityiOS-simulator.xcarchive:
	$(call build_archive,generic/platform=iOS Simulator,$(basename $@))

build/EmbraceUnityiOS.xcframework: build/EmbraceUnityiOS.xcarchive build/EmbraceUnityiOS-simulator.xcarchive
	xcodebuild \
		-create-xcframework \
		-archive build/EmbraceUnityiOS.xcarchive \
		-framework EmbraceUnityiOS.framework \
		-archive build/EmbraceUnityiOS-simulator.xcarchive \
		-framework EmbraceUnityiOS.framework \
		-output "$@" \
		| xcpretty

clean:
	-rm -r build

test:
	$(call run_tests,osxuniversal,editmode)
	$(call run_tests,ios,editmode)

# Update the embrace-apple-sdk files to the specified version. This will
# download the specified version of the Apple SDK from the Embrace GitHub
# releases page and update the files in the project.
update-apple-sdk:
	gh release download "$(APPLE_SDK_VERSION)" --repo embrace-io/embrace-apple-sdk --pattern 'embrace_*.zip' --dir ./build --clobber
	-rm -r ./build/embrace-apple-sdk
	-rm -r "./Embrace Unity iOS Interface/xcframeworks"
	cd build && unzip -o embrace_$(APPLE_SDK_VERSION).zip -d ./embrace-apple-sdk
	cd build && mv -v embrace-apple-sdk/xcframeworks "../Embrace Unity iOS Interface/xcframeworks"
	cd build && mv -v embrace-apple-sdk/run.sh embrace-apple-sdk/*.darwin ../io.embrace.sdk/iOS/
	-rm -r build/embrace_$(APPLE_SDK_VERSION).zip build/embrace-apple-sdk
