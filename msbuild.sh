#!/bin/sh

set -e

PATH="/mnt/c/Windows/System32/:/mnt/c/Program Files (x86)/Microsoft Visual Studio/2017/Community/MSBuild/15.0/Bin/:${PATH}"
msbuild.exe $@
