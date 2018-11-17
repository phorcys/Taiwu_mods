using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;

/// <summary>
/// 作弊用的mod
/// </summary>
namespace LKX_LoopedStoryInKunlun
{
    /// <summary>
    /// 设置文件
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        public bool activeMod;
        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="modEntry"></param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        /// <summary>
        /// umm日志
        /// </summary>
        public static UnityModManager.ModEntry.ModLogger logger;

        /// <summary>
        /// mod设置
        /// </summary>
        public static Settings settings;

        /// <summary>
        /// 是否开启mod
        /// </summary>
        public static bool enabled;

        /// <summary>
        /// 载入mod。
        /// </summary>
        /// <param name="modEntry">mod管理器对象</param>
        /// <returns></returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.logger = modEntry.Logger;
            Main.settings = Settings.Load<Settings>(modEntry);

            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            string resdir = System.IO.Path.Combine(modEntry.Path, "Data");
            BaseResourceMod.Main.registModResDir(modEntry, resdir);

            return true;
        }

        /// <summary>
        /// 确定是否激活mod
        /// </summary>
        /// <param name="modEntry">umm</param>
        /// <param name="value">是否激活</param>
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }

        /// <summary>
        /// 展示mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUIStyle redLabelStyle = new GUIStyle();
            redLabelStyle.normal.textColor = new Color(225f / 256f, 20f / 256f, 29f / 256f);

            Main.settings.activeMod = GUILayout.Toggle(Main.settings.activeMod, "开启MOD");

            if (GUILayout.Button("卸载MOD数据"))
            {
                if (DateFile.instance.MianActorID() != 0)
                {
                    RemoveStoryForKunLun();
                }
                else
                {
                    Main.logger.Log("没有进入存档，无法卸载！");
                }
            }

            GUILayout.Label("卸载MOD注意事项：", redLabelStyle);
            GUILayout.Label("如果没有点击上方“卸载MOD”按钮去卸载MOD的话，直接删除MOD可能会导致存档出错！", redLabelStyle);
            GUILayout.Label("1、必须进入有昆仑·镜冢的存档。", redLabelStyle);
            GUILayout.Label("2、必须先去掉上面的“开启MOD”的勾，并按下Save保存MOD设置。", redLabelStyle);
            GUILayout.Label("3、点击注意事项上方“卸载MOD”的按钮。", redLabelStyle);
            GUILayout.Label("4、结束时节->存档。", redLabelStyle);
            GUILayout.Label("删除时会在MOD管理器的Logs中记录下本次日志。", redLabelStyle);
        }

        /// <summary>
        /// 保存mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        /// <summary>
        /// 设置昆仑·镜冢的奇遇到太吾村的地图上。
        /// </summary>
        public static void SetStoryForKunLun()
        {
            int partId = int.Parse(DateFile.instance.GetGangDate(16, 3));
            int placeId = int.Parse(DateFile.instance.GetGangDate(16, 4));
            List<int> list = new List<int>(GetTaiWuHomePlaceList(partId, placeId, false));

            if (list.Count > 0)
            {
                int placeId2 = list[UnityEngine.Random.Range(0, list.Count)];
                DateFile.instance.SetStory(true, partId, placeId2, 200001, -1);
            }
            else
            {
                Main.logger.Log("添加失败，太吾村所在地图已经没有位置。");
            }
        }

        /// <summary>
        /// 从太吾村的大地图上删除昆仑·镜冢奇遇
        /// </summary>
        public static void RemoveStoryForKunLun()
        {
            int partId = int.Parse(DateFile.instance.GetGangDate(16, 3));
            int placeId = int.Parse(DateFile.instance.GetGangDate(16, 4));
            List<int> list = new List<int>(GetTaiWuHomePlaceList(partId, placeId));

            if (list.Count > 0)
            {
                int i = 0;
                foreach(int storyKey in list)
                {
                    if (DateFile.instance.worldMapState[partId][storyKey][0] == 200001)
                    {
                        DateFile.instance.SetStory(false, partId, storyKey, 0, -1);
                        i++;
                    }
                }

                if (i > 0)
                {
                    Main.logger.Log("完成卸载！卸载了：" + i.ToString() + "个昆仑·镜冢.");
                    return;
                }
            }

            Main.logger.Log("没有找到昆仑·镜冢，可能已经卸载完成过或者该冢并没有出现过，建议备份存档。");
        }

        /// <summary>
        /// 获取太吾村大地图周围的格子
        /// </summary>
        /// <param name="partId">大地图ID</param>
        /// <param name="placeId">太吾村位置</param>
        /// <param name="isStory">是否包含奇遇的格子</param>
        /// <returns>地图位置</returns>
        public static List<int> GetTaiWuHomePlaceList(int partId, int placeId, bool isStory = true)
        {
            List<int> list = new List<int>();
            List<int> list2 = new List<int>(DateFile.instance.GetWorldMapNeighbor(partId, placeId, 1));
            List<int> list3 = new List<int>(DateFile.instance.GetWorldMapNeighbor(partId, placeId, 100));
            for (int j = 0; j < list3.Count; j++)
            {
                int num2 = list3[j];
                bool flag;
                if (isStory)
                {
                    flag = !list2.Contains(num2) && DateFile.instance.HaveStory(partId, num2) && int.Parse(DateFile.instance.GetNewMapDate(partId, num2, 93)) == 0;
                }
                else
                {
                    flag = !list2.Contains(num2) && !DateFile.instance.HaveStory(partId, num2) && int.Parse(DateFile.instance.GetNewMapDate(partId, num2, 93)) == 0;
                }

                if (flag)
                {
                    list.Add(num2);
                }
            }

            return list;
        }

        /// <summary>
        /// 检查太吾村周围是否有昆仑·镜冢的奇遇
        /// </summary>
        /// <returns></returns>
        public static bool KunLunIsExist()
        {
            int partId = int.Parse(DateFile.instance.GetGangDate(16, 3));
            int placeId = int.Parse(DateFile.instance.GetGangDate(16, 4));
            List<int> list = new List<int>(GetTaiWuHomePlaceList(partId, placeId));

            if (list.Count > 0)
            {
                foreach (int storyKey in list)
                {
                    if (DateFile.instance.worldMapState[partId][storyKey][0] == 200001)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 时节结束时执行检查。
    /// </summary>
    [HarmonyPatch(typeof(UIDate), "DoChangeTrun")]
    public class LKX_LoopedStoryInKunlun_For_UIDate_DoChangeTrun
    {
        static void Prefix()
        {
            if (Main.enabled && Main.settings.activeMod)
            {
                if (DateFile.instance.GetWorldXXLevel() >= 7 && !Main.KunLunIsExist()) DateFile.instance.SetEvent(new int[] { 0, -1, 16785 }, false);
            }
        }
    }

    /// <summary>
    /// 创建战斗人物时，反减掉需要增加的数值。
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MakeXXEnemy")]
    public class LKX_LoopedStoryInKunlun_For_DateFile_MakeXXEnemy
    {
        /// <summary>
        /// 临时用的装备计算
        /// </summary>
        public static int MakeXXEnemyNum;

        static void Prefix(int baseActorId, int index)
        {
            if (!Main.enabled || !Main.settings.activeMod) return;

            if (baseActorId == 18628)
            {
                int num = Mathf.Min(index + 1, 10);
                MakeXXEnemyNum = num;
                for(int i = 0; i < 7; i++)
                {
                    int equipId = int.Parse(DateFile.instance.presetActorDate[18628][304 + i]);
                    if(equipId != 0)
                    {
                        DateFile.instance.presetActorDate[18628][304 + i] = (equipId - num / 2).ToString();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查事件是否进入16781的对话事件。
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "SetEvent")]
    public class LKX_LoopedStoryInKunlun_For_DateFile_SetEvent
    {
        static void Prefix(int[] eventDate)
        {
            if (!Main.enabled || !Main.settings.activeMod) return;

            if (eventDate[2] == 16781)
            {
                LKX_LoopedStoryInKunlun_MassageWindow_EndEvent147_1.RunActorData();
            }
        }
    }

    /// <summary>
    /// 结束事件检查是否进入16785的对话事件
    /// </summary>
    [HarmonyPatch(typeof(MassageWindow), "EndEvent")]
    public class LKX_LoopedStoryInKunlun_For_MassageWindow_EndEvent
    {
        static void Prefix()
        {
            if (!Main.enabled || !Main.settings.activeMod) return;

            if (MassageWindow.instance.eventValue.Count > 0 && MassageWindow.instance.eventValue[0] != 0)
            {
                Main.logger.Log(MassageWindow.instance.eventValue[0].ToString());
                if (MassageWindow.instance.eventValue[0] == 16785)
                {
                    Main.SetStoryForKunLun();
                }
            }
        }
    }

    /// <summary>
    /// 战斗结束后执行事件，把人物装备计算回来，免得对话着装不同。
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem), "SetupBattleEndEvent")]
    public class LKX_LoopedStoryInKunlun_For_BattleSystem_SetupBattleEndEvent
    {
        static void Prefix(ref int ___mianEnemyId)
        {
            if (!Main.enabled || !Main.settings.activeMod) return;
            
            if (int.Parse(DateFile.instance.GetActorDate(___mianEnemyId, 997, false)) == 18628)
            {
                int num = LKX_LoopedStoryInKunlun_For_DateFile_MakeXXEnemy.MakeXXEnemyNum;
                int equipId = int.Parse(DateFile.instance.presetActorDate[18628][305]);
                DateFile.instance.presetActorDate[18628][305] = (equipId + num / 2).ToString();
            }
        }
    }

    /// <summary>
    /// 打Boss开始的事件，包括替换Boss人物模板等。
    /// </summary>
    [HarmonyPatch(typeof(MassageWindow), "EndEvent147_1")]
    public class LKX_LoopedStoryInKunlun_MassageWindow_EndEvent147_1
    {
        /// <summary>
        /// 检查事件的数组1是不是符合对战人物模板的id，是的话拦截，否则放行。
        /// </summary>
        /// <returns></returns>
        static bool Prefix()
        {
            //长度2，index{0:事件id  1:对战人物id}
            int eventIndex = MassageWindow.instance.eventValue[1];
            if (eventIndex == 18628 && Main.enabled && Main.settings.activeMod)
            {
                Event_StartBattleWindow();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 替换队伍的id数据。
        /// </summary>
        public static void Event_StartBattleWindow()
        {
            int teamId = 200001;

            StartBattle.instance.ShowStartBattleWindow(teamId, 0, 18, new List<int> { MassageWindow.instance.eventValue[1] });
        }

        /// <summary>
        /// 执行Boss镜像玩家的数据
        /// </summary>
        public static void RunActorData()
        {
            int id = DateFile.instance.MianActorID();
            Dictionary<int, string> pactor = new Dictionary<int, string>();
            Dictionary<int, string> mianactor = new Dictionary<int, string>();
            if (DateFile.instance.presetActorDate.TryGetValue(18628, out pactor) && DateFile.instance.actorsDate.TryGetValue(DateFile.instance.mianActorId, out mianactor))
            {
                SortedDictionary<int, int[]> mianGongFa = new SortedDictionary<int, int[]>();
                if (DateFile.instance.actorGongFas.TryGetValue(id, out mianGongFa))
                {
                    pactor[906] = GongFaMerge(mianGongFa);
                }
                
                foreach (KeyValuePair<int, string> actor in mianactor)
                {
                    List<int> equip = new List<int> { 304, 305, 306, 307, 308, 309, 310, 311 };
                    if (equip.Contains(actor.Key))
                    {
                        pactor[actor.Key] = DateFile.instance.GetItemDate(int.Parse(actor.Value), 999);
                    }

                    if (pactor.ContainsKey(actor.Key))
                    {

                        List<int> exist = new List<int> {
                            5,11,14,15,17,
                            61,62,63,64,65,66,
                            71,72,73,
                            81,82,83,84,85,86,
                            92,93,94,95,96,97,98,
                            101,
                            501,502,503,504,505,506,507,508,509,510,511,512,513,514,515,516,
                            601,602,603,604,605,606,607,608,609,610,611,612,613,614,
                            651,551,
                            995,996
                        };

                        if (!exist.Contains(actor.Key)) continue;
                        
                        if (actor.Key == 101)
                        {
                            pactor[actor.Key] = "10011|" + actor.Value;
                        }
                        else if(actor.Key == 5)
                        {
                            //暂时0是有名字的
                            pactor[0] = "昆仑镜·" + mianactor[actor.Key];
                            if (mianactor[0] != "NoSurname" || mianactor[0] != "无名" || mianactor[0] != "") pactor[0] = pactor[0] + mianactor[0];
                        }
                        else
                        {
                            pactor[actor.Key] = actor.Value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 执行Boss镜像玩家的功法
        /// </summary>
        /// <param name="gongfa">玩家的功法数据</param>
        /// <returns></returns>
        public static string GongFaMerge(SortedDictionary<int, int[]> gongfa)
        {
            string defaultGongFa = "170010&0";
            foreach(KeyValuePair<int, int[]> item in gongfa)
            {
                if(item.Value[0] >= 25)
                {
                    string addGongFa = "";
                    if(item.Value[2] == 0)
                    {
                        addGongFa = "|" + item.Key.ToString() + "&0";
                    }
                    else if(item.Value[2] <= 5 && item.Value[2] != 0)
                    {
                        addGongFa = "|" + item.Key.ToString() + "&1";
                    }
                    else
                    {
                        addGongFa = "|" + item.Key.ToString() + "&2";
                    }
                    defaultGongFa += addGongFa;
                }
            }

            return defaultGongFa;
        }
    }
}
