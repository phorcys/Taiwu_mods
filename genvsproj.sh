#!/bin/sh

set -e

REG_KEY_NAME='HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 838350'
REG_FIELD_NAME='InstallLocation'
PATH="/mnt/c/Windows/System32/:/mnt/c/Program Files (x86)/Microsoft Visual Studio/2017/Community/Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin:${PATH}"

GAME_PATH="$(reg.exe query "$REG_KEY_NAME" /f "$REG_FIELD_NAME" \
  | grep "$REG_FIELD_NAME" | sed -E "s/^\\s+${REG_FIELD_NAME}\\s+REG_SZ\\s+(.*)\\s+$/\\1/g")"

mkdir -p build
cd build
cmake.exe -G "Visual Studio 15 2017 Win64" -D STEAMDIR="${GAME_PATH}\\"  ..
