set STEAMDIR="D:\Software\steam\steamapps\common\The Scroll Of Taiwu"

mkdir build

cd build


cmake -G "Visual Studio 15 2017" -D STEAMDIR=%STEAMDIR%  ..


cd ..
