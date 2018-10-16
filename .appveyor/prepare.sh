#!/bin/sh
set -ex

STEAMDIR="$(cmd.exe /C 'echo %cd%')\\dlls\\"

mkdir -p build dlls
cd build

cmake -G "Visual Studio 15 2017" -D "STEAMDIR=${STEAMDIR}" ..
