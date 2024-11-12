#!/bin/bash

# Define the directory containing xcframeworks and the project.yml file
XCFRAMEWORKS_DIR="./xcframeworks"
PROJECT_YML="project.yml"

# Verify that the xcframeworks directory exists
if [ ! -d "$XCFRAMEWORKS_DIR" ]; then
  echo "Directory $XCFRAMEWORKS_DIR does not exist."
  exit 1
fi

# Gather all xcframeworks into a list format
xcframeworks_list=()
for xcframework in "$XCFRAMEWORKS_DIR"/*.xcframework; do
  if [ -d "$xcframework" ]; then
    xcframeworks_list+=("$xcframework")
  fi
done

# Create a temporary YAML file with the new dependencies
TEMP_YML=$(mktemp)
echo "dependencies:" > "$TEMP_YML"
for xcframework in "${xcframeworks_list[@]}"; do
  echo "  - framework: $xcframework" >> "$TEMP_YML"
  echo "    embed: true" >> "$TEMP_YML"
done

# Replace the dependencies block in the project.yml file
yq eval 'del(.targets.EmbraceUnityiOS.dependencies) | .targets.EmbraceUnityiOS.dependencies = load("'"$TEMP_YML"'").dependencies' "$PROJECT_YML" > temp.yml && mv temp.yml "$PROJECT_YML"

# Clean up
rm "$TEMP_YML"

echo "Updated $PROJECT_YML with xcframeworks list."