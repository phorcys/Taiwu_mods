#!/bin/env bash -ex
cd build
mkdir -p Mods_publish
cd Mods

mkdir -p ${HOME}/.taiwu/Mods_publish

echo "Cached mod dir file list:"
ls -al ${HOME}/.taiwu/ | true

echo "Cached mod list:"
ls -al ${HOME}/.taiwu/Mods_publish | true
for modzip in `ls *.zip`
do
	modfullname=`echo $modzip |sed -e "s/\.zip//g"`
	modname=`echo $modfullname |sed -e "s/-.*//g"`
	modversion=`echo $modfullname |sed -e "s/.*-//g"`
	lastcommit=`git log ../../${modname}|  head -n 1 | awk  '{print $2}'`
	lastcommittime=`git log ../../${modname}|  head -n 3 |tail -n 1| sed -e "s/Date:\s*//g"`
	lastbuildcommit=""
	if [ -f ${HOME}/.taiwu/${modname}.commit ]
	then 
		lastbuildcommit=`cat ${HOME}/.taiwu/${modname}.commit` | true
	fi
	lastbuildcommittime=
	if [ -f ${HOME}/.taiwu/${modname}.commit.time ]
	then
		lastbuildcommittime=`cat ${HOME}/.taiwu/${modname}.commit.time` | true
	fi
	
	echo "Processing Mod $modname commit $lastcommit , last build commit $lastbuildcommit \n   last commit time: $lastcommittime   last build commit time : $lastbuildcommittime"
	
	if [ "$lastcommit" != "$lastbuildcommit" ] || [ "$lastbuildcommit" = "" ]
	then
		COMM_TAG="$(git describe --tags $(git rev-list --tags --max-count=1))"
		modurl="https://github.com/phorcys/Taiwu_mods/releases/download/${COMM_TAG}/${modzip}"
	
		# mods that need publish and regenerate json
		python ../../.travis/addnewrelease.py ${HOME}/.taiwu/${modname}.json "${modname}" "${modversion}" "${modurl}"
		
		\cp -Rf ${modzip} ${HOME}/.taiwu/Mods_publish/ | true
		\cp -Rf  ${HOME}/.taiwu/${modname}.json ${HOME}/.taiwu/Mods_publish/ | true
		echo "Published Mod  ${modfullname} to Github Release page, Release tag : ${COMM_TAG}"
	fi
	\cp -Rf ${modzip} ../Mods_publish/ | true
	if [ -f ${HOME}/.taiwu/${modname}.json ]
	then
		\cp -Rf ${HOME}/.taiwu/${modname}.json ../Mods_publish/ | true
	fi
done
echo "List Published Mods:"
ls -al ../Mods_publish/
