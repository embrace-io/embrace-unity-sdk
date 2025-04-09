#!/bin/bash

# Set environment variables
export app_id="YOUR_APP_ID"
export api_token="YOUR_API_TOKEN"

# Path to Unity 2021.3.48f1 on macOS
# Change this to the version of Unity you are using
UNITY_PATH="/Applications/Unity/Hub/Editor/2021.3.48f1/Unity.app/Contents/MacOS/Unity"

# Launch Unity
"$UNITY_PATH"
