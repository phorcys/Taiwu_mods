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
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using LumenWorks.Framework.IO.Csv;

namespace BaseResourceMod
{

    /// <summary>
    ///  dump 读取后的图片文件
    ///  加载自定义图像文件
    /// </summary>
    [HarmonyPatch(typeof(Loading), "LoadScene")]
    public static class Loading_LoadScene_Patch
    {
        public static void replaceCommonSpriteImage(string nkey, string ftype, string ufname)
        {
            Sprite sprite = null;
            var instance = Main.getSpriteRef<Sprite[]>(nkey);
            if (instance != null)
            {
                sprite = instance.FirstOrDefault(o => o.name == ftype);
            }
            if (sprite != null)
            {
                var newsprite = ChangeTexturePartPNG(sprite, ufname);
                if(newsprite != null)
                {
                    int index = Array.FindIndex(instance, row => row.name == ftype);
                    string oldname = sprite.name;
                    sprite.name = oldname + "to_remove";
                    newsprite.name = oldname;
                    instance[index] = newsprite;
                    UnityEngine.Object.Destroy(sprite);
                    
                }
                Main.Logger.Log(String.Format("[Texture] Ijected  sprite from  {0}  to instance {1} sprite name {2}", ufname, nkey, ftype));
            }
            else 
            {
                sprite = AddNewSpriteFromTexturePNG(ufname);
                if (sprite == null)
                {
                    Main.Logger.Log(String.Format("[Texture] failed Ijected  sprite from  {0}  to instance {1} sprite name {2}  sprite: {3}"
                     , ufname, nkey, ftype, sprite == null ? "null" : sprite.ToString()));
                }
                else
                {
                    int index = int.Parse(Regex.Replace(Regex.Replace(ftype, @"[^_]+_", ""), @"_.*", ""));
                    sprite.name = ftype;
                    if (instance != null && index >=0)
                    {
                        while(index < instance.Length +1)
                        {
                            instance.Add(null);
                        }
                        instance[index] = sprite;
                    }
                }
            }
        }

