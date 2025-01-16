#!/bin/bash

# Define the source and destination directories
src_dir="./io.embrace.sdk"
dest_dir="./UnityProjects/2021/Packages"

# Check if the source directory exists
if [ ! -d "$src_dir" ]; then
    echo "Source directory $src_dir does not exist."
    exit 1
fi

# Check if the destination directory exists
if [ ! -d "$dest_dir" ]; then
    echo "Destination directory $dest_dir does not exist."
    exit 1
fi

# Copy the directory
cp -R "$src_dir" "$dest_dir"

echo "Directory copied successfully."
