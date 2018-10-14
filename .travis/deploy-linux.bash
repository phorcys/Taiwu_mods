#!/bin/env bash -ex
cd build
mkdir -p Mods_publish
cd Mods

mkdir -p ${HOME}/.taiwu

for modzip in `ls *.zip`
do
	modfullname=`echo $modzip |sed -e "s/\.zip//g"`
	modname=`echo $modfullname |sed -e "s/-.*//g"`
	modversion=`echo $modfullname |sed -e "s/.*-//g"`
	lastcommit=`git log ../../${modname}|  head -n 1 | awk  '{print $2}'`
	lastcommittime=`git log ../../InventorySort|  head -n 3 |tail -n 1| sed -e "s/Date:\s*//g"`
	lastbuildcommit=`cat ${HOME}/.taiwu/${modname}.commit`
	lastbuildcommittime=`cat ${HOME}/.taiwu/${modname}.commit.time`
	echo "Processing Mod $modname commit $lastcommit , last build commit $lastbuildcommit \n   last commit time: $lastcommittime   last build commit time : $lastbuildcommittime"
	
	if [ "$lastcommit" != "$lastbuildcommit" ]
	then
		COMM_TAG="$(git describe --tags $(git rev-list --tags --max-count=1))"
		releasecommit=${lastcommit:0:8}
		modurl="https://github.com/phorcys/Taiwu_mods/releases/download/${COMM_TAG}/${modzip}"
	
		# mods that need publish and regenerate json
		python ../../.travis/addnewrelease.py ${HOME}/.taiwu/${modname}.json "${modname}" "${modversion}" "${modurl}"
		
		cp  ${modzip} ../Mods_publish/
		cp  ${HOME}/.taiwu/${modname}.json ../Mods_publish/
		echo "Published Mod  ${modfullname} to Github Release page, Release tag : ${COMM_TAG}"
	fi
fi
