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
    public static class SpritePatch
    {
        // 在打开游戏第一次进入游戏主菜单时替换Sprite
        [HarmonyPatch(typeof(MainMenu), "Start")]
        private static class Loading_LoadingScene_Patch
        {
            public static void PostInjectData()
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

            private static bool Prefix()
            {
                // 在打开游戏第一次进入游戏主菜单时替换Sprites
                if (Main.enabled && !DateFile.instance.openGame)
                {
                    PostInjectData();
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
            Texture2D toload = new Texture2D(2, 2);
            toload.LoadImage(fileData);
            Sprite newsprite = Sprite.Create(toload, new Rect(0, 0, toload.width, toload.height), new Vector2(0, 0), 100);
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
                if (!Main.enabled)
                {
                    return true;
                }

                if (!Main.customSpritePathInfosDic.TryGetValue(spriteName, out var spritePath))
                {
                    return true;
                }

                image.sprite = CreateSpriteFromImage(spritePath);
                image.enabled = true;
                return false;
            }
        }
    }
}
