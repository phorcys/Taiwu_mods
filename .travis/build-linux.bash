#!/bin/env bash -ex
shopt -s nocasematch

cd /build/Taiwu_Mods

# Download pre-compiled llvm libs
echo "downloading dep dll packs...."
curl -sLO https://github.com/phorcys/Taiwu_mods/releases/download/dll181223/dlls-181223.zip
unzip ./dlls-181127.zip -d ..


mkdir -p build ; cd build

cmake -DCMAKE_BUILD_TYPE=Release ..
cmake --build .

echo "List builded Mods:"
ls -al Mods

cd ..

# If it compiled succesfully let's deploy
echo "build finished , result : $? , TRAVIS_BRANCH $TRAVIS_BRANCH , TRAVIS_PULL_REQUEST $TRAVIS_PULL_REQUEST"
/bin/bash -ex .travis/deploy-linux.bash

