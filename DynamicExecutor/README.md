# DynamicExecutor
# 动态执行代码

在游戏内动态执行c#代码

## 使用步骤
- *太吾根目录/Mods/DynamicExecutor/Execute.csproj.template* 中的第32行，修改为你的解密的Assembly-CSharp.dll的路径
- 在游戏内按 Ctrl+F10 启动 UMM 的管理界面，设置你的msbuild.exe的路径。(如果是vs2017且安装在默认路径，应该不需要修改)
- 修改 *太吾根目录/Mods/DynamicExecutor/Execute.cs.template* 为你需要运行的代码
- 在 UMM 设置界面点击运行
- 编译的log保存在本mod目录中的compile.log
- Main函数返回结果可在 UMM 的 Logs 界面查看，亦可查看 *%APPDATA%/LocalLow/Conch Ship Game/The Scroll Of Taiwu Alpha V1.0/output_log.txt*

## 注意事项
- 程序的入口为Sth4nothing.Execute.Main
- class Execute不能为静态类
- Execute.Main必须为静态方法，返回类型任意，可为void
- 已支持其他的Mod调用
- 输出大量数据推荐使用Debug.Log来记录，结果查看output_log.txt即可
