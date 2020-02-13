using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Harmony12;
using UnityEngine;
using UnityEngine.UI;

namespace GayMax
{
    //事件用目录
    //意识渐渐模糊……
    struct Index
    {
        //public static Dictionary<int, string> PicIndex = new Dictionary<int, string>();

        public static Dictionary<int, int>
            TurnEvenIndex = new Dictionary<int, int>(),
            EventIndex = new Dictionary<int, int>(),
            GongFaIndex = new Dictionary<int, int>();

        public static Dictionary<int, List<int>>
            GongFaPowerIndex = new Dictionary<int, List<int>>();
    }

    //你已经是一个成熟的MOD，该学会自己读写目录了
    public static class Diction
    {
        public static Dictionary<int, string> Read(string path) //读txt文件 返回字典
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            var dic = new Dictionary<int, string>();
            while ((line = sr.ReadLine()) != null)
            {
                string[] li = line.Split(','); //将一行用,分开成键值对
                if (li.Length >= 2) dic.Add(int.Parse(li[0]), li[1]);
            }

            Main.Logger.Log("载入指派图片目录完成，找到" + dic.Count + "个指派任务");
            return dic;
        }

        public static List<string> Read2(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            List<string> dic = new List<string>();
            while ((line = sr.ReadLine()) != null)
            {
                if (line != "") dic.Add(line);
            }

            return dic;
        }

