#Taiwu_mods
#太吾绘卷游戏Mod

[![Build Status](https://travis-ci.com/phorcys/Taiwu_mods.svg?branch=master)](https://travis-ci.com/phorcys/Taiwu_mods)

##编译依赖

*1. Visual Studio 2017 (Community版)
*2. 游戏本体
*3. 解密的Assembly-CSharp.dll,和本README.md文件放一个目录下里
*4. 修改genvsproj.cmd ，将里面的 STEAMDIR 设置为本机 太吾 安装目录
*5. 如果没有cmake，安装cmake  3.12或以上版本  ( 编写此说明时，使用 3.12.3 版本测试ok)
*6. 命令行运行 genvsproj.cmd, 会自动生成所需的工程文件到build目录下。
*7. mod目录下的.cs会自动加入工程,.dll会自动作为依赖
*8. cmake会自动为工程添加post build事件，build成功后，如果游戏mod目录下存在同名mod，则自动复制dll到 游戏mods目录下对应mod目录内

##新建 Mod流程

*1. 新建目录, 将你的mod .cs文件放入
*2. 在此目录下放入 Info.json 文件，格式类似：

    {
      "Id": "HerbRecipes",
      "DisplayName": "药引烹饪配方精制材料说明",
      "Author": "phorcys",
      "Version": "2.3.0",
      "AssemblyName": "HerbRecipes.dll",
      "EntryMethod": "HerbRecipes.Main.Load",
      "Requirements": ["BaseResourceMod"]
    }
    
*3. 除 最后一行 Requirements 外，其他为必填
*4. 运行genvsproj.cmd 生成工程，开始mod开发

##Mods 开发辅助工具 repo：

https://github.com/phorcys/Taiwu_Mods_Tools.git

