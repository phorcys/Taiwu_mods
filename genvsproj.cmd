::set STEAMDIR="C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Scroll Of Taiwu\\"
set STEAMDIR="E:\\SteamLibrarys\\steamapps\\common\\The Scroll Of Taiwu\\"
mkdir build
cd build

cmake -G "Visual Studio 15 2017 Win64" -D STEAMDIR=%STEAMDIR%  ..