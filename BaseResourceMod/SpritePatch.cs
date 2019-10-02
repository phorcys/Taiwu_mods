using Harmony12;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BaseResourceMod
{
    /// <summary>
    ///  dump 读取后的图片文件
    ///  加载自定义图像文件
    /// </summary>
    internal static class SpritePatch
    {
        ///<summary>是否已经载入外部贴图</summary>
        private static bool isLoaded = false;

        public static void InjectData()
        {
            if (!Main.enabled)
            {
                return;
            }

            try
            {
                if (Directory.Exists(Main.imgresdir))
                {
                    //遍历 Texture目录子目录
                    foreach (string path in Directory.GetDirectories(Main.imgresdir))
                    {
                        Main.Logger.Log("[Texture] Found subdir : " + path);
                        if (Directory.Exists(path))
                        {
                            SpriteLoadHelper.GetInstance().ProcessDir(path);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Main.Logger.Log(e.ToString() + "  " + e.Message);
                Main.Logger.Log(e.StackTrace);
            }

            try
            {
                // 处理其他MOD的替换请求
                foreach (var kv in Main.mods_sprite_dict)
                {
                    Main.Logger.Log("[Texture] Found Mod subdir : " + kv.Value);
                    if (Directory.Exists(kv.Value))
                    {
                        SpriteLoadHelper.GetInstance().ProcessDir(kv.Value);
                    }
                    else
                    {
                        Main.Logger.Log("[Texture] subdir not exsit : " + kv.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Main.Logger.Log(e.Message);
                Main.Logger.Log(e.StackTrace);
            }
            // 释放资源
            SpriteLoadHelper.ClearInstance();
        }

        /// <summary>
        /// 在打开游戏第一次进入游戏主菜单时替换Sprite,
        /// </summary>
        /// <remarks>这个Patch位点不容易随游戏更新而失效(呵护工具人的肝→_→)</remarks>
        [HarmonyPatch(typeof(MainMenu), "Start")]
        private static class MainMenu_Start_Patch
        {
            private static bool Prefix()
            {
                // 在打开游戏第一次进入游戏主菜单时替换Sprites
                if (Main.enabled && !isLoaded)
                {
                    isLoaded = true;
                    InjectData();
                }
                return true;
            }
        }

        /// <summary>
        /// 从外部载入sprite
        /// </summary>
        /// <param name="path">外部图片路径</param>
        /// <returns>创建好的sprite</returns>
        public static Sprite CreateSpriteFromImage(string path)
        {
            if (!File.Exists(path))
            {
                Main.Logger.Log($"[Texture] Texture file {path}  NOT found.");
                return null;
            }
            byte[] fileData = File.ReadAllBytes(path);
            var toload = new Texture2D(2, 2);
            toload.LoadImage(fileData);
            var newsprite = Sprite.Create(toload, new Rect(0, 0, toload.width, toload.height), new Vector2(0, 0), 100);
#if DEBUG
            Main.Logger.Log($"[Texture] new Texture file {path} loaded, tex size : {toload.width},{toload.height}.");
#endif
            return newsprite;
        }

        [HarmonyPatch(typeof(DynamicSetSprite), "SetImageSprite", typeof(Image), typeof(string))]
        private static class DynamicSetSprite_SetImageSprite_SetExternalSprite
        {
            private static bool Prefix(Image image, string spriteName = "")
            {
                if (!Main.enabled || !Main.customSpriteInfosDic.TryGetValue(spriteName, out var spriteInfo))
                {
                    return true;
                }
                // 将外部贴图缓存，只有当缓存中没有时，才从外部资源创建sprite，避免反复创建相同内容的sprite造成内存泄露问题，并提高响应速度
                Sprite sprite;
                if (spriteInfo.Item2 == null)
                {
                    if (!File.Exists(spriteInfo.Item1))
                        return true;
                    // 若没有外部贴图则缓存则从外部读取
                    sprite = CreateSpriteFromImage(spriteInfo.Item1);
                    // 将外部贴图存入缓存，注意ValueTuple是Value Type
                    Main.customSpriteInfosDic[spriteName] = (spriteInfo.Item1, sprite);
                }
                else
                {
                    // 从贴图缓存载入
                    sprite = spriteInfo.Item2;
                }
                image.sprite = sprite;
                image.enabled = true;
                return false;
            }
        }

        /// <summary>
        /// 每次载入新场景时，清理外部贴图缓存
        /// </summary>
        [HarmonyPatch(typeof(ui_Loading), "Show")]
        private static class Ui_Loading_Show_Patch
        {
            private static bool Prefix()
            {
                // 当游戏载入完毕后，每次进入新的场景时清除原有场景里的外部贴图缓存
                if (Main.enabled && isLoaded && Main.customSpriteInfosDic.Count > 0)
                {
                    foreach (var spriteInfo in Main.customSpriteInfosDic)
                    {
                        if (spriteInfo.Value.Item2 != null)
                        {
                            // 清除缓存中的sprite
                            UnityEngine.Object.Destroy(spriteInfo.Value.Item2);
                        }
                    }
                }
                return true;
            }
        }
    }
}
