#!/bin/sh
set -ex

DLL_VERSION='181013'
DLL_DIR='dlls'
DLL_PATH="${DLL_DIR}/The Scroll Of Taiwu Alpha V1.0_Data/Managed"

[ -f "${DLL_PATH}/version" ] && {
  CURRENT_VERSION="$(cat "${DLL_PATH}/version")"
  [ "${CURRENT_VERSION}" = "${DLL_VERSION}" ] && {
    cp "${DLL_PATH}/Assembly-CSharp.dll" .
    exit
  }
  rm -rf "$DLL_DIR"
}

mkdir -p "$DLL_PATH"

curl -Lo dlls.zip "https://github.com/phorcys/Taiwu_mods/releases/download/dlls/dlls-${DLL_VERSION}.zip"
unzip dlls.zip -d "$DLL_PATH"
mv "${DLL_PATH}/dlls"/* "$DLL_PATH"
cp "${DLL_PATH}/Assembly-CSharp.dll" .

echo "$DLL_VERSION" > "${DLL_PATH}/version"
