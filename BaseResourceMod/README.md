# 功能介绍
BaseResourceMod 的功能：  

1. 启动时候 dump 所有配置表的 csv 和 json 到 Backup 目录供分析；
1. 启动时，额外从游戏根目录 Data 目录下任意子目录内载入增量配置文件合并到游戏。

    增量意思是，你只需要填你新增和改变的那些行，不变的不需要动，mod 会自动合并进游戏配置文件。命名规则类似 MakeItem_Date.txt.001.txt。MakeItem_Date 是配置文件类型，参见 dump 出来的配置文件，001 是载入顺序号，随意写，mod 从 001-999 按顺序载入。  

    其他 mod 也可以把当前 mod 目录的指定子目录通过 BaseResourceMod.Main.registModResDir 接口注册，子目录内容也同样按照规则载入。这样如果制作其他 mod，就无需去修改 assets 文件了，修改 assets 每次更新就没了。  

1. 支持游戏图片资源外置加载，可以 dump 游戏图片资源，mod 会自动从游戏根目录下 Texture 目录中的子目录里，按照 dump 出的目录结构，寻找替换的图片资源。  

    mod 也可以依赖 BaseResourceMod，通过 registModSpriteDir 接口，注册 mod 本地目录图片资源目录。BaseResourceMod 会在需要时，一起加载。  

    使用方法：先开启 dump 图片资源，参照 dump 出的目录结构，建立对应目录，放入修改后图片。  
    例如，我要修改物品 193 号，则将修改后的 ItemIcon_193.png 放入 `C:\...\The Scroll Of Taiwu\Texture\testmod\ItemIcon\ItemIcon\` 目录下，testmod 是你自己随便起的 mod 名称。  
    目录名和文件名全部大小写敏感。所有图片支持修改，除蛐蛐、地形、人脸、建筑图标之外的支持新增。  

    目前不支持新增的列表：  

    ```
    HomeMap/HomeBuildingIcon
    Cricket/Cricket
    WorldMap/MianMapBack
    ActorFace
    ActorFaceSmall
    ChildFace
    ChildFaceSmall
    ```


# Changelog
## 1.0.11
- 修复：兼容游戏版本 0.1.7.3

## 1.0.10
- 修复：兼容游戏版本 0.1.7.2

## 1.0.9
- 修复：1.0.8 版开始游戏时报红字

## 1.0.8
- 兼容游戏版本 0.1.7.0

## 1.0.7
- 修复：无法载入自定义图片
