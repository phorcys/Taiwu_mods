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
    }

    public static class Main
    {
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

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value) return false;
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("<color=#FF6EB4>【龙岛女仆】</color>：龙岛只为你提供女性仆从.");
            GUILayout.Label("<color=#FF6EB4>【慧眼识珠】</color>：你总会挑到好看的仆从.");
            GUILayout.Label("<color=#FF6EB4>【品如衣服】</color>：女仆初始拥有一件下品服装.");
            GUILayout.Label("<color=#FF6EB4>【略通六艺】</color>：女仆资质比常人略高一线.");
            //GUILayout.Label("<color=#FF6EB4>【】</color>：你不接受石芯玉女.");//想不到起什么名，不显示了！
            //后续待增加功能：暂无

            GUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal("Box");
            settings.stanceChange = GUILayout.Toggle(settings.stanceChange, "<color=#FF6EB4>【革命战友】</color> ");
            GUILayout.Label("效果：处世立场与玩家获得女仆时的立场相同.");
            GUILayout.EndHorizontal();

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
            int.TryParse(GUILayout.TextField(settings.ssrBoom.ToString(), 3, GUILayout.Width(30)), out settings.ssrBoom);
            GUILayout.Label("%的爆率爆出剑冢相貌的女仆（无法生育）.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("　 <color=#FF6EB4>【女装山脉】</color> ：你有时想要些不一样的<color=#FF6EB4>女仆</color>.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.nvZhuang = GUILayout.SelectionGrid(settings.nvZhuang, new string[]
            {
                "<color=#FF6EB4>正常女仆</color>",
                "<color=#FF6EB4>女生男相</color>",
                "<color=#3987D6>男生女相</color>",
                "<color=#3987D6>正常男仆</color>",
            }, 4, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("<color=#FF6EB4>　 【挑老拣少】</color>：你与伏龙坛主商议，只挑走", GUILayout.Width(270));
            int.TryParse(GUILayout.TextField(settings.minAge.ToString(), 3, GUILayout.Width(30)), out settings.minAge);
            GUILayout.Label("~", GUILayout.Width(10));
            int.TryParse(GUILayout.TextField(settings.maxAge.ToString(), 3, GUILayout.Width(30)), out settings.maxAge);
            GUILayout.Label("岁的女仆");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("<color=#FF6EB4>　 【再生父母】</color>：你为招募到的女仆　赐姓：", GUILayout.Width(280));
            settings.getNewSurname = GUILayout.TextField(settings.getNewSurname, 5, GUILayout.Width(80));
            GUILayout.Label("　赐名：", GUILayout.Width(60));
            settings.getNewName = GUILayout.TextField(settings.getNewName, 5, GUILayout.Width(80));
            GUILayout.Label("　　　（赐姓赐名皆可选填）");
            GUILayout.EndHorizontal();
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
                DateFile.instance.actorsFeatureCache.Remove(__result); //刷新特性
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

                if (npcFeature[i] == 1002)//石芯玉女 1001为无根之人
                {
                    npcFeature.Remove(i);
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
            Dictionary<int, string> npc;
            npc = DateFile.instance.actorsDate[id];
            Dictionary<int, string> player;
            player = DateFile.instance.actorsDate[DateFile.instance.mianActorId];

            if (!npc.ContainsKey(14))
            {
                npc.Add(14, DateFile.instance.GetActorDate(id, 14, false));
            }
            if (settings.nvZhuang <= 1)//女生男相也为女
            {
                npc[14] = "2";
                if (settings.nvZhuang == 1) npc[17] = "1";
                else npc[17] = "0";
            }
            else
            {
                npc[14] = "1";
                if (settings.nvZhuang == 2) npc[17] = "1";
                else npc[17] = "0";
            }

            //1.龙岛女仆的处世立场与玩家获得女仆时的立场相同
            if (settings.stanceChange == true)
                npc[16] = player[16]; //修改立场

            //2.每个龙岛女仆都被龙神赋予更多的阳寿。因为自动调整过年龄，所以增加10年补贴可能的亏损。
            int countTemp13 = Convert.ToInt32(npc[13]);
            int countTemp12 = Convert.ToInt32(npc[12]);
            countTemp13 += 10;
            countTemp12 += 10;

            if (settings.add1s == true)
            {
                countTemp13 += 10;
                countTemp12 += 10;
            }
            npc[13] = Convert.ToString(countTemp13);
            npc[12] = Convert.ToString(countTemp12);

            //3.改变取向
            if (settings.sexOrientation == true)
            {
                if (DateFile.instance.GetActorDate(id, 14, false) == npc[14])
                    npc[21] = "1";
                else npc[21] = "0";
            }
            //麻蛋！原来只要不是0（异性恋）就是双性恋！我还以为1是同2是双！原来这游戏没有同！！！

            //4.年龄区间
            if (settings.maxAge > 0 && settings.minAge > 0)
            {
                npc[11] = Convert.ToString(Random.Range(settings.minAge, settings.maxAge));
            }

            //5.人口买卖
            if (settings.buyMaid && Convert.ToInt32(player[406]) >= 9998)
            {
                DateFile.instance.baseWorldDate[int.Parse(DateFile.instance.gangDate[14][11])][int.Parse(DateFile.instance.gangDate[14][3])][3] += 300;
                UIDate.instance.ChangeResource(DateFile.instance.mianActorId, 5, -9998, true);
            }

            //6.再生父母
            if (settings.getNewSurname != "")
            {
                npc[29] = settings.getNewSurname;
                npc[5] = settings.getNewSurname;
            }

            if (settings.getNewName != "")
                npc[0] = settings.getNewName;

            //慧眼识珠
            //0|鼻子|特征|眼睛|眉毛|嘴|胡子|头发
            string rdHair = Convert.ToString(Random.Range(0, 54));
            string rdNose = Convert.ToString(Random.Range(30, 44));
            string rdSign = Convert.ToString(Random.Range(20, 29));
            string rdEyes = Convert.ToString(Random.Range(30, 44));
            string rdBrow = Convert.ToString(Random.Range(30, 44));
            string rdMouse = Convert.ToString(Random.Range(30, 44));

            npc[995] = "0" + "|" + rdNose + "|" + rdSign + "|" + rdEyes + "|" + rdBrow + "|" + rdMouse + "|" + "0" + "|" + rdHair;
            if (Random.Range(1, 4) != 1)
                npc[996] = 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4)) + "|" + 
                    Convert.ToString(Random.Range(0, 4));//75%概率，发肤颜色取前5

            //重新roll魅力
            npc[15] = Convert.ToString(Random.Range(501,900));

            if (Random.Range(1, 10) == 1)//再roll，10%概率发肤色与太吾一致，好歹给黑妹留点机会
                npc[996] = player[996];

            //List<int> love = DateFile.instance.GetActorSocial(id, 312, false, false);

            //资质均衡
            npc[551] = "2";
            npc[651] = "2";

            //挑选最出众的资质
            int yiID = 501;
            for (int i = 0; i < 16; i++)
            {
                if (Int32.Parse(npc[yiID]) < Int32.Parse(npc[501 + i]))
                {
                    yiID = 501 + i;
                }
            }
            int wuID = 604;
            for (int i = 0; i < 11; i++)
            {
                if (Int32.Parse(npc[wuID]) < Int32.Parse(npc[604 + i]))
                {
                    wuID = 604 + i;
                }
            }

            //略微强化资质
            if (int.Parse(npc[yiID]) <= 100)
                npc[yiID] = Convert.ToString(Convert.ToInt32(npc[yiID]) + 10);
            if (int.Parse(npc[wuID]) <= 100)
                npc[wuID] = Convert.ToString(Convert.ToInt32(npc[wuID]) + 10);
            if (int.Parse(npc[601]) <= 100)
                npc[601] = Convert.ToString(Convert.ToInt32(npc[601]) + 10);
            if (int.Parse(npc[602]) <= 100)
                npc[601] = Convert.ToString(Convert.ToInt32(npc[601]) + 10);
            if (int.Parse(npc[603]) <= 100)
                npc[601] = Convert.ToString(Convert.ToInt32(npc[601]) + 10);

            int rdyiID = Random.Range(501, 516);
            if (int.Parse(npc[rdyiID]) <= 100)
                npc[rdyiID] = Convert.ToString(Convert.ToInt32(npc[rdyiID]) + 10);

            //SSR
            if (settings.ssr && Random.Range(1, 100) < settings.ssrBoom)
            {
                npc[995] = Convert.ToString(Random.Range(2001, 2009));
                DateFile.instance.AddActorFeature(id, 1002);
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
            npc[305] = DateFile.instance.MakeNewItem(newItem).ToString();

            DateFile.instance.actorsFeatureCache.Remove(id); //刷新特性
        }
    }
}