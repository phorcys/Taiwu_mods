#!/usr/bin/python
# -*- coding: UTF-8 -*

import sys
import json
import io

k1="Id"
k2="Version"
k3="DownloadUrl"
k4="Releases"
outstr=""
try:
	with io.open(sys.argv[1], "r") as f:
		r=json.load(f)
		n=r.copy()
		n.pop(k4,None)
		r[k4].append(n)
		r[k1]=sys.argv[2]
		r[k2]=sys.argv[3]
		r[k3]=sys.argv[4]
		outstr=json.dumps(r,ensure_ascii=False, encoding='utf-8');
except:
    print('[New Mod]'+ sys.argv[1] + ' not found . Create new...')
    r={}
    r[k1]=sys.argv[2]
    r[k2]=sys.argv[3]
    r[k3]=sys.argv[4]
    r[k4]=[]
    outstr=json.dumps(r,ensure_ascii=False, encoding='utf-8');
	
with io.open(sys.argv[1], "w") as f:
    f.write(outstr.decode('unicode_escape'))