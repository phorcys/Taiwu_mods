using Harmony12;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Majordomo
{
    public class ResourceLoader
    {
        // name -> identifier
        private static Dictionary<string, string> resources = new Dictionary<string, string>();


        /// <summary>
        /// 往指定的 Sprite 数组中添加新图片，并保存 ID 到资源库中
        /// </summary>
        /// <param name="sprites"></param>
        /// <param name="filePath"></param>
        /// <returns>图片是否添加成功</returns>
        public static bool AppendSprite(ref Sprite[] sprites, string filePath)
        {
            if (sprites == null || sprites.Length == 0)
            {
                Main.Logger.Log("Sprites not loaded yet, cannot append new one.");
                return false;
            }

            var sprite = ResourceLoader.CreateSpriteFromImage(filePath);
            if (sprite == null)
            {
                Main.Logger.Log($"Failed to create sprite from {filePath}.");
                return false;
            }

            sprite.name = Path.GetFileNameWithoutExtension(filePath);

            sprites = sprites.AddToArray(sprite);

            var spriteId = sprites.Length - 1;
            resources[sprite.name] = spriteId.ToString();

            Main.Logger.Log($"Appended sprite {sprite.name}, Id: {spriteId}.");
            return true;
        }


        /// <summary>
        /// 添加表格行，添加前进行变量替换
        /// </summary>
        /// <param name="table"></param>
        /// <param name="data"></param>
        /// <returns>所添加的行 ID</returns>
        public static int AppendRow(Dictionary<int, Dictionary<int, string>> table, Dictionary<int, string> data)
        {
            Regex rgx = new Regex(@"\${([^}]+)}");

            int newRowId = table.Keys.Max() + 1;
            Dictionary<int, string> interpolatedData = new Dictionary<int, string>();

            foreach (int index in data.Keys)
            {
                string oriValue = data[index];
                var sb = new StringBuilder();
                int beginPos = 0;

                foreach (Match match in rgx.Matches(oriValue))
                {
                    string resourceKey = match.Groups[1].Value;

                    string logText = $"Found expression '{match.Value}', ";

                    if (!resources.ContainsKey(resourceKey))
                    {
                        logText += "but did not find the corresponding resource.";
                        Main.Logger.Log(logText);
                        continue;
                    }

                    logText += $"and replaced it with '{resources[resourceKey]}'.";
                    Main.Logger.Log(logText);

                    sb.Append(oriValue.Substring(beginPos, match.Index - beginPos));
                    sb.Append(resources[resourceKey]);
                    beginPos = match.Index + match.Length;
                }

                sb.Append(oriValue.Substring(beginPos, oriValue.Length - beginPos));

                interpolatedData[index] = sb.ToString();
            }

            table[newRowId] = interpolatedData;

            return newRowId;
        }


        public static Sprite CreateSpriteFromImage(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Main.Logger.Log(string.Format("Texture file not found: {0}.", path));
                return null;
            }

            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100);

            Main.Logger.Log(string.Format("New texture file loaded: {0}, texture size: {1}, {2}.", path, texture.width, texture.height));
            return sprite;
        }
    }
}
