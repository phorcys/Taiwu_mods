using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Harmony12;
using UnityModManagerNet;
using System.Reflection;
using Random = UnityEngine.Random;
using GameData;
//using System.IO;

namespace LongDaoMaid
{

    public class Settings : UnityModManager.ModSettings
    {
        public bool stanceChange = true;
        public bool add1s = true;
        public bool sexOrientation = true;
        public int maxAge = 25;
        public int minAge = 16;
        public bool buyMaid = true;
        public int nvZhuang = 0;
        public string getNewSurname = "";
        public string getNewName = "";
        public bool ssr = true;
        public int ssrBoom = 1;

        public int liChang = 2;
        public bool xingGeChange = true;
        public int xingGe;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Settings settings;
        public static bool enabled;
        public static int lastNPCid = -1; //最后生成且还未判断的NPC的id，-1表示无

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value) return false;
            enabled = value;
            return true;
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box");
            GUILayout.Label("<color=#FF6EB4>【龙岛女仆】</color>：龙岛只为你提供女性仆从.");
            GUILayout.Label("<color=#FF6EB4>【慧眼识珠】</color>：你总会挑到好看的仆从.");
            GUILayout.Label("<color=#FF6EB4>【品如衣服】</color>：女仆初始拥有一件下品服装.");
            GUILayout.Label("<color=#FF6EB4>【略通六艺】</color>：女仆资质比常人略高一线.");
            GUILayout.BeginHorizontal("Box");
            settings.add1s = GUILayout.Toggle(settings.add1s, "<color=#FF6EB4>【再续十年】</color> ");
            GUILayout.Label("效果：女仆被龙神赋予额外十年阳寿.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.sexOrientation = GUILayout.Toggle(settings.sexOrientation, "<color=#FF6EB4>【后宫展开】</color> ");
            GUILayout.Label("效果：太吾与女仆同性时，女仆为双性恋，否则为异性恋.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.buyMaid = GUILayout.Toggle(settings.buyMaid, "<color=#FF6EB4>【人口买卖】</color> ");
            GUILayout.Label("效果：获得女仆后，花费9998银钱赎回消耗的地区恩义.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.ssr = GUILayout.Toggle(settings.ssr, "<color=#FF6EB4>【ＳＳＲ！】</color> ");
            string s = GUILayout.TextField(settings.ssrBoom.ToString(), 3, GUILayout.Width(30f));
            int.TryParse(s, out settings.ssrBoom);
            GUILayout.Label("%的爆率爆出剑冢相貌的女仆（无法生育）.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("<color=#FF6EB4>\u3000 【挑老拣少】</color>：你与伏龙坛主商议，只挑走", GUILayout.Width(270f));
            string s2 = GUILayout.TextField(settings.minAge.ToString(), 3, GUILayout.Width(30f));
            int.TryParse(s2, out settings.minAge);

            GUILayout.Label("~", GUILayout.Width(10f));

            string s3 = GUILayout.TextField(settings.maxAge.ToString(), 3, GUILayout.Width(30f));
            int.TryParse(s3, out settings.maxAge);
            GUILayout.Label("岁的女仆");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("<color=#FF6EB4>\u3000 【再生父母】</color>：你为招募到的女仆\u3000赐姓：", GUILayout.Width(280f));

            settings.getNewSurname = GUILayout.TextField(settings.getNewSurname, 5, GUILayout.Width(80f));

            GUILayout.Label("\u3000赐名：", GUILayout.Width(60f));

            settings.getNewName = GUILayout.TextField(settings.getNewName, 5, GUILayout.Width(80f));
            GUILayout.Label("\u3000\u3000\u3000（赐姓赐名皆可选填）");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.stanceChange = GUILayout.Toggle(settings.stanceChange, "<color=#FF6EB4>【朋党之说】</color> ：玩家只招募对应立场的女仆.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            string[] texts = new string[5]
            {
            "<color=#EFF284>刚正</color>",
            "<color=#3987D6>仁善</color>",
            "中庸",
            "<color=#B688DA>叛逆</color>",
            "<color=#FF0000>唯我</color>"
            };
            settings.liChang = GUILayout.SelectionGrid(settings.liChang, texts, 5);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.xingGeChange = GUILayout.Toggle(settings.xingGeChange, "<color=#FF6EB4>【入职培训】</color> ：玩家将对招募来的女仆进行初级调教.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            string[] texts2 = new string[3]
            {
            "随机性格",
            "<color=#3987D6>知书达理</color>",
            "<color=#FF0000>毒舌傲娇</color>"
            };
            settings.xingGe = GUILayout.SelectionGrid(settings.xingGe, texts2, 3);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("\u3000 <color=#FF6EB4>【女装山脉】</color> ：你有时想要些不一样的<color=#FF6EB4>女仆</color>.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            string[] texts3 = new string[4]
            {
            "<color=#FF6EB4>正常女仆</color>",
            "<color=#FF6EB4>女生男相</color>",
            "<color=#3987D6>男生女相</color>",
            "<color=#3987D6>正常男仆</color>"
            };
            settings.nvZhuang = GUILayout.SelectionGrid(settings.nvZhuang, texts3, 4);
            GUILayout.EndHorizontal();
            GUILayout.Label("<color=#FF0000>注意</color>：请在1.0.4及过去版本使用过赐姓功能的存档中点击1次下面的按钮，修复将来赐姓女仆生子时产生的BUG！");
            DateFile instance = DateFile.instance;
            if (instance == null || !Characters.HasChar(instance.mianActorId))
            {
                GUILayout.Label("  存档未载入!");
            }
            else if (GUILayout.Button("重置隐藏姓氏"))
            {
                resetTrueSurName();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 获取新生NPC的ID
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "MakeNewActor")]
        public static class DateFile_MakeNewActor_Patch
        {
            private static void Postfix(DateFile __instance, int __result)
            {
                if (!Main.enabled)
                {
                    return;
                }
                lastNPCid = __result;
                DateFile.instance.ActorFeaturesCacheReset(__result); //刷新特性
                return;
            }
        }

        /// <summary>
        /// 创建NPC之后，显示新相知之前执行的函数，用来修改龙岛忠仆
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "ChangeFavor")]
        public static class DateFile_ChangeFavor_Patch
        {
            private static void Postfix()
            {
                if (!Main.enabled)
                {
                    return;
                }
                if (lastNPCid != -1)
                {
                    if (isLongDaoZhongPu(lastNPCid))
                    {
                        npcChange(lastNPCid);
                    }
                    lastNPCid = -1;
                }
            }
        }

        /// <summary>
        /// 判断指定idNPC是否为龙岛忠仆
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool isLongDaoZhongPu(int id)
        {
            bool flag = false;
            List<int> npcFeature = DateFile.instance.GetActorFeature(lastNPCid);
            for (int i = 0; i < npcFeature.Count; i++)
            {
                if (npcFeature[i] == 4005) //4005为龙岛忠仆特性
                {
                    flag = true;
                }

                if (npcFeature[i] == 1002 | npcFeature[i] == 1001)//石芯玉女 无根之人
                {
                    DateFile.instance.RemoveActorFeature(id,i);
                }
            }
            return flag;
        }

        /// <summary>
        /// 修改指定idNPC数据
        /// </summary>
        /// <param name="id"></param>
        public static void npcChange(int id)
        {
            int mainActorId = DateFile.instance.MianActorID();

            Characters.SetCharProperty(id, 14, DateFile.instance.GetActorDate(id, 14, false));

            //not necessary
            //if (!Characters.HasCharProperty(id, 14))
            //{
            //    Characters.SetCharProperty(id, 14, DateFile.instance.GetActorDate(id, 14, false));
            //}

            if (settings.nvZhuang <= 1) //女生男相也为女
            {
                Characters.SetCharProperty(id, 14, "2");
                if (settings.nvZhuang == 1)
                    Characters.SetCharProperty(id, 17, "1");
                else
                    Characters.SetCharProperty(id, 17, "0");
            }
            else
            {
                Characters.SetCharProperty(id, 14, "1");
                if (settings.nvZhuang == 2)
                    Characters.SetCharProperty(id, 17, "1");
                else
                    Characters.SetCharProperty(id, 17, "0");
            }

            List<int> actorFeature = DateFile.instance.GetActorFeature(id);
            for (int i = 0; i < actorFeature.Count; i++)
            {
                if ((actorFeature[i] == 1002) | (actorFeature[i] == 1001))
                {
                    DateFile.instance.RemoveActorFeature(id, i);
                }
                if (settings.xingGeChange && settings.xingGe >= 43 && settings.xingGe <= 48)
                {
                    DateFile.instance.RemoveActorFeature(id, i);
                }
            }

            //1.龙岛女仆的处世立场与玩家获得女仆时的立场相同
            if (settings.stanceChange == true)
                GameData.Characters.SetCharProperty(id, 16, GameData.Characters.GetCharProperty(mainActorId, 16)); //修改立场

            //2.每个龙岛女仆都被龙神赋予更多的阳寿。因为自动调整过年龄，所以增加10年补贴可能的亏损。
            int countTemp13 = Convert.ToInt32(GameData.Characters.GetCharProperty(mainActorId, 13));
            int countTemp12 = Convert.ToInt32(GameData.Characters.GetCharProperty(mainActorId, 12));
            countTemp13 += 10;
            countTemp12 += 10;

            if (settings.add1s == true)
            {
                countTemp13 += 10;
                countTemp12 += 10;
            }
            GameData.Characters.SetCharProperty(id, 13, countTemp13.ToString());
            GameData.Characters.SetCharProperty(id, 12, countTemp12.ToString());

            //3.改变取向
            if (settings.sexOrientation)
            {
                if (DateFile.instance.GetActorDate(id, 14, false) == DateFile.instance.GetActorDate(mainActorId, 14, false))
                    Characters.SetCharProperty(id, 21, "1");
                else
                    Characters.SetCharProperty(id, 21, "0");
            }
            //麻蛋！原来只要不是0（异性恋）就是双性恋！我还以为1是同2是双！原来这游戏没有同！！！

            //4.年龄区间
            if (settings.maxAge > 0 && settings.minAge > 0)
            {
                Characters.SetCharProperty(id, 11, Convert.ToString(Random.Range(settings.minAge, settings.maxAge)));
            }

            //5.人口买卖
            if (settings.buyMaid && Convert.ToInt32(DateFile.instance.GetActorDate(mainActorId, 406)) >= 9998)
            {
                DateFile.instance.baseWorldDate[int.Parse(DateFile.instance.gangDate[14][11])][int.Parse(DateFile.instance.gangDate[14][3])][3] += 300;
                UIDate.instance.ChangeResource(DateFile.instance.mianActorId, 5, -9998);
            }

            //6.再生父母
            if (settings.getNewSurname != "")
            {
                Characters.SetCharProperty(id, 5, settings.getNewSurname);
                Characters.SetCharProperty(id, 29, settings.getNewSurname);
            }

            if (settings.getNewName != "")
                Characters.SetCharProperty(id, 0, settings.getNewName);

            //慧眼识珠
            //0|鼻子|特征|眼睛|眉毛|嘴|胡子|头发
            string rdHair = Convert.ToString(Random.Range(0, 54));
            string rdNose = Convert.ToString(Random.Range(30, 44));
            string rdSign = Convert.ToString(Random.Range(20, 29));
            string rdEyes = Convert.ToString(Random.Range(30, 44));
            string rdBrow = Convert.ToString(Random.Range(30, 44));
            string rdMouse = Convert.ToString(Random.Range(30, 44));

            Characters.SetCharProperty(id, 995, "0" + "|" + rdNose + "|" + rdSign + "|" + rdEyes + "|" + rdBrow + "|" + rdMouse + "|" + "0" + "|" + rdHair);
            if (Random.Range(1, 4) != 1)
                Characters.SetCharProperty(id, 996,
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)));//75%概率，发肤颜色取前5

            //重新roll魅力
            Characters.SetCharProperty(id, 15, Convert.ToString(Random.Range(501,900)));

            if (Random.Range(1, 10) == 1)//再roll，10%概率发肤色与太吾一致，好歹给黑妹留点机会
                Characters.SetCharProperty(id, 996, DateFile.instance.GetActorDate(mainActorId, 996));

            //List<int> love = DateFile.instance.GetActorSocial(id, 312, false, false);

            //资质均衡
            Characters.SetCharProperty(id, 551, "2");
            Characters.SetCharProperty(id, 651, "2");

            //挑选最出众的资质
            int yiID = 501;
            for (int j = 0; j < 16; j++)
            {
                if (int.Parse(DateFile.instance.GetActorDate(id, yiID)) < int.Parse(DateFile.instance.GetActorDate(id, 501 + j)))
                {
                    yiID = 501 + j;
                }
            }
            int wuID = 604;
            for (int k = 0; k < 11; k++)
            {
                if (int.Parse(DateFile.instance.GetActorDate(id, wuID)) < int.Parse(DateFile.instance.GetActorDate(id, 604 + k)))
                {
                    wuID = 604 + k;
                }
            }

            //略微强化资质
            if (int.Parse(DateFile.instance.GetActorDate(id, yiID)) <= 100)
                Characters.SetCharProperty(id, yiID, Convert.ToString(Convert.ToInt32(DateFile.instance.GetActorDate(id, yiID)) + 10));
            if (int.Parse(DateFile.instance.GetActorDate(id, wuID)) <= 100)
                Characters.SetCharProperty(id, wuID, Convert.ToString(Convert.ToInt32(DateFile.instance.GetActorDate(id, wuID)) + 10));
            if (int.Parse(DateFile.instance.GetActorDate(id, 601)) <= 100)
                Characters.SetCharProperty(id, 601, Convert.ToString(Convert.ToInt32(DateFile.instance.GetActorDate(id, 601)) + 10));
            if (int.Parse(DateFile.instance.GetActorDate(id, 602)) <= 100)
                Characters.SetCharProperty(id, 602, Convert.ToString(Convert.ToInt32(DateFile.instance.GetActorDate(id, 602)) + 10));
            if (int.Parse(DateFile.instance.GetActorDate(id, 603)) <= 100)
                Characters.SetCharProperty(id, 603, Convert.ToString(Convert.ToInt32(DateFile.instance.GetActorDate(id, 603)) + 10));
            
            int rdyiID = UnityEngine.Random.Range(501, 516);
            if (int.Parse(DateFile.instance.GetActorDate(id, rdyiID)) <= 100)
            {
                Characters.SetCharProperty(id, rdyiID, Convert.ToString(Convert.ToInt32(DateFile.instance.GetActorDate(id, rdyiID)) + 10));
            }

            //SSR
            if (settings.ssr && Random.Range(1, 100) <= settings.ssrBoom)
            {
                Characters.SetCharProperty(id, 995, Convert.ToString(UnityEngine.Random.Range(2001, 2010)));
                if (DateFile.instance.GetActorDate(id, 14) == "2")
                    DateFile.instance.AddActorFeature(id, 1002);
                else
                    DateFile.instance.AddActorFeature(id, 1001);
            }

            //给衣服
            //73601——73703 73801——73809
            int newItem;
            if (Random.Range(1, 2) == 1)
            {
                newItem = 73000 + Random.Range(1, 3) + Random.Range(6, 7) * 100;
            }
            else
            {
                newItem = 73800 + Random.Range(1, 9);
            }
            Characters.SetCharProperty(id, 305, DateFile.instance.MakeNewItem(newItem).ToString());

            DateFile.instance.ActorFeaturesCacheReset(id); //刷新特性
        }

        public static void resetTrueSurName()
        {
            Logger.Log("开始重置真实姓氏");
            int[] allCharIds = Characters.GetAllCharIds();
            foreach (int charId in allCharIds)
            {
                if (Characters.HasCharProperty(charId, 29) && !IsNumber(Characters.GetCharProperty(charId, 29)))
                {
                    Characters.RemoveCharProperty(charId, 29);
                }
            }
            Logger.Log("重置完毕！");
        }

        public static bool IsNumber(string str)
        {
            bool result;
            if (str == null || str.Length == 0)
            {
                result = false;
            }
            else
            {
                byte[] bytes = new ASCIIEncoding().GetBytes(str);
                foreach (byte b in bytes)
                {
                    if (b < 48 || b > 57)
                    {
                        return false;
                    }
                }
                result = true;
            }
            return result;
        }
    }
}