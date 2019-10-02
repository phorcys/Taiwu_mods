#define DEBUG
using Harmony12;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;

namespace BaseResourceMod
{
    internal class SpriteLoadHelper
    {
        private static SpriteLoadHelper instance;

        /// <summary>Sprite路径库</summary>
        private readonly Dictionary<string, string> singleSpritePathInfosDic;
        /// <summary>SpriteAtlas的Sprite名称缓存</summary>
        private readonly Dictionary<string, List<string>> runTimeNamesCache;
        /// <summary>SpriteAtlas的Atlas信息缓存</summary>
        private readonly Dictionary<string, AtlasInfo.AtlasDetail> runTimeCache;
        /// <summary>一般Sprite名称组</summary>
        private readonly Dictionary<string, string[]> commonNameGroup;
        /// <summary>地图地块Sprite名称组</summary>
        private readonly Dictionary<int, string[]> placeBack;
        /// <summary>促织Sprite名称组</summary>
        private readonly Dictionary<int, string[]> cricketImage;
        /// <summary>儿童角色脸部Sprite名称组</summary>
        private readonly Dictionary<int, Dictionary<int, string[]>> childFace;
        /// <summary>儿童角色脸部小尺寸Sprite名称组</summary>
        private readonly Dictionary<int, Dictionary<int, string[]>> childFaceSmall;
        /// <summary>成年角色脸部Sprite名称组</summary>
        private readonly Dictionary<int, Dictionary<int, List<string[]>>> actorFace;
        /// <summary>成年角色脸部小尺寸Sprite名称组</summary>
        private readonly Dictionary<int, Dictionary<int, List<string[]>>> actorFaceSmall;


        private SpriteLoadHelper()
        {
            // 构造时执行反射，避免实际加载Sprite反复执行反射影响性能
            singleSpritePathInfosDic = (Dictionary<string, string>)Traverse.Create(Main.getSpritesInfoAsset)
                .Field("singleSpritePathInfosDic").GetValue();
            commonNameGroup = (Dictionary<string, string[]>)Traverse.Create(Main.getSpritesInfoAsset)
                .Field("commonNameGroup").GetValue();
            runTimeNamesCache = (Dictionary<string, List<string>>)Traverse.Create(AtlasInfo.Instance)
                .Field("runTimeNamesCache").GetValue();
            runTimeCache = (Dictionary<string, AtlasInfo.AtlasDetail>)Traverse.Create(AtlasInfo.Instance)
                .Field("runTimeCache").GetValue();
            cricketImage = (Dictionary<int, string[]>)Traverse.Create(Main.getSpritesInfoAsset)
                .Field("_cricketImage").GetValue();
            placeBack = (Dictionary<int, string[]>)Traverse.Create(Main.getSpritesInfoAsset)
                .Field("_placeBack").GetValue();
            actorFace = (Dictionary<int, Dictionary<int, List<string[]>>>)Traverse.Create(Main.getSpritesInfoAsset)
                .Field("_actorFace").GetValue();
            actorFaceSmall = (Dictionary<int, Dictionary<int, List<string[]>>>)Traverse.Create(Main.getSpritesInfoAsset)
                .Field("_actorFaceSmall").GetValue();
            childFace = (Dictionary<int, Dictionary<int, string[]>>)Traverse.Create(Main.getSpritesInfoAsset)
                .Field("_childFace").GetValue();
            childFaceSmall = (Dictionary<int, Dictionary<int, string[]>>)Traverse.Create(Main.getSpritesInfoAsset)
                .Field("_childFaceSmall").GetValue();
        }

        /// <summary>
        /// 获取SpriteLoadHelper实例
        /// </summary>
        /// <returns>SpriteLoadHelper实例</returns>
        public static SpriteLoadHelper GetInstance()
        {
            if (instance == null)
            {
                instance = new SpriteLoadHelper();
            }
            return instance;
        }

        /// <summary>
        /// 清除SpriteLoadHelper实例
        /// </summary>
        public static void ClearInstance() => instance = null;

