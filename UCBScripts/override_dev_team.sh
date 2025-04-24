#!/bin/bash
# This script updates the appleDeveloperTeamID in Unity's ProjectSettings.asset file
project_settings=($(find . -name ProjectSettings.asset))

for file in "${project_settings[@]}"; do
    if grep -q "appleDeveloperTeamID:" "$file"; then
        yq eval ".PlayerSettings.appleDeveloperTeamID = \"$APPLE_TEAM_ID\"" -i "$file"
    fi
done