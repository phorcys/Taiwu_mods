# 功能介绍
BaseResourceMod 的功能：

1. 启动时候导出所有配置表的 csv 和 json 到 Backup 目录供分析；
1. 启动时，额外从游戏根目录 Data 目录下任意子目录内载入增量配置文件合并到游戏。

   增量意思是，你只需要填你新增和改变的那些行，不变的不需要动，mod 会自动合并进游戏配置文件。命名规则类似 MakeItem_Date.txt.001.txt。MakeItem_Date 是配置文件类型，参见导出来的配置文件，001 是载入顺序号，随意写，mod 从 001-999 按顺序载入。

   其他 mod 也可以把当前 mod 目录的指定子目录通过 BaseResourceMod.Main.registModResDir 接口注册，子目录内容也同样按照规则载入。这样如果制作其他 mod，就无需去修改 assets 文件了，修改 assets 每次更新就没了。

1. 支持游戏图片资源外置加载，可以导出游戏图片资源，mod 会自动从游戏根目录下 Texture 目录中的子目录里，按照导出的目录结构，寻找替换的图片资源。

   mod 也可以依赖 BaseResourceMod，通过 registModSpriteDir 接口，注册 mod 本地目录图片资源目录。BaseResourceMod 会在需要时，一起加载。

   **使用方法**：

   - 修改图片（对所有能通过本MOD导出的贴图都支持）

     先开启导出图片资源，参照导出的目录结构，建立对应目录，放入修改后**同名**图片。

	   例如，我要修改某物品图标，通过导出图片资源，观察发现该物品的图标为 `...\The Scroll Of Taiwu\Backup\Texture\ItemIcon\ItemIcon_193.png` 文件，即 `\ItemIcon` 为目标目录，`ItemIcon_193.png` 为目标文件。所以将修改后的 `ItemIcon_193.png` 放入 `...\The Scroll Of Taiwu\Texture\testmod\ItemIcon` 目录下，testmod 是你自己随便起的 mod 名称。

   - 新增图片(对部分种类的贴图支持)

	   先开启导出图片资源，参照导出的目录结构，建立对应目录，放入按下列规则命名的文件

	   命名规则为: `名字_序号.png` 或者 `名字_序号_其他内容.png`

	   `名字` 可以是为任意字符（不包含下划线）组成的组合，只要放入的目录正确，名字本身不重要。

	   `序号` 为想要插入的位置（从零算起），需要大于对应的导出的目录中的文件数，否则会替换掉已有的文件

	   `其他内容` 可以是任意内容

	   例如，我想加入新的物品图标，在导出的目录中发现`...\The Scroll Of Taiwu\Backup\Texture\ItemIcon`目录放置所有物品图标，共有193个文件。但是我想要在第200号位置加入新贴图，则将创建的新图片命名为`ItemIcon_200.png`，放入`...\The Scroll Of Taiwu\Texture\testmod\ItemIcon` 目录下，testmod 是你自己随便起的 mod 名称。 **不要通过序号来替换贴图，见前面** `序号` **的说明**

	   **目录名和文件名全部大小写敏感。**

   所有图片支持修改，目前不支持新增的列表：

   ```
   Cricket/Cricket
   WorldMap/MianMapBack
   ActorFace
   ActorFaceSmall
   ChildFace
   ChildFaceSmall
   ```

# Changelog
## 1.0.14.4
- 修复: 兼容游戏版本 0.2.5.X
## 1.0.14
- 解决内存泄露问题，加快外部贴图载入速度
## 1.0.13
- 修复: 兼容游戏版本 0.2.3.1
- 新增: 现在HomeMap/HomeBuildingIcon也支持新增
## 1.0.12
- 修复：兼容游戏版本 0.1.7.3

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
