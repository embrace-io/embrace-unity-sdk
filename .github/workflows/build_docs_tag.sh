#!/bin/bash

# Run in the embrace-io docs repo to generate a unique docs release tag

release=1
today=$(date +%Y%m%d)

# Increment release until tag (today.release) is unique
while [ ! -z $(git tag -l $today.$release) ]
do
    release=$((release+1))
done

echo $today.$release


