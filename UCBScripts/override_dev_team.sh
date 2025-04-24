#!/bin/bash
# This script updates the appleDeveloperTeamID in Unity's ProjectSettings.asset file
project_settings=($(find . -name ProjectSettings.asset))

for file in "${project_settings[@]}"; do
    if grep -q "appleDeveloperTeamID:" "$file"; then
        header_lines=$(( $(awk '/PlayerSettings:/ {print NR; exit}' "$file") - 1 ))
        head -n "$header_lines" "$file" > temp_header
        tail -n +$((header_lines + 1)) "$file" > temp_body
        yq eval ".PlayerSettings.appleDeveloperTeamID = \"$APPLE_TEAM_ID\"" -i "temp_body"
        cat temp_header temp_body > "$file"
        rm temp_header temp_body
    fi
done