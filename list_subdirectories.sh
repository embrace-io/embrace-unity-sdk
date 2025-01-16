#!/bin/bash

# Check if a directory is provided as an argument
if [ -z "$1" ]; then
  echo "Usage: $0 <directory>"
  exit 1
fi

# Check if the provided argument is a directory
if [ ! -d "$1" ]; then
  echo "Error: $1 is not a directory"
  exit 1
fi

# List all top-level subdirectories
for dir in "$1"/*/; do
  if [ -d "$dir" ]; then
    echo "$(basename "$dir")"
  fi
done