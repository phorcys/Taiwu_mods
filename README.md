#Taiwu_mods
#太吾绘卷游戏Mod

[![Build Status](https://travis-ci.com/phorcys/Taiwu_mods.svg?branch=master)](https://travis-ci.com/phorcys/Taiwu_mods)

****
## 编译依赖
*  Visual Studio 2017 (Community版)
*  .NET Framework 3.5
*  游戏本体
*  从QQ群下载解密的Assembly-CSharp.dll,存放在本地仓库的根目录
*  修改genvsproj.cmd ，将里面的 STEAMDIR 设置为本机 太吾 安装目录
*  如果没有cmake，安装cmake  3.12或以上版本 并将cmake加入环境变量PATH ( 编写此说明时，使用 3.12.3 版本测试ok)
*  命令行运行 genvsproj.cmd, 会自动生成Visual Studio的解决方案到build目录下
*  mod目录下的.cs会自动加入工程,.dll会自动作为依赖，其他的文件如 .md，.txt等会自动拷贝到游戏的Mod路径
*  cmake会自动为工程添加post build事件，build成功后，如果游戏mod目录下存在mod同名文件夹，则自动复制dll到 游戏mods目录下对应mod目录内

****
## 新建 Mod流程

1. 新建目录,将你的mod的 .cs文件放入
2. 在此目录下放入 **Info.json** (注意大小写，编码为*utf8 with bom*)文件，格式类似：
```json
{
    "Id": "HerbRecipes",
    "DisplayName": "药引烹饪配方精制材料说明",
    "Author": "phorcys",
    "Version": "2.3.0",
    "AssemblyName": "HerbRecipes.dll",
    "EntryMethod": "HerbRecipes.Main.Load",
    "Requirements": ["BaseResourceMod"]
}
```
3. 除 最后一行 Requirements 外，其他为必填
4. 在太吾游戏路径下的Mods文件夹中，新建一个文件夹存放你的mod，如：*E:/SteamLibrary/steamapps/common/The Scroll Of Taiwu/Mods/HerbRecipes/*
5. 运行genvsproj.cmd 生成工程，开始mod开发

## 注意事项 

##Mods 开发辅助工具 repo：

https://github.com/phorcys/Taiwu_Mods_Tools.git

