#!/bin/bash

rm -rf ./xcframeworks/*

gh release download --repo embrace-io/embrace-apple-sdk --pattern 'embrace_*.zip' --dir ./

unzip -o embrace_*.zip

rm -rf embrace_*.zip run.sh *.darwin