        /// <summary>
        /// 替换基础sprite的路径
        /// </summary>
        /// <param name="spriteGroupName">sprite组名</param>
        /// <param name="spriteFilePaths">要更改成的sprite路径</param>
        private void ReplaceCommonSpriteImage(string spriteGroupName, string[] spriteFilePaths)
        {
            if (!commonNameGroup.TryGetValue(spriteGroupName, out string[] spriteNames) || spriteNames == null || spriteNames.Length == 0)
            {
                Main.Logger.Log($"SpriteGroup {spriteGroupName} not loaded yet.");
                return;
            }
#if DEBUG
            Main.Logger.Log($"Begin Injection for {spriteGroupName}, size before injection: {commonNameGroup[spriteGroupName].Length}");
#endif
            // Sprite名称数组编辑缓存
            List<string> spriteNamesCache = null;
            // 处理文件
            foreach (var filePath in spriteFilePaths)
            {
                // 文件名作为sprite名称
                string spriteName = Path.GetFileNameWithoutExtension(filePath);
                // sprite名字是否已经存在, 如果存在则只替换路径
                int index = Array.FindIndex(spriteNames, name => name == spriteName);
                // sprite名字不存在，添加
                if (index < 0)
                {
                    // 确定新sprite的插入位置，如 itemIcon_200_Something_Else.png, 插入index为200的位置
                    if (!int.TryParse(Regex.Match(spriteName, @"^(?>[^_]+_(\d+))").Groups[1].Value, out index) || index < 0)
                    {
                        Main.Logger.Log($"[Texture] FileName:{spriteName} is an illegal sprite name");
                        continue;
                    }

                    // 首次需要额外添加sprite时，创建spriteName编辑缓存，并将原有的sprite名称添加到编辑缓存
                    if (spriteNamesCache == null)
                    {
                        spriteNamesCache = new List<string>(spriteNames);
                    }
                    // 当sprite名称组不够长时加入null充数
                    while (spriteNamesCache.Count() < index + 1)
                    {
                        spriteNamesCache.Add(null);
                    }
                    // 新增名称
                    spriteNamesCache[index] = spriteName;
                }
                // 将路径加入自定义数据路径库
                Main.customSpriteInfosDic[spriteName] = (filePath, null);
                // 将所有通过基本资源框架成功加载的png文件计数(包括其他mod的加载请求)
                Main.imageFilesCounter[1]++;
                if (Main.settings.detailed_custom_texture_log)
                {
                    Main.Logger.Log($"[Texture] Injected sprite from {filePath} to spriteGroup {spriteGroupName} sprite name {spriteName}");
                }
            }
            // 仅当添加了新spriteName才需要同步缓存编辑结果
            if (spriteNamesCache != null)
                commonNameGroup[spriteGroupName] = spriteNamesCache.ToArray();

#if DEBUG
            Main.Logger.Log($"Done Injection for {spriteGroupName}, size after injection: {commonNameGroup[spriteGroupName].Length}");
#endif
        }

        /// <summary>
        /// 更改非常规sprite的路径
        /// </summary>
        /// <param name="spriteNameGroup">sprite名称组</param>
        /// <param name="spriteTyp">sprite类型</param>
        /// <param name="spriteFilePaths">要更改成的sprite路径</param>
        private void DoReplaceSpecialSpritePath(Dictionary<int, string[]> spriteNameGroup, string spriteTyp, string[] spriteFilePaths)
        {
            // 处理文件
            foreach (var filePath in spriteFilePaths)
            {
                // 获取不含扩展名的文件名
                string spriteName = Path.GetFileNameWithoutExtension(filePath);
                // 获得类别ID, 如Cricket_123_456.png, 获得123
                if (!int.TryParse(Regex.Match(spriteName, @"^(?>[^_]+_(\d+))").Groups[1].Value, out int cateid) || cateid < 0)
                {
                    Main.Logger.Log($"[Texture] FileName:{spriteName} is an illegal sprite name");
                    continue;
                }
                // 若sprite名字存在则替换路径
                if (spriteNameGroup != null && spriteNameGroup.TryGetValue(cateid, out string[] spriteGroup)
                    && spriteGroup != null && spriteGroup.Length > 0)
                {
                    if (Array.FindIndex(spriteGroup, name => name == spriteName) >= 0)
                    {
                        Main.customSpriteInfosDic[spriteName] = (filePath, null);
                        // 将所有通过基本资源框架成功加载的png文件计数(包括其他mod的加载请求)
                        Main.imageFilesCounter[1]++;
                        if (Main.settings.detailed_custom_texture_log)
                        {
                            Main.Logger.Log($"[Texture] Injected sprite from {filePath} to spriteGroup {spriteTyp} sprite name {spriteName}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更改非常规sprite的路径
        /// </summary>
        /// <param name="spriteNameGroup">sprite名称组</param>
        /// <param name="spriteTyp">sprite类型</param>
        /// <param name="spriteFilePaths">要更改成的sprite路径</param>
        private void DoReplaceSpecialSpritePath(Dictionary<int, Dictionary<int, string[]>> spriteNameGroup,
                                                          string spriteTyp,
                                                          string[] spriteFilePaths)
        {
            // 处理文件
            foreach (var filePath in spriteFilePaths)
            {
                // 获取不含扩展名的文件名
                string spriteName = Path.GetFileNameWithoutExtension(filePath);
                int[] cates = { -1, -1 };
                // 获得第一类别和第二类别ID，如ChildFace_1_0_3.png, 1为第一类ID，0为第二类ID
                if (!int.TryParse(Regex.Match(spriteName, @"^(?>[^_]+_(\d+))").Groups[1].Value, out cates[0])
                    || !int.TryParse(Regex.Match(spriteName, @"^(?>[^_]+_\d+_(\d+))").Groups[1].Value, out cates[1])
                    || cates[0] < 0 || cates[1] < 0)
                {

                    Main.Logger.Log($"[Texture] FileName:{spriteName} is an illegal sprite name");
                    continue;
                }
                // 存在就替换路径
                if (spriteNameGroup != null
                    && spriteNameGroup.TryGetValue(cates[0], out Dictionary<int, string[]> dict1)
                    && dict1.TryGetValue(cates[1], out string[] spriteGroup)
                    && spriteGroup != null && spriteGroup.Length > 0)
                {
                    if (Array.FindIndex(spriteGroup, name => name == spriteName) >= 0)
                    {
                        Main.customSpriteInfosDic[spriteName] = (filePath, null);
                        // 将所有通过基本资源框架成功加载的png文件计数(包括其他mod的加载请求)
                        Main.imageFilesCounter[1]++;
                        if (Main.settings.detailed_custom_texture_log)
                        {
                            Main.Logger.Log($"[Texture] Injected sprite from {filePath} to spriteGroup {spriteTyp} sprite name {spriteName}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更改常规sprite的路径
        /// </summary>
        /// <param name="spriteNameGroup">sprite名称组</param>
        /// <param name="spriteTyp">sprite类型</param>
        /// <param name="spriteFilePaths">要更改成的sprite路径</param>
        private void DoReplaceSpecialSpritePath(Dictionary<int, Dictionary<int, List<string[]>>> spriteNameGroup,
                                                         string spriteTyp,
                                                         string[] spriteFilePaths)
        {
            // 处理文件
            foreach (var filePath in spriteFilePaths)
            {
                // 获取不含扩展名的文件名
                string spriteName = Path.GetFileNameWithoutExtension(filePath);
                int[] cates = { -1, -1, -1 };
                // 获取第一、二、三类别ID，如ActorFace_5_4_3_2_1.png，5,4,3分别为第一、二、三类别ID
                if (!int.TryParse(Regex.Match(spriteName, @"^(?>[^_]+_(\d+))").Groups[1].Value, out cates[0])
                    || !int.TryParse(Regex.Match(spriteName, @"^(?>[^_]+_\d+_(\d+))").Groups[1].Value, out cates[1])
                    || !int.TryParse(Regex.Match(spriteName, @"^(?>[^_]+_\d+_\d+_(\d+))").Groups[1].Value, out cates[2])
                    || cates[0] < 0 || cates[1] < 0 || cates[2] < 0)

                {
                    Main.Logger.Log($"[Texture] FileName:{spriteName} is an illegal sprite name");
                    continue;
                }
                // 存在就替换路径
                if (spriteNameGroup != null && spriteNameGroup.TryGetValue(cates[0], out Dictionary<int, List<string[]>> dict1)
                    && dict1.TryGetValue(cates[1], out List<string[]> list) && list.Count() > cates[2]
                    && list[cates[2]] != null && list[cates[2]].Length > 0)
                {
                    if (Array.FindIndex(list[cates[2]], name => name == spriteName) >= 0)
                    {
                        Main.customSpriteInfosDic[spriteName] = (filePath, null);
                        // 将所有通过基本资源框架成功加载的png文件计数(包括其他mod的加载请求)
                        Main.imageFilesCounter[1]++;
                        if (Main.settings.detailed_custom_texture_log)
                        {
                            Main.Logger.Log($"[Texture] Injected sprite from {filePath} to spriteGroup {spriteTyp} sprite name {spriteName}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更改非常规sprite的路径
        /// </summary>
        /// <param name="spriteTyp">sprite类别名</param>
        /// <param name="spritefilePaths">要更改成的sprite路径</param>
        private void ReplaceSpecialSpritePath(string spriteTyp, string[] spritefilePaths)
        {
            switch (spriteTyp)
            {
                case "Cricket/Cricket":
                    DoReplaceSpecialSpritePath(cricketImage, spriteTyp, spritefilePaths);
                    break;
                case "WorldMap/MianMapBack":
                    DoReplaceSpecialSpritePath(placeBack, spriteTyp, spritefilePaths);
                    break;
                case "ActorFace":
                    DoReplaceSpecialSpritePath(actorFace, spriteTyp, spritefilePaths);
                    break;
                case "ActorFaceSmall":
                    DoReplaceSpecialSpritePath(actorFaceSmall, spriteTyp, spritefilePaths);
                    break;
                case "ChildFace":
                    DoReplaceSpecialSpritePath(childFace, spriteTyp, spritefilePaths);
                    break;
                case "ChildFaceSmall":
                    DoReplaceSpecialSpritePath(childFaceSmall, spriteTyp, spritefilePaths);
                    break;
            }
        }

        /// <summary>
        /// 替换游戏加载sprite时访问的sprite储存路径
        /// </summary>
        /// <param name="spriteTyp">sprite类型</param>
        /// <param name="spriteFilePaths">要更改成的sprite路径</param>
        private void DoReplaceSpritePath(string spriteTyp, string[] spriteFilePaths)
        {
            if (Main.sprite_instance_dict.TryGetValue(spriteTyp, out string spriteGroupName))
            {
                ReplaceCommonSpriteImage(spriteGroupName, spriteFilePaths);
            }
            else
            {
                ReplaceSpecialSpritePath(spriteTyp, spriteFilePaths);
            }
        }

        /// <summary>
        /// 获取特定根路径下的所有子目录中的png图片路径(不包含根路径本身的png文件）
        /// </summary>
        /// <param name="path">指定搜寻根目录路径</param>
        /// <returns>迭代返回 Tuple(子目录路径, 子目录中所有png图片路径)</returns>
        // 同一目录下的贴图属于同类。每次返回同一目录下的所有png文件，可以一起处理，减少修改基础sprite时创建新数组的次数，提高性能
        private IEnumerable<ValueTuple<string, string[]>> GetImageFiles(string path)
        {
            // 一级子目录路径
            string[] dirs;
            try
            {
                dirs = Directory.GetDirectories(path);
            }
            catch (Exception e)
            {
                Main.Logger.Log(e.ToString());
                yield break;
            }
            // 储存子目录路径的栈, 目录过多时使用递归可能会使系统栈溢出，故使用自定义的栈
            var dirStack = new Stack<string>();
            // 将子目录路径推入栈
            foreach (string dir in dirs)
            {
                dirStack.Push(dir);
            }
            // DFS遍历所有子文件夹，输出含有png文件的目录及其中png文件路径
            while (dirStack.Count != 0)
            {
                string currentDir = dirStack.Pop();
                try
                {
                    // 获得子目录的子目录
                    dirs = Directory.GetDirectories(currentDir);
                    // 将子文件夹路径推入栈
                    foreach (string dir in dirs)
                    {
                        dirStack.Push(dir);
                    }
                }
                catch (Exception e)
                {
                    Main.Logger.Log($"For Directory {currentDir}: {e.ToString()}");
                }

                string[] files;
                try
                {
                    // 获得当前目录中的png文件路径
                    files = Directory.GetFiles(currentDir, "*.png");
                }
                catch (Exception e)
                {
                    Main.Logger.Log($"For Directory {currentDir}: {e.ToString()}");
                    continue;
                }

                if (files.Length != 0)
                {
                    // 输出当前目录及其中png文件路径
                    yield return (currentDir, files);
                }
            }
        }

        /// <summary>
        /// 处理文件
        /// </summary>
        /// <param name="path">要处理的目录路径</param>
        public void ProcessDir(string path)
        {
            // 遍历该目录的所有子目录下的 png 文件
            foreach (var pair in GetImageFiles(path))
            {
                string parentDir = pair.Item1.Replace("\\", "/");
                string[] imageFiles = pair.Item2;
                // 去掉多余的路径, 如"./abc/bcd/ItemIcon/", 输出"ItemIcon", 即sprite类别
                int keyBeginIndex = (parentDir[path.Length] == '/') ? path.Length + 1 : path.Length;
                int keyLength = (parentDir[parentDir.Length - 1] == '/') ? parentDir.Length - keyBeginIndex - 1 : parentDir.Length - keyBeginIndex;
                string spriteTyp = parentDir.Substring(keyBeginIndex, keyLength);
                // 将所有欲通过基本资源框架加载的png文件计数(包括其他mod的加载请求)
                Main.imageFilesCounter[0] += imageFiles.Length;
                // 游戏中sprite名称以数组形式保存，每次增加sprite数量会创建新的数组，将相同类型的sprite集中处理
                // 可以减少数组对象的创建次数, 减少不必要的GC压力
                DoReplaceSpritePath(spriteTyp, imageFiles);
            }
        }

        /// <summary>
        /// 将sprite保存为png图片
        /// </summary>
        /// <param name="sprite">需要保存的sprite</param>
        /// <param name="path">保存路径</param>
        /// <param name="spriteName">sprite的名字</param>
        private static void DumpPNG(Sprite sprite, string path, string spriteName = null)
        {
            /*使用游戏中的sprite的名字，而非sprite文件本身的名字，因为游戏中的名字可能会改，如ActorFaceSmall类别里的sprite
            文件名为ActorFace_l_m_n_o_p，而游戏里的名字为ActorFace_l_m_n_o_p_small，为使输出的文件名与游戏中的名称相符，
            方便替换, 统一使用游戏中的名称*/
            string fileName = (spriteName == null || spriteName == "") ? sprite.name : spriteName;

            string filepath = Path.Combine(Main.backupimgdir, $"{path}/{fileName}.png");
            string dirpath = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(dirpath))
            {
                Directory.CreateDirectory(dirpath);
            }

            Texture2D img = sprite.texture;
            FilterMode fmbak = img.filterMode;
            img.filterMode = FilterMode.Point;
            var rt = RenderTexture.GetTemporary(img.width, img.height);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            //x y 坐标  y坐标需要变换
            int x, y, height, width;
            if (sprite.packed && sprite.packingMode == SpritePackingMode.Rectangle)
            {
                x = (int)sprite.textureRect.x;
                y = (int)sprite.textureRect.y;
                width = (int)sprite.textureRect.width;
                height = (int)sprite.textureRect.height;
            }
            else
            {
                x = (int)sprite.rect.x;
                y = (int)sprite.rect.y;
                width = (int)sprite.rect.width;
                height = (int)sprite.rect.height;
            }
            y = img.height - y - height;
#if DEBUG
            if (sprite.packed)
                Main.Logger.Log($"[Texture] sprite packingMode: {sprite.packingMode}");
            Main.Logger.Log($"[Texture] Blit texture {img.width},{img.height} to dest {width},{height} revert x y {x},{y} orignal offset {sprite.rect.x},{sprite.rect.y} , file {filepath}  orignal path {path} SpriteName: {sprite.name}");
#endif
            Graphics.Blit(img, rt);
            var img2 = new Texture2D(width, height, TextureFormat.ARGB32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            img2.ReadPixels(new Rect(x, y, width, height), 0, 0);
            img2.Apply();
            RenderTexture.active = null;
            img.filterMode = fmbak;
            byte[] bytes = ImageConversion.EncodeToPNG(img2);
            File.WriteAllBytes(filepath, bytes);
            RenderTexture.ReleaseTemporary(rt);
            UnityEngine.Object.DestroyImmediate(img2);
        }

        /// <summary>
        /// 导出Sprite
        /// </summary>
        /// <param name="spriteNames">需要导出的sprite的名称</param>
        /// <param name="dirpath">导出路径</param>
        /// <remarks>参考<see cref="DynamicSetSprite.SetImageSprite(UnityEngine.UI.Image, string, int[])"/></remarks>
        private void DoDumpSprite(string[] spriteNames, string dirpath)
        {
            if (spriteNames != null && spriteNames.Length > 0 && !dirpath.IsNullOrEmpty())
            {
#if DEBUG
                Main.Logger.Log($"[Texture] Sprite Save Path: {dirpath} count: {spriteNames.Length}");
#endif      
                foreach (var spriteName in spriteNames)
                {
                    if (singleSpritePathInfosDic.TryGetValue(spriteName, out string spritePath) || !string.IsNullOrEmpty(spritePath))
                    {
#if DEBUG
                        Main.Logger.Log($"[Texture] To Dump {spriteName}, path: {spritePath}");
#endif
                        // 增加待导出sprite数目, 使用原子操作以防日后游戏加载机制变成多线程
                        Interlocked.Increment(ref Main.spriteCounter);
                        ResLoader.Load(spritePath, delegate (Sprite sprite)
                        {
#if DEBUG
                            Main.Logger.Log($"[Texture] Dumping: {sprite.name} {spritePath}");
#endif                   
                            if (sprite != null)
                                DumpPNG(sprite, dirpath, spriteName);
                            // 减少待导出sprite数目, 使用原子操作以防日后游戏加载机制变成多线程
                            Interlocked.Decrement(ref Main.spriteCounter);
                        });
                    }
                    else
                    {
#if DEBUG
                        Main.Logger.Log($"[Texture] Atlas Loading, spriteName: {spriteName}");
#endif
                        /// 参考<see cref="AtlasInfo.SetImageSprite(UnityEngine.UI.Image, string)"/>
                        foreach (var pair in runTimeNamesCache)
                        {
                            if (pair.Value.Contains(spriteName))
                            {
                                // 增加待导出sprite数目, 使用原子操作以防日后游戏加载机制变成多线程
                                Interlocked.Increment(ref Main.spriteCounter);
                                if (!runTimeCache[pair.Key].isPacker)
                                {
#if DEBUG
                                    Main.Logger.Log($"[Texture] Atlas NonPacker Loading, spriteName: {spriteName}");
#endif
                                    AtlasInfo.Instance.LoadAtlas(pair.Key, delegate (Atlas atlas, bool needFade)
                                    {
                                        Sprite sprite = atlas.GetSprite(spriteName);
                                        if (sprite != null)
                                            DumpPNG(sprite, dirpath, spriteName);
                                        // 减少待导出sprite数目, 使用原子操作以防日后游戏加载机制变成多线程
                                        Interlocked.Decrement(ref Main.spriteCounter);
                                    });
                                }
                                else
                                {
#if DEBUG
                                    Main.Logger.Log($"[Texture] Atlas Packer Loading, spriteName: {spriteName}");
#endif
                                    AtlasInfo.Instance.LoadPacker(pair.Key, delegate (SpriteAtlas atlas, bool needFade)
                                    {
                                        Sprite sprite = atlas.GetSprite(spriteName);
                                        if (sprite != null)
                                            DumpPNG(sprite, dirpath, spriteName);
                                        // 减少待导出sprite数目, 使用原子操作以防日后游戏加载机制变成多线程
                                        Interlocked.Decrement(ref Main.spriteCounter);
                                    });
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Main.Logger.Log($"[Texture] empty sprites or invalid output Directory...{dirpath}");
            }
        }

        /// <summary>
        /// 导出Sprite
        /// </summary>
        /// <param name="spritePath"></param>
        /// <param name="dirpath"></param>
        private void DoDumpSprite(string spritePath, string dirpath)
        {
            if (!spritePath.IsNullOrEmpty() && !dirpath.IsNullOrEmpty())
            {
                Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
                if (sprites != null && sprites.Length > 0)
                {
                    foreach (var sprite in sprites)
                    {
#if DEBUG
                        Main.Logger.Log($"[Texture] ");
#endif
                    }
                }
                else
                {
                    Main.Logger.Log($"[Texture] invalid sprites loading path...{spritePath}");
                }
            }
            else
            {
                Main.Logger.Log($"[Texture] empty sprites loading path or invalid output Directory...{dirpath}");
            }
        }

        /// <summary>
        /// 导出基础sprite
        /// </summary>
        public void DumpCommonSprite()
        {
            try
            {
                if (Main.settings.save_common_sprite == true)
                {
                    foreach (var kv in Main.sprite_instance_dict)
                    {
                        if (commonNameGroup.TryGetValue(kv.Value, out string[] spriteNames))
                        {
                            DoDumpSprite(spriteNames, kv.Key);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Main.Logger.Log(e.Message);
                Main.Logger.Log(e.StackTrace);
            }
        }

        /// <summary>
        /// 导出特殊sprite
        /// </summary>
        public void DumpSpecialSprite()
        {
            try
            {
                if (Main.settings.save_maptile == true)
                {
                    //地图
                    foreach (var kv in placeBack)
                    {
                        DoDumpSprite(kv.Value, "WorldMap/MianMapBack");
                    }
                }
                if (Main.settings.save_ququ == true)
                {
                    //蛐蛐
                    foreach (var kv in cricketImage)
                    {
                        DoDumpSprite(kv.Value, "Cricket/Cricket");
                    }
                }
                if (Main.settings.save_avatar == true)
                {
                    //脸 k_l_m_n_<sprite序号> , n 在 GetSprites LoadScene读取时候合并了
                    foreach (var kvk in actorFace)
                    {
                        foreach (var kvl in kvk.Value)
                        {
                            foreach (var spriteNameGroup in kvl.Value)
                            {
                                DoDumpSprite(spriteNameGroup, "ActorFace");
                            }
                        }
                    }
                    //脸 k_l_m_n_<sprite序号> , n 在 GetSprites LoadScene读取时候合并了
                    foreach (var kvk in actorFaceSmall)
                    {
                        foreach (var kvl in kvk.Value)
                        {
                            foreach (var spriteNameGroup in kvl.Value)
                            {
                                DoDumpSprite(spriteNameGroup, "ActorFaceSmall");
                            }
                        }
                    }
                    //Child脸 k_l_m_n_<sprite序号> , n 在 GetSprites LoadScene读取时候合并了
                    foreach (var kvk in childFace)
                    {
                        foreach (var kvl in kvk.Value)
                        {
                            DoDumpSprite(kvl.Value, "ChildFace");
                        }
                    }
                    //Child脸 k_l_m_n_<sprite序号> , n 在 GetSprites LoadScene读取时候合并了
                    foreach (var kvk in childFaceSmall)
                    {
                        foreach (var kvl in kvk.Value)
                        {
                            DoDumpSprite(kvl.Value, "ChildFaceSmall");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Main.Logger.Log(e.Message);
                Main.Logger.Log(e.StackTrace);
            }
        }
    }
}
