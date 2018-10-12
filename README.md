# Taiwu_mods
太吾绘卷游戏Mod


#编译依赖

1. 需要 unity tools for visuallstuio （可选）
2. 游戏本体
3. ActorMenu之类的在 Assembly-CSharp.dll里，有加密，解开即可
4. 修改genvsproj.cmd ，将里面的 STEAMDIR 设置为本机 太吾 安装目录
5. 如果没有cmake，安装cmake  3.12或以上版本  （ 测试3.12.3 ok）
6. 命令行运行 genvsproj.cmd, 会自动生成所需的工程文件。Mod目录下的.cs会自动加入工程,.dll会自动作为依赖
   会自动添加post build事件，build成功自动复制dll到 游戏mods目录下对应mod目录内

#讨论，qq群 90311 09 62

