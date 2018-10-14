#!/bin/env bash -ex
shopt -s nocasematch

cd /build/Taiwu_Mods

# Download pre-compiled llvm libs
echo "downloading dep dll packs...."
curl -sLO https://github.com/phorcys/Taiwu_mods/releases/download/dlls/dlls-181013.zip
unzip ./dlls-181013.zip -d ..


mkdir -p build ; cd build

cmake -DCMAKE_BUILD_TYPE=Release ..
cmake --build .

cd ..

# If it compiled succesfully let's deploy
if [ $? -eq 0 ] && [ "$TRAVIS_BRANCH" = "master" ]  && [ "$TRAVIS_PULL_REQUEST" = false ]; then /bin/bash -ex .travis/deploy-linux.bash ; fi