        public static void Write(string path, Dictionary<string, string> mydic) //将字典写入txt
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            foreach (var d in mydic)
            {
                sw.Write(d.Key + "," + d.Value); //键值对写入，用逗号隔开
            }

            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }
    }

    public static class SevenNineLove
    {
        public static Dictionary<int, string> Transpic(Dictionary<int, string> piclist, Dictionary<int, int> index)
        {
            Dictionary<int, string> trans = new Dictionary<int, string>();
            foreach (int id in piclist.Keys)
            {
                if (index.Keys.Contains(id))
                {
                    trans.Add(index[id], piclist[id]);
                }
            }

            return trans;
        }

        //为事件指定图片
        public static void DoImagine(string mianpath, Dictionary<int, string> piclist, ref Sprite[] SpritesList,
            ref Dictionary<int, Dictionary<int, string>> DateList, int picindex)
        {
            var ima = SpritesList;
            if (ima == null || ima.Length == 0) return;
            foreach (int id in piclist.Keys)
            {
                string path = Path.Combine(mianpath, piclist[id]);
                if (!File.Exists(path))
                {
                    Main.Logger.Log("错误：图片不存在，请检查路径与文件名是否正确");
                    continue;
                }

                var fileData = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0),
                    100);
                if (sprite != null)
                {
                    var images = SpritesList;
                    SpritesList = images.AddToArray(sprite);
                    int num = SpritesList.Length - 1;

                    Main.Logger.Log("找到图片ID：" + num);
                    if (DateList.ContainsKey(id))
                    {
                        DateList[id][picindex] = num.ToString();
                        Main.Logger.Log("指派事件：" + id + "--->图片ID" + num + "，成功");
                    }
                    else Main.Logger.Log("错误：事件不存在！");
                }
                else Main.Logger.Log("错误：图片资源读取失败");
            }
        }

        //读入事件专用（好烦啊不想改了）
        public static Dictionary<int, int> LoadEventDate(string mianpath, string txtname)
        {
            string path = Path.Combine(mianpath, txtname);
            string text;
            Dictionary<int, int> IdChange = new Dictionary<int, int>();
            if (DateFile.instance.eventDate == null) return IdChange;
            if (File.Exists(path))
            {
                text = File.OpenText(path).ReadToEnd();
            }
            else
            {
                Main.Logger.Log("加载事件资源失败！");
                return IdChange;
            }

            //整个事件这样
            int count = 0;
            int count2 = 0;
            string[] EvevtAllString = text.Replace("\r", "").Split("\n"[0]);
            //事件写入的Index这样
            string[] EventIndexs = EvevtAllString[0].Split(',');
            //已有事件的id合集
            List<int> Eventids_EX = new List<int>(DateFile.instance.eventDate.Keys);
            int Maxid = Eventids_EX[0];
            foreach (int i in Eventids_EX)
            {
                if (i > Maxid) Maxid = i;
            }

            Main.Logger.Log("Maxid=" + Maxid);
            //maxid:已有事件最大ID
            //dic{读取的事件ID，动态id}

            for (int i = 1; i < EvevtAllString.Length; i++)
            {
                int id;
                if (int.TryParse(EvevtAllString[i].Split(',')[0], out id))
                    IdChange.Add(id, Maxid + id);
            }


            Dictionary<int, Dictionary<int, string>> NewEvents = new Dictionary<int, Dictionary<int, string>>();
            for (int i = 1; i < EvevtAllString.Length; i++)
            {
                //每一条事件的数据拆分
                string[] EventBody = EvevtAllString[i].Split(',');
                var ExId = EventBody[0];
                if (ExId != "#" && ExId != "")
                {
                    string[] EventBranchs = EventBody[6].Split('|');
                    //处理子事件
                    if (EventBranchs[0] != "")
                    {
                        for (int m = 0; m < EventBranchs.Length; m++)
                        {
                            foreach (int x in IdChange.Keys)
                            {
                                if (EventBranchs[m].Equals(x.ToString()))
                                {
                                    EventBranchs[m] = IdChange[x].ToString();
                                    break;
                                }
                            }
                        }

                        //写入替换的子事件
                        string s = EventBranchs[0];
                        for (int m = 1; m < EventBranchs.Length; m++)
                        {
                            s = s + '|' + EventBranchs[m];
                        }

                        EventBody[6] = s;
                    }

                    //处理跳转事件
                    if (EventBody[8] != "" && EventBody[8] != "-1")
                    {
                        var id = EventBody[8];
                        foreach (int x in IdChange.Keys)
                        {
                            if (id.Equals(x.ToString()))
                            {
                                EventBody[8] = IdChange[x].ToString();
                                break;
                            }
                        }
                    }

                    int exid2 = int.Parse(ExId);
                    Dictionary<int, string> EventBodyDic = new Dictionary<int, string>();
                    for (int j = 0; j < EventIndexs.Length; j++)
                    {
                        //#-ID，0-备注
                        if (EventIndexs[j] != "#" && EventIndexs[j] != "" && int.Parse(EventIndexs[j]) != 0)
                        {
                            EventBodyDic.Add(int.Parse(EventIndexs[j]), Regex.Unescape(EventBody[j]));
                        }
                    }

                    NewEvents.Add((exid2 + Maxid), EventBodyDic);
                    count++;
                }
            }

            if (!IndexCheck("eventDate", NewEvents, DateFile.instance.eventDate))
            {
                Main.Logger.Log("加载事件失败");
                return null;
            }

            lock (DateFile.instance.eventDate)
            {
                foreach (int id in NewEvents.Keys)
                {
                    if (!DateFile.instance.eventDate.Keys.Contains(id))
                    {
                        DateFile.instance.eventDate.Add(id, NewEvents[id]);
                        count2++;
                    }
                    else Main.Logger.Log("错误： ID重复:" + (id - Maxid));
                }
            }

            Main.Logger.Log("找到共" + count + "个事件，成功载入" + count2 + "个事件。");
            return IdChange;
        }

        public static int GetMaxid(Dictionary<int, Dictionary<int, string>> DateList, int limit = -1)
        {
            int Maxid = 0;
            List<int> Eventids_EX = new List<int>(DateList.Keys);
            if (Eventids_EX.Count > 0)
            {
                if (limit < 0)
                {
                    Maxid = Eventids_EX[0];
                    foreach (int id in Eventids_EX)
                    {
                        if (id > Maxid) Maxid = id;
                    }
                }
                else
                {
                    foreach (int id in Eventids_EX)
                    {
                        if (id > Maxid && id <= limit) Maxid = id;
                    }
                }
            }

            return Maxid;
        }

        //79我恨你……
        //读取基础数据，out id=原始id的字典or直接改了
        //其实就是照抄茄茄朴素的读取方式啦
        public static bool LoadBaseDate(string mianpath, string txtname,
            out Dictionary<int, Dictionary<int, string>> DateList, int passDateIndex = -1)
        {
            DateList = new Dictionary<int, Dictionary<int, string>>();
            if (GetSprites.instance == null) return false;
            string path = Path.Combine(mianpath, txtname);
            FieldInfo textColor0 = typeof(GetSprites).GetField("textColor",
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<int, Dictionary<int, string>> textColor =
                (Dictionary<int, Dictionary<int, string>>) textColor0.GetValue(GetSprites.instance);
            textColor0.SetValue(GetSprites.instance, textColor);
            List<int> textColorKeys = new List<int>(textColor.Keys);

            string text;
            if (!File.Exists(path))
            {
                return false;
            }

            text = File.OpenText(path).ReadToEnd();
            string[] lineArray = text.Replace("\r", "").Split("\n"[0]);
            string[] lineIndex = lineArray[0].Split(',');
            for (int i = 1; i < lineArray.Length; i++)
            {
                string[] dateBody = lineArray[i].Split(',');
                var id = dateBody[0];
                if (id != "#" && id != "")
                {
                    Dictionary<int, string> dateBodydic = new Dictionary<int, string>();
                    for (int j = 0; j < lineIndex.Length; j++)
                    {
                        bool flag3 = lineIndex[j] != "#" && lineIndex[j] != "" &&
                                     int.Parse(lineIndex[j]) != passDateIndex;
                        if (flag3)
                        {
                            bool flag4 = dateBody[j].Contains("C_");
                            if (flag4)
                            {
                                for (int k = 0; k < textColorKeys.Count; k++)
                                {
                                    dateBody[j] = dateBody[j].Replace("C_" + textColorKeys[k],
                                        textColor[textColorKeys[k]][0]);
                                }

                                dateBody[j] = dateBody[j].Replace("C_D", "</color>");
                            }

                            dateBodydic.Add(int.Parse(lineIndex[j]), Regex.Unescape(dateBody[j]));
                        }
                    }

                    DateList.Add(int.Parse(id), dateBodydic);
                }
            }

            return true;
        }

        public static Dictionary<int, List<int>> LoadGongFaPower(string mianpath, string name, string antiname,
            bool setantipower)
        {
            Dictionary<int, Dictionary<int, string>> power = new Dictionary<int, Dictionary<int, string>>();
            Dictionary<int, Dictionary<int, string>> antipower = new Dictionary<int, Dictionary<int, string>>();
            Dictionary<int, List<int>> changeid = new Dictionary<int, List<int>>();
            if (LoadBaseDate(mianpath, name, out power) &&
                (!setantipower || LoadBaseDate(mianpath, antiname, out antipower)))
            {
                Dictionary<int, Dictionary<int, string>> oldDate = new Dictionary<int, Dictionary<int, string>>();
                foreach (var dictionary in DateFile.instance.gongFaFPowerDate.Values)
                {
                    if (dictionary.Keys.Contains(97))
                    {
                        oldDate.Add(1, dictionary);
                        break;
                    }
                }

                if (!IndexCheck("gongFaFPowerDate", power, oldDate) ||
                    !IndexCheck("gongFaFPowerDate", antipower, oldDate))
                {
                    Main.Logger.Log("载入功法效果失败");
                    return changeid;
                }

                int maxid = GetMaxid(DateFile.instance.gongFaFPowerDate, 5000);
                lock (DateFile.instance.gongFaFPowerDate)
                {
                    foreach (int id in power.Keys)
                    {
                        DateFile.instance.gongFaFPowerDate.Add(id + maxid, power[id]);
                        changeid.Add(id, new List<int> {id + maxid, 0});
                        if (antipower.Keys.Contains(id))
                        {
                            DateFile.instance.gongFaFPowerDate.Add(id + maxid + 5000, antipower[id]);
                            changeid[id][1] = id + maxid + 5000;
                        }
                    }
                }
            }
            else Main.Logger.Log("错误：载入" + name + "&" + antiname + "失败，请检查文件名是否正确");

            return changeid;
        }

        public static Dictionary<int, int> LoadGongFa(string mianpath, string name, Dictionary<int, List<int>> power,
            int baseid = 0)
        {
            Dictionary<int, Dictionary<int, string>> gongfa = new Dictionary<int, Dictionary<int, string>>();
            Dictionary<int, int> changeid = new Dictionary<int, int>();
            if (LoadBaseDate(mianpath, name, out gongfa))
            {
                if (!IndexCheck("gongFaDate", gongfa, DateFile.instance.gongFaDate))
                {
                    Main.Logger.Log("载入功法失败");
                    return changeid;
                }

                int maxid = 0;
                if (baseid == 0)
                {
                    maxid = GetMaxid(DateFile.instance.gongFaDate);
                }
                else maxid = baseid;

                lock (DateFile.instance.gongFaFPowerDate)
                {
                    foreach (int gongid in gongfa.Keys)
                    {
                        if (power.Keys.Contains(gongid))
                        {
                            gongfa[gongid][103] = power[gongid][0].ToString();
                            gongfa[gongid][104] = power[gongid][1].ToString();
                            Main.Logger.Log("载入功法：" + gongfa[gongid][0] + " 功法Id：" + (gongid + maxid) + " 正练效果:" +
                                            power[gongid][0] + " 逆练效果：" + power[gongid][1]);
                        }

                        DateFile.instance.gongFaDate.Add(gongid + maxid, gongfa[gongid]);
                        changeid.Add(gongid, gongid + maxid);
                    }
                }
            }
            else Main.Logger.Log("错误：载入" + name + "失败，请检查文件名是否正确");

            return changeid;
        }

        public static Dictionary<int, int> LoadOtherDate(string mianpath, string name,
            ref Dictionary<int, Dictionary<int, string>> DateList, int preindex = -1, bool indexcheck = false)
        {
            Dictionary<int, Dictionary<int, string>> data = new Dictionary<int, Dictionary<int, string>>();
            Dictionary<int, int> changeid = new Dictionary<int, int>();
            if (LoadBaseDate(mianpath, name, out data, preindex))
            {
                if (IndexCheck(name, data, DateList) || !indexcheck)
                {
                    int maxid = GetMaxid(DateList);
                    lock (DateList)
                    {
                        foreach (int id in data.Keys)
                        {
                            DateList.Add(id + maxid, data[id]);
                            changeid.Add(id, id + maxid);
                        }
                    }
                }
            }
            else Main.Logger.Log("错误：载入" + name + "失败，请检查文件名是否正确");

            return changeid;
        }

        public static bool IndexCheck(string datename, Dictionary<int, Dictionary<int, string>> NewDate,
            Dictionary<int, Dictionary<int, string>> OldDate)
        {
            if (NewDate.Keys.Count == 0 || OldDate.Keys.Count == 0)
            {
                Main.Logger.Log("错误：文件尚未读取，或参数输入错误");
                return false;
            }

            List<int> OK = new List<int>(OldDate.FirstOrDefault().Value.Keys);
            List<int> NK = new List<int>(NewDate.FirstOrDefault().Value.Keys);
            string[] error = OK.Except(NK).Select(n => Convert.ToString(n)).ToArray();
            if (error.Length == 0) return true;
            var text = string.Join(",", error);
            Main.Logger.Log("错误：" + datename + "中目录不正确，以下目录缺失：" + text);

            return false;
        }

        public static Dictionary<string, Sprite> GayMaxSpriteCache = new Dictionary<string, Sprite>();

        public static Sprite CreateSpriteFromImage(string path) //读取外部图片
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
            Main.Logger.Log($"[Texture] new Texture file {path} loaded, tex size : {toload.width},{toload.height}.");
            return newsprite;
        }

        public static List<string> turnEventSpriteNameList = new List<string>();

        public static void 绑定过月事件和图片(string turnEventImageName, Dictionary<int, string> turnEventDateValue,
            int turnEventIndex)
        {
            var dynamicSetSprite = SingletonObject.getInstance<DynamicSetSprite>();
            var getSpritesInfoAsset =
                (GetSpritesInfoAsset) Traverse.Create(dynamicSetSprite).Field("gsInfoAsset").GetValue();

            int spriteId = 0;

            string spritePath = Path.Combine(Main.picpath, turnEventImageName);
            string spriteName = Path.GetFileNameWithoutExtension(spritePath);

            if (getSpritesInfoAsset.trunEventImage.Last() != spriteName)
            {
                GetSpritesInfoAsset.SingleSpritePathInfo spritePathInfo = new GetSpritesInfoAsset.SingleSpritePathInfo
                {
                    name = spriteName,
                    path = spritePath
                };
                getSpritesInfoAsset.singleSpritePathInfos.Add(spritePathInfo);
                Traverse.Create(getSpritesInfoAsset).Method("InitSingleSpritePathes").GetValue();

                getSpritesInfoAsset.trunEventImage =
                    getSpritesInfoAsset.trunEventImage.AddToArray(spriteName);

                var commonNameGroup =
                    (Dictionary<string, string[]>) Traverse.Create(getSpritesInfoAsset).Field("commonNameGroup")
                        .GetValue();
                commonNameGroup["trunEventImage"] = getSpritesInfoAsset.trunEventImage;
                spriteId = getSpritesInfoAsset.trunEventImage.Length - 1;
                Main.Logger.Log("加入过月事件图片：" + turnEventImageName + " 图片Id：" + spriteId);

                turnEventSpriteNameList.Add(spriteName);
                GayMaxSpriteCache[spriteName] = CreateSpriteFromImage(spritePath); //将外部图片载入缓存
            }

            if (spriteId == 0)
            {
                spriteId = getSpritesInfoAsset.trunEventImage.Length - 1;
            }

            if (turnEventDateValue.Keys.Contains(98))
            {
                turnEventDateValue[98] = spriteId.ToString();
            }
            else
            {
                Main.Logger.Log("加入过月事件失败");
            }


            int turnEventId = DateFile.instance.trunEventDate.Keys.Max() + 1;
            DateFile.instance.trunEventDate.Add(turnEventId, turnEventDateValue);
            Index.TurnEvenIndex[turnEventIndex] = turnEventId;

            Main.Logger.Log("加入过月事件：" + turnEventDateValue[0] + " 事件Id：" + turnEventId + " 图片Id：" +
                            turnEventDateValue[98]);
        }


        public static bool IsRegistered()
        {
            var dynamicSetSprite = SingletonObject.getInstance<DynamicSetSprite>();
            var getSpritesInfoAsset =
                (GetSpritesInfoAsset) Traverse.Create(dynamicSetSprite).Field("gsInfoAsset").GetValue();
            if (getSpritesInfoAsset.trunEventImage.Contains("StarFall_1"))
            {
                return true;
            }

            return false;
        }

        public static void Register()
        {
            Dictionary<int, Dictionary<int, string>> rawTurnEventDate = new Dictionary<int, Dictionary<int, string>>();
            LoadBaseDate(Main.txtpath, "GayMax_TrunEvent_Date.txt", out rawTurnEventDate);
            if (!IndexCheck("trunEventDate", rawTurnEventDate, DateFile.instance.trunEventDate))
            {
                Main.Logger.Log("注册过月事件图片失败");
                return;
            }

            List<Dictionary<int, string>> turnEventDateValues = new List<Dictionary<int, string>>();
            foreach (var key in rawTurnEventDate.Keys)
            {
                turnEventDateValues.Add(rawTurnEventDate[key]);
            }

            绑定过月事件和图片("StarFall_1.png", turnEventDateValues[0], 1);
            绑定过月事件和图片("StarFall_1.png", turnEventDateValues[1], 2);
            绑定过月事件和图片("StarFall_2.png", turnEventDateValues[2], 3);
            绑定过月事件和图片("StarFall_3.png", turnEventDateValues[3], 4);
            绑定过月事件和图片("SevenNine_0.png", turnEventDateValues[4], 5);

            /*var dynamicSetSprite = SingletonObject.getInstance<DynamicSetSprite>();
            var getSpritesInfoAsset =
                (GetSpritesInfoAsset) Traverse.Create(dynamicSetSprite).Field("gsInfoAsset").GetValue();*/
        }
    }

    [HarmonyPatch(typeof(DynamicSetSprite), "SetImageSprite", typeof(Image), typeof(string))]
    public static class DynamicSetSprite_SetImageSprite_SetExternalSprite //从缓存中载入图片
    {
        public static bool Prefix(Image image, string spriteName = "")
        {
            if (!Main.enabled || !SevenNineLove.turnEventSpriteNameList.Contains(spriteName))
            {
                return true;
            }

            image.sprite = SevenNineLove.GayMaxSpriteCache[spriteName];
            image.enabled = true;

            return false;
        }
    }
}