PLATFORM = $(shell python3 .github/scripts/vars.py platform)

ifeq ($(strip $(EDITOR_VERSION)),)
# If the editor version is not set, default to 2021.
BUILD_TARGET := $(or $(BUILD_TARGET),android)
EDITOR_VERSION = $(shell python3 .github/scripts/vars.py editor-versions --project 2021 --field version)
EDITOR_CHANGESET = $(shell python3 .github/scripts/vars.py editor-versions --project 2021 --field changeset)
endif
EXTRA_BUILD_ARGS = $(if $(UNITY_SERIAL),, --skip-license)
EXTRA_INSTALL_ARGS = $(if $(BUILD_TARGET), --module $(BUILD_TARGET),)
EXTRA_TEST_ARGS = $(if $(BUILD_TARGET), --build-target $(BUILD_TARGET),)$(if $(UNITY_SERIAL),, --skip-license)

UNITY_SDK_VERSION = $(shell python3 .github/scripts/vars.py sdk-version)
UNITY_SDK_UNITYPACKAGE = build/EmbraceSDK_$(UNITY_SDK_VERSION).unitypackage

APPLE_SDK_VERSION ?= $(shell python3 .github/scripts/vars.py apple-sdk-version)
APPLE_SDK_DIR = build/embrace_$(APPLE_SDK_VERSION)
APPLE_SDK_ZIP = build/embrace_$(APPLE_SDK_VERSION).zip

.PHONY: build clean github_env_vars install_ios_dependencies test test_all version build_source_generator

# Build the Unity package for the Embrace Unity SDK.
build: $(UNITY_SDK_UNITYPACKAGE)

# Build the source generator DLL
build_source_generator:
	dotnet build EmbraceUnitySourceGenerator/EmbraceUnitySourceGenerator.csproj --configuration Release
	cp EmbraceUnitySourceGenerator/bin/Release/netstandard2.0/EmbraceUnitySourceGenerator.dll io.embrace.sdk/Scripts/EmbraceUnitySourceGenerator.dll

# Remove ephemeral build artifacts.
clean:
	-rm -r build

# Print out environment variables needed by the GitHub build action.
github_env_vars:
	@echo "BUILD_TARGET=$(BUILD_TARGET)"
	@echo "EDITOR_CHANGESET=$(EDITOR_CHANGESET)"
	@echo "EDITOR_VERSION=$(EDITOR_VERSION)"
	@echo "UNITY_SDK_UNITYPACKAGE=$(UNITY_SDK_UNITYPACKAGE)"
	@echo "UNITY_SDK_VERSION=$(UNITY_SDK_VERSION)"

# Install the Unity editor. This is used by the GitHub workflows.
install_editor:
	python3 .github/scripts/unity.py --version "$(EDITOR_VERSION)" install --changeset "$(EDITOR_CHANGESET)" $(EXTRA_INSTALL_ARGS)
# There is currently a bug on Linux where the Android Support package is not
# fully installed. Work around this by manually extracting the package.
ifeq ($(PLATFORM),linux)
	cd /opt/unity/editors/$(EDITOR_VERSION)/Editor/Data/PlaybackEngines/AndroidPlayer && \
		test -e UnityEditor.Android.Extensions.dll || (zcat TargetSupport.pkg.tmp/Payload | cpio -iu)
endif

# Install the Unity Hub, which will be used to install the Unity editor.
# This is used by the GitHub workflows.
install_hub:
ifeq ($(PLATFORM),darwin)
	brew install --cask unity-hub
else ifeq ($(PLATFORM),linux)
	bash .github/scripts/install_hub_linux.sh
else ifeq ($(PLATFORM),win32)
	powershell.exe -ExecutionPolicy Bypass -File .github/scripts/install_hub_windows.ps1
else
	$(error Platform "$(PLATFORM)" not supported)
endif

# Install the Embrace Apple SDK dependencies into the Unity project. This is
# run before building the Unity package.
install_ios_dependencies: $(APPLE_SDK_DIR)
	-rm ./io.embrace.sdk/iOS/embrace_symbol_upload.darwin
	-rm ./io.embrace.sdk/iOS/run.sh
	mkdir -p ./io.embrace.sdk/iOS/
	cp $(APPLE_SDK_DIR)/embrace_symbol_upload.darwin $(APPLE_SDK_DIR)/run.sh ./io.embrace.sdk/iOS/

# Run the Unity tests.
test:
	python3 .github/scripts/unity.py --version "$(EDITOR_VERSION)" test $(EXTRA_TEST_ARGS)

# Uninstall the Unity editor. This is used by the GitHub workflows.
uninstall_editor:
	python3 .github/scripts/unity.py --version "$(EDITOR_VERSION)" uninstall

# Download the Embrace Apple SDK release from GitHub.
$(APPLE_SDK_ZIP):
	gh release download "$(APPLE_SDK_VERSION)" --repo embrace-io/embrace-apple-sdk --pattern 'embrace_$(APPLE_SDK_VERSION).zip' --dir ./build --clobber

# Unzip the Embrace Apple SDK release.
$(APPLE_SDK_DIR): $(APPLE_SDK_ZIP)
	unzip -q -o $(APPLE_SDK_ZIP) -d $(APPLE_SDK_DIR)

# Build the Unity package for the Embrace Unity SDK.
$(UNITY_SDK_UNITYPACKAGE): build_source_generator install_ios_dependencies
	python3 .github/scripts/unity.py --version $(EDITOR_VERSION) build $(EXTRA_BUILD_ARGS)
