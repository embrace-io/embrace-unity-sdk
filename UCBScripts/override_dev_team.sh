#!/bin/bash
# This script updates the appleDeveloperTeamID in Unity's ProjectSettings.asset file
project_settings=($(find . -name ProjectSettings.asset))

for file in "${project_settings[@]}"; do
    echo "$file"
    if grep "appleDeveloperTeamID:" "$file"; then
        echo "Found appleDeveloperTeamID in $file"
        yq eval ".PlayerSettings.appleDeveloperTeamID = \"$APPLE_TEAM_ID\"" -i "$file"
        echo "Updated appleDeveloperTeamID in $file"
    else
        echo "appleDeveloperTeamID not found in $file"
    fi
done