        public static void replaceSpecialSpriteImage(string nkey, string ftype, string ufname)
        {
            Sprite sprite = null;
            Sprite[] instance = null;
            List<Sprite> linstance = null;
            //挨个特殊处理
            if (nkey == "HomeMap/HomeBuildingIcon" && GetSprites.instance.buildingSprites != null)
            {
                sprite = GetSprites.instance.buildingSprites.FirstOrDefault(o => o.name == ftype);
                linstance = GetSprites.instance.buildingSprites;
            }
            else if(nkey == "Cricket/Cricket")
            {
                //蛐蛐....
                string ququcate = Regex.Replace(Regex.Replace(ftype, @"[^_]+_", ""), @"_.*", "");
                int ququcateid = int.Parse(ququcate);
                if(GetSprites.instance.cricketImage!= null && GetSprites.instance.cricketImage.ContainsKey(ququcateid))
                {
                    sprite = GetSprites.instance.cricketImage[ququcateid].FirstOrDefault(o => o.name == ftype);
                    instance = GetSprites.instance.cricketImage[ququcateid];
                }
            }
            else if (nkey == "WorldMap/MianMapBack")
            {
                //地图tile....
                string mapcate = Regex.Replace(Regex.Replace(ftype, @"[^_]+_", ""), @"_.*", "");
                int mapcateid = int.Parse(mapcate);
                if (GetSprites.instance.placeBack != null && GetSprites.instance.placeBack.ContainsKey(mapcateid))
                {
                    sprite = GetSprites.instance.placeBack[mapcateid].FirstOrDefault(o => o.name == ftype);
                    instance = GetSprites.instance.placeBack[mapcateid];
                }
            }
            else if (nkey == "ActorFace")
            {
                //ActorFace....
                int cate_k = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+_\d+_\d+_\d+$", ""), @".*_", ""));
                int cate_l = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+_\d+_\d+$", ""), @".*_", ""));
                int cate_m = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+_\d+$", ""), @".*_", ""));
                int cate_n = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+$", ""), @".*_", ""));

                if (GetSprites.instance.actorFace != null 
                    && GetSprites.instance.actorFace.ContainsKey(cate_k)
                    && GetSprites.instance.actorFace[cate_k].ContainsKey(cate_l)
                    && cate_m < GetSprites.instance.actorFace[cate_k][cate_l].Count && cate_m >=0)
                {
                    sprite = GetSprites.instance.actorFace[cate_k][cate_l][cate_m].FirstOrDefault(o => o.name == ftype);
                    instance = GetSprites.instance.actorFace[cate_k][cate_l][cate_m];
                }
            }
            else if (nkey == "ActorFaceSmall")
            {
                //ActorFaceSmall....
                int cate_k = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+_\d+_\d+_\d+$", ""), @".*_", ""));
                int cate_l = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+_\d+_\d+$", ""), @".*_", ""));
                int cate_m = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+_\d+$", ""), @".*_", ""));
                int cate_n = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+$", ""), @".*_", ""));

                if (GetSprites.instance.actorFaceSmall != null
                    && GetSprites.instance.actorFaceSmall.ContainsKey(cate_k)
                    && GetSprites.instance.actorFaceSmall[cate_k].ContainsKey(cate_l)
                    && cate_m < GetSprites.instance.actorFaceSmall[cate_k][cate_l].Count && cate_m >= 0)
                {
                    sprite = GetSprites.instance.actorFaceSmall[cate_k][cate_l][cate_m].FirstOrDefault(o => o.name == ftype);
                    instance = GetSprites.instance.actorFaceSmall[cate_k][cate_l][cate_m];
                }
            }
            else if (nkey == "ChildFace")
            {
                //ChildFace....
                int cate_k = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+_\d+$", ""), @".*_", ""));
                int cate_l = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+$", ""), @".*_", ""));

                if (GetSprites.instance.childFace != null
                    && GetSprites.instance.childFace.ContainsKey(cate_k)
                    && GetSprites.instance.childFace[cate_k].ContainsKey(cate_l))
                {
                    sprite = GetSprites.instance.childFace[cate_k][cate_l].FirstOrDefault(o => o.name == ftype);
                    instance = GetSprites.instance.childFace[cate_k][cate_l];
                }
            }
            else if (nkey == "ChildFaceSmall")
            {
                //ChildFaceSmall
                int cate_k = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+_\d+$", ""), @".*_", ""));
                int cate_l = int.Parse(Regex.Replace(Regex.Replace(ftype, @"_\d+$", ""), @".*_", ""));

                if (GetSprites.instance.childFaceSmall != null
                    && GetSprites.instance.childFaceSmall.ContainsKey(cate_k)
                    && GetSprites.instance.childFaceSmall[cate_k].ContainsKey(cate_l))
                {
                    sprite = GetSprites.instance.childFaceSmall[cate_k][cate_l].FirstOrDefault(o => o.name == ftype);
                    instance = GetSprites.instance.childFaceSmall[cate_k][cate_l];
                }
            }



            if (sprite != null)
            {
                var newsprite = ChangeTexturePartPNG(sprite, ufname);
                if (newsprite != null)
                {
                    if(instance!= null)
                    {
                        int index = Array.FindIndex(instance, row => row.name == ftype);
                        instance[index] = newsprite;
                    }
                    else if( linstance != null)
                    {
                        int index = linstance.FindIndex( row => row.name == ftype);
                        linstance[index] = newsprite;
                    }

                    string oldname = sprite.name;
                    sprite.name = oldname + "to_remove";
                    newsprite.name = oldname;

                    UnityEngine.Object.Destroy(sprite);

                }
                Main.Logger.Log(String.Format("[Texture] Ijected  sprite from  {0}  to instance {1} sprite name {2}", ufname, nkey, ftype));
            }
            else
            {
                Main.Logger.Log(String.Format("[Texture] failed Ijected  sprite from  {0}  to instance {1} sprite name {2}  sprite: {3}"
                    , ufname, nkey, ftype, sprite == null ? "null" : sprite.ToString()));
            }
        }

        public static void doReplaceSpriteImage(string key, string ftype, string ufname)
        {
            string nkey = key;
            if(key[key.Length-1] == '/')
            {
                nkey = key.Substring(0, key.Length - 1);
            }
            if(Main.sprite_instance_dict.ContainsKey(nkey))
            {
                replaceCommonSpriteImage(nkey, ftype, ufname);
            }
            else
            {
                replaceSpecialSpriteImage(nkey, ftype, ufname);
            }

        }

        public static void processDir(string path)
        {
            //遍历 子目录下所有png
            foreach (string fname in Directory.GetFiles(path, "*.png", SearchOption.AllDirectories))
            {

                string filename = Path.GetFileName(fname);
                string ufname = fname.Replace("\\", "/");
                Main.Logger.Log(String.Format("[Texture] Found {0} in subdir {1}", ufname, path));
                try
                {
                    if (File.Exists(ufname))
                    {
                        string ftype = filename.Replace(".png","");
                        string dir = Path.GetDirectoryName(ufname);
                        string key = Regex.Replace(dir, @"\./Texture/[^/]+/", "");
                        Main.Logger.Log(String.Format("[Texture] sprite type {0} ftype {1} dir {2}  going to parsing {3}", key, ftype,dir, ufname));

                        try
                        {
                            doReplaceSpriteImage(key, ftype, ufname);
                        }
                        catch (Exception e)
                        {
                            Main.Logger.Log(e.ToString() + "  " + e.Message);
                            Main.Logger.Log(e.StackTrace);
                        }
                    }
                    else
                    {
                        Main.Logger.Log(String.Format("[Texture] file not exsit {0} in subdir {1}", ufname, path));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    Debug.Log(e.StackTrace);
                }

            }
        }
        public static Sprite AddNewSpriteFromTexturePNG(string path)
        {
            if (System.IO.File.Exists(path) == false)
            {
                Main.Logger.Log(String.Format("[Texture] Texture file {0}  NOT found.", path));
                return null;
            }
            var fileData = File.ReadAllBytes(path);
            var toload = new Texture2D(2, 2);
            toload.LoadImage(fileData);
            var newsprite = Sprite.Create(toload, new Rect(0, 0, toload.width, toload.height), new Vector2(0, 0), 100);

            Main.Logger.Log(String.Format("[Texture] new Texture file {0} loaded, tex size : {1},{2}.", path, toload.width,toload.height));
            return newsprite;
        }

        public static Sprite ChangeTexturePartPNG(Sprite sprite, string path)
        {
            if (System.IO.File.Exists(path) == false)
            {
                Main.Logger.Log(String.Format("[Texture] Texture file {0} for sprite :{1} NOT found.", path, sprite.name));
                return null;
            }
            int w = (int)sprite.rect.width;
            int h = (int)sprite.rect.height;

            var fileData = File.ReadAllBytes(path);
            var toload = new Texture2D(w,h);
            toload.LoadImage(fileData);
            var newsprite = Sprite.Create(toload, new Rect(0, 0, toload.width, toload.height), new Vector2(0, 0), sprite.pixelsPerUnit);
            return newsprite;
        }

        public static void dumpPNG(Sprite sprite, string path, int index)
        {
            string filepath = System.IO.Path.Combine(Main.backupimgdir, path + "/" + sprite.name+ ".png");
            string dirpath = System.IO.Path.GetDirectoryName(filepath);
            if(System.IO.Directory.Exists(dirpath) == false)
            {
                System.IO.Directory.CreateDirectory(dirpath);
            }
            var img = sprite.texture;
            var fmbak = img.filterMode;
            img.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(img.width,img.height);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            //x y 坐标  y坐标需要变换
            int x = (int) sprite.rect.x;
            int y = (int)sprite.rect.y;
            y = img.height - y - (int)sprite.rect.height;

            Main.Logger.Log(String.Format("[Texture] Blit texture {0},{1} to dest {2},{3} revert x y {4},{5} orignal offset {6},{7} , file {8}  orignal path {9}"
                , img.width, img.height
                , sprite.rect.width, sprite.rect.height
                , x, y
                , sprite.rect.x,sprite.rect.y
                ,filepath
                ,path));
            Graphics.Blit(img, rt);
            Texture2D img2 = new Texture2D((int)sprite.rect.width,(int)sprite.rect.height,TextureFormat.ARGB32,false);
            img2.hideFlags = HideFlags.HideAndDontSave;
            img2.ReadPixels(new Rect(x,y,sprite.rect.width,sprite.rect.height), 0, 0);
            img2.Apply();
            RenderTexture.active = null;
            img.filterMode = fmbak;
            var bytes = ImageConversion.EncodeToPNG(img2);
            System.IO.File.WriteAllBytes(filepath, bytes);
            RenderTexture.ReleaseTemporary(rt);
            UnityEngine.Object.DestroyImmediate(img2);
        }

        public static void doDumpSprite(Sprite[] sprites, string dirpath)
        {
            if (sprites != null && sprites.Length > 0)
            {
                Main.Logger.Log(String.Format("Sprite {0} count: {1} ppu: {2} packed: {3}  packed mode :{4} rect: {5}  texture rect {6} count: {7} texture memory usage :{8}",
                     dirpath,
                     sprites.Length,
                     sprites[0].pixelsPerUnit,
                     sprites[0].packed,
                     (sprites[0].packed ? sprites[0].packingMode.ToString() : ""),
                     sprites[0].rect == null ? "null" : sprites[0].rect.ToString(),
                     sprites[0].textureRect == null ? "null" : sprites[0].textureRect.ToString(),
                     sprites.Length,
                     RenderTexture.totalTextureMemory));
                int counter = 0;
                foreach (Sprite ss in sprites)
                {
                    dumpPNG(ss, dirpath, counter);
                    counter++;
                }
                
            }
            else
            {
                Main.Logger.Log("[Texture] empty  sprites ..." + dirpath);
            }
        }
        public static void DumpCommonSprite()
        {
            try
            {
                if (Main.settings.save_sprite == true)
                {
                    foreach (var kv in Main.sprite_instance_dict)
                    {
                        Sprite[] sprites = Main.getSpriteRef<Sprite[]>(kv.Key);
                        doDumpSprite(sprites, kv.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Main.Logger.Log(e.Message);
                Main.Logger.Log(e.StackTrace);
            }
        }

        public static void DumpSpecialSprite()
        {
            try
            {
                //建筑图标
                if (GetSprites.instance.buildingSprites != null)
                {
                    doDumpSprite(GetSprites.instance.buildingSprites.ToArray(), "HomeMap/HomeBuildingIcon");
                }
                
                if (Main.settings.save_maptile == true && GetSprites.instance.placeBack!=null)
                {
                    //地图
                    foreach (var kv in GetSprites.instance.placeBack)
                    {
                        doDumpSprite(kv.Value, "WorldMap/MianMapBack");
                    }
                }
                if (Main.settings.save_ququ == true && GetSprites.instance.cricketImage != null)
                {
                    //蛐蛐
                    foreach (var kv in GetSprites.instance.cricketImage)
                    {
                        doDumpSprite(kv.Value, "Cricket/Cricket");
                    }
                }
                if (Main.settings.save_avatar == true
                    && GetSprites.instance.actorFace != null
                    && GetSprites.instance.actorFaceSmall != null
                    && GetSprites.instance.childFace != null
                    && GetSprites.instance.childFaceSmall != null
                    )
                {
                    //脸 k_l_m_n_<sprite序号> , n 在 GetSprites LoadScene读取时候合并了
                    foreach (var kvk in GetSprites.instance.actorFace)
                    {
                        foreach (var kvl in kvk.Value)
                        {
                            int m = 0;
                            foreach (var sprites in kvl.Value)
                            {
                                Dictionary<int, Sprite[]> nlist = new Dictionary<int, Sprite[]>();
                                foreach (var ss in sprites)
                                {
                                    string sn = ss.name;
                                    string ns = Regex.Replace(sn, @"[^_]*_\d+_\d+_\d+_", "");
                                    ns = Regex.Replace(ns, @"_\d+$", "");
                                    int z = int.Parse(ns);
                                    if (nlist.ContainsKey(z))
                                    {
                                        nlist[z].Add(ss);
                                    }
                                    else
                                    {
                                        nlist[z] = new Sprite[] { ss };
                                    }
                                }
                                foreach (var kvn in nlist)
                                {
                                    doDumpSprite(sprites, "ActorFace"); //  /ActorFace_" + kvk.Key + "_" + kvl.Key + "_" + m + "_" + kvn.Key + "_");
                                }
                                m++;
                            }
                        }
                    }
                    //脸 k_l_m_n_<sprite序号> , n 在 GetSprites LoadScene读取时候合并了
                    foreach (var kvk in GetSprites.instance.actorFaceSmall)
                    {
                        foreach (var kvl in kvk.Value)
                        {
                            int m = 0;
                            foreach (var sprites in kvl.Value)
                            {
                                Dictionary<int, Sprite[]> nlist = new Dictionary<int, Sprite[]>();
                                foreach (var ss in sprites)
                                {
                                    string sn = ss.name;
                                    string ns = Regex.Replace(sn, @"[^_]*_\d+_\d+_\d+_", "");
                                    ns = Regex.Replace(ns, @"_\d+$", "");
                                    int z = int.Parse(ns);
                                    if (nlist.ContainsKey(z))
                                    {
                                        nlist[z].Add(ss);
                                    }
                                    else
                                    {
                                        nlist[z] = new Sprite[] { ss };
                                    }
                                }
                                foreach (var kvn in nlist)
                                {
                                    doDumpSprite(sprites, "ActorFaceSmall"); // /ActorFace_" + kvk.Key + "_" + kvl.Key + "_" + m + "_" + kvn.Key + "_");
                                }
                                m++;
                            }
                        }
                    }
                    //Child脸 k_l_m_n_<sprite序号> , n 在 GetSprites LoadScene读取时候合并了
                    foreach (var kvk in GetSprites.instance.childFace)
                    {
                        foreach (var kvl in kvk.Value)
                        {
                            doDumpSprite(kvl.Value, "ChildFace"); // /ChildFace_" + kvk.Key + "_" + kvl.Key + "_");
                        }
                    }
                    //Child脸 k_l_m_n_<sprite序号> , n 在 GetSprites LoadScene读取时候合并了
                    foreach (var kvk in GetSprites.instance.childFace)
                    {
                        foreach (var kvl in kvk.Value)
                        {
                            doDumpSprite(kvl.Value, "ChildFaceSmall"); // /ChildFace_" + kvk.Key + "_" + kvl.Key + "_");
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

        public static void post_InjectData()
        {
            if(!Main.enabled)
            {
                return;
            }
            //dump png文件到备份目录
            if (Main.settings.save_sprite == true)
            {
                DumpCommonSprite();
                DumpSpecialSprite();
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
                            processDir(path);
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
                foreach (var kv in Main.mods_sprite_dict)
                {
                    Main.Logger.Log("[Texture] Found Mod subdir : " + kv.Value);
                    if (Directory.Exists(kv.Value))
                    {
                        processDir(kv.Value);
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

}

static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Main.Logger.Log(" Transpiler init codes ");
            var codes = new List<CodeInstruction>(instructions);

            var foundtheforend = false;
            int startIndex = -1;

            //寻找注入点
            //for (int i = 0; i < codes.Count; i++)
            //{
                if (codes[codes.Count -1].opcode == OpCodes.Ret)
                {
                    startIndex = codes.Count -1;
                    foundtheforend = true;
                    Main.Logger.Log(" found the end of the ret , at index: " + startIndex);
                }

            //}


            if (foundtheforend)
            {
                var injectedCodes = new List<CodeInstruction>();

                // 注入 IL code 
                //
                injectedCodes.Add(new CodeInstruction(OpCodes.Call, typeof(Loading_LoadScene_Patch).GetMethod("post_InjectData")));

                codes.InsertRange(startIndex, injectedCodes);
            }
            else
            {
                Main.Logger.Log(" game changed ... this mod failed to find code to patch...");
            }

            //Main.Logger.Log(" dump the patch codes ");

            //for (int i = 0; i < codes.Count; i++)
            //{
            //    Main.Logger.Log(String.Format("{0} : {1}  {2}", i, codes[i].opcode, codes[i].operand));
            //}
            return codes.AsEnumerable();
        }
    }
}
