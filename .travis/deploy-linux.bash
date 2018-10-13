#!/bin/env bash -ex
shopt -s nocasematch
cd build
cd Mods
for modzip in `ls *.zip`
do
	modname=`echo $modzip |sed -e s/\.zip//g`
	echo "Processing Mod $modname"
	COMM_TAG="$(git describe --tags $(git rev-list --tags --max-count=1))"
	COMM_COUNT="$(git rev-list --count HEAD)"
	curl "${UPLOAD_URL}${TRAVIS_COMMIT:0:8}&t=${COMM_TAG}&a=${COMM_COUNT}" --upload-file ./*.zip
fi
if [ "$DEPLOY_PPA" = "true" ]; then
	export DEBFULLNAME="Taiwu_Mods Build Bot"
fi