using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.IO;
using System.Text.RegularExpressions;

namespace Majordomo
{
    public class ResourceDynamicallyLoader
    {
        // name -> identifier
        private static Dictionary<string, string> resources = new Dictionary<string, string>();


        public static bool AppendTurnEventImage(string filePath)
        {
            var images = GetSprites.instance.trunEventImage;
            if (images == null || images.Length == 0)
            {
                Main.Logger.Log("Sprites not loaded yet, cannot append new one.");
                return false;
            }

            var sprite = CreateSpriteFromImage(filePath);
            if (sprite == null)
            {
                Main.Logger.Log($"Failed to create sprite from {filePath}.");
                return false;
            }

            sprite.name = Path.GetFileNameWithoutExtension(filePath);

            GetSprites.instance.trunEventImage = images.AddToArray(sprite);

            var imageId = GetSprites.instance.trunEventImage.Length - 1;
            resources[sprite.name] = imageId.ToString();

            Main.Logger.Log($"Appended sprite {sprite.name}, Id: {imageId}.");
            return true;
        }


        // 添加表格行，添加前进行变量替换
        // 返回添加的行 ID
        public static int AppendTurnEvent(Dictionary<int, string> data)
        {
            Regex rgx = new Regex(@"\${([^}]+)}");

            var rows = DateFile.instance.trunEventDate;
            int newRowId = rows.Keys.Max() + 1;
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

            rows[newRowId] = interpolatedData;

            return newRowId;
        }


        private static Sprite CreateSpriteFromImage(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Main.Logger.Log(String.Format("Texture file not found: {0}.", path));
                return null;
            }

            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100);

            Main.Logger.Log(String.Format("New texture file loaded: {0}, texture size: {1}, {2}.", path, texture.width, texture.height));
            return sprite;
        }
    }
}
