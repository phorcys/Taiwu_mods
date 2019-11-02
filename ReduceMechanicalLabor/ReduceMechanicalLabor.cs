using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using System.Linq;

namespace ReduceMechanicalLabor
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool autoRead = true; // 自动读书
        public bool autoBreakThrough = true; // 自动突破
        public bool quickKill = true; // 快速杀敌
        public bool autoSearchSamsara = true; // 自动搜寻轮回台合适人选


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }


    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Main.settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;
            return true;
        }


        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }


        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#87CEEB>此 mod 旨在保留游戏性的前提下减少机械性劳动. 强烈建议每个被简化的功能都至少手动执行过一百次, 再来使用此 mod.</color>\n");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.autoRead = GUILayout.Toggle(Main.settings.autoRead,
                "自动读书 (需难度 50% 及以下, 且耐心和悟性满足需求. 在进入读书界面后自动进行.)");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.autoBreakThrough = GUILayout.Toggle(Main.settings.autoBreakThrough,
                "自动突破 (需难度 100% 及以下. 在关闭突破窗口时自动进行.)");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.quickKill = GUILayout.Toggle(Main.settings.quickKill,
                "快速杀敌 (精纯高于敌方时, 可按 K 键秒杀. 可能会导致 NPC 战后不治身亡.)");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.settings.autoSearchSamsara = GUILayout.Toggle(Main.settings.autoSearchSamsara,
                "自动搜寻轮回台合适人选 (在进入轮回台界面后自动进行. 信息将显示在 UMM 的日志页面.)");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("人口统计")) PopulationStats.Show();
            GUILayout.Label("(点击按钮后, 统计信息将显示在 UMM 的日志页面. 请在存档载入后再进行统计.)");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }
    }


    /// <summary>
    /// 自动搜寻轮回台合适人选
    /// </summary>
    [HarmonyPatch(typeof(ui_SamsaraPlatform), "OnInit")]
    public static class ui_SamsaraPlatform_OnInit_Patch
    {
        /// <summary>
        /// 属性类型
        /// 0: 基础属性, 1: 功法资质, 2: 技艺资质
        /// </summary>
        private static readonly int[] _attrTypes = { 0, 1, 2 };


        /// <summary>
        /// 获取最佳轮回人选
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>各个属性的最佳人选 dataIndex -> (charId, attrValue, deltaBonus)</returns>
        private static SortedDictionary<int, (int, int, int)> GetBestCandidates(ui_SamsaraPlatform instance)
        {
            var candidates = new SortedDictionary<int, (int, int, int)>();

            var allAttrBonuses = GetAllAttrBonuses(instance);
            var buildingLevel = Traverse.Create(instance).Field("buildingLevel").GetValue<int>();
            var mainActorId = DateFile.instance.MianActorID();

            // 已转世的人
            var bornedActorSet = new HashSet<int>();
            foreach (var data in DateFile.instance.samsaraPlatformChildrenData.Values)
                bornedActorSet.Add(data.samsaraActorId);

            foreach (var charId in DateFile.instance.deadActors)
            {
                if (DateFile.instance.GetActorFavor(false, mainActorId, charId) == -1) continue;    // 去掉不认识的人
                if (bornedActorSet.Contains(charId)) continue;  // 去掉已转世的人
                if (int.Parse(DateFile.instance.GetActorDate(charId, 11, false)) <= ConstValue.actorMinAge) continue;   // 去掉未成年人

                foreach (var entry in allAttrBonuses)
                {
                    var dataIndex = entry.Key;
                    var oriBonus = entry.Value;

                    var attrValue = int.Parse(DateFile.instance.GetActorDate(charId, dataIndex, false));
                    int currBonus = attrValue * buildingLevel / 100;
                    if (currBonus <= oriBonus) continue;

                    if (!candidates.ContainsKey(dataIndex) || candidates[dataIndex].Item2 < currBonus)
                        candidates[dataIndex] = (charId, attrValue, currBonus - oriBonus);
                }
            }

            return candidates;
        }


        /// <summary>
        /// 获取目前的所有属性加成, 包括还未有加成的属性
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private static Dictionary<int, int> GetAllAttrBonuses(ui_SamsaraPlatform instance)
        {
            var allAttrBonuses = new Dictionary<int, int>();

            var attrBonuses = DateFile.instance.samsaraPlatformAddAttribute;
            var attrDataStartIndex = Traverse.Create(instance).Field("attrDataStartIndex").GetValue<int[]>();
            var attrDataText = Traverse.Create(instance).Field("attrDataText").GetValue<List<CText>[]>();

            foreach (var attrType in _attrTypes)
            {
                var firstDataIndex = attrDataStartIndex[attrType];
                var attrText = attrDataText[attrType];

                for (var i = 0; i < attrText.Count; ++i)
                {
                    var dataIndex = firstDataIndex + i;
                    var bonus = attrBonuses.ContainsKey(dataIndex) ? attrBonuses[dataIndex] : 0;
                    allAttrBonuses[dataIndex] = bonus;
                }
            }

            return allAttrBonuses;
        }


        private static string GetAttrName(int dataIndex)
        {
            var attrId = (dataIndex >= 61 && dataIndex <= 66) ? dataIndex - 61 : dataIndex;
            return DateFile.instance.actorAttrDate[attrId][0];
        }


        private static void Postfix(ui_SamsaraPlatform __instance)
        {
            if (!Main.enabled || !Main.settings.autoSearchSamsara) return;

            var candidates = GetBestCandidates(__instance);

            // 最佳人选信息 charId -> [(dataIndex, attrValue, deltaBonus), ]
            var candidateChars = new Dictionary<int, List<(int, int, int)>>();

            foreach (var entry in candidates)
            {
                var dataIndex = entry.Key;
                var charId = entry.Value.Item1;
                var attrValue = entry.Value.Item2;
                var deltaBonus = entry.Value.Item3;

                if (!candidateChars.ContainsKey(charId)) candidateChars[charId] = new List<(int, int, int)>();
                candidateChars[charId].Add((dataIndex, attrValue, deltaBonus));
            }

            foreach (var charId in candidateChars.Keys.ToArray())
            {
                var attrbutes = candidateChars[charId];
                attrbutes.Sort((lhs, rhs) => lhs.Item1.CompareTo(rhs.Item1));
            }

            // 输出结果
            if (candidateChars.Count <= 0)
            {
                Main.Logger.Log($"轮回台最佳人选: 找不到合适的人选.");
                return;
            }

            var message = new StringBuilder();

            foreach (var entry in candidateChars)
            {
                var charId = entry.Key;
                var attributesInfo = entry.Value;

                message.Append($"{DateFile.instance.GetActorName(charId, true)}: ");

                foreach (var attrInfo in attributesInfo)
                {
                    var dataIndex = attrInfo.Item1;
                    var attrValue = attrInfo.Item2;
                    var deltaBonus = attrInfo.Item3;

                    message.Append($"{GetAttrName(dataIndex)} {attrValue} (+{deltaBonus}), ");
                }

                message.AppendLine();
            }

            Main.Logger.Log($"轮回台最佳人选:\n{message.ToString()}");
        }
    }


    /// <summary>
    /// 人口统计
    /// </summary>
    public static class PopulationStats
    {
        private static readonly int[] _worldIds = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
        private static readonly int[] _gangIds = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };


        public static void Show()
        {
            var worldStats = ShowWorldStats();
            Main.Logger.Log($"世界人口统计:\n{worldStats}");

            var gangStats = ShowGangStats();
            Main.Logger.Log($"门派人口统计:\n{gangStats}");
        }


        private static string ShowWorldStats()
        {
            var stats = new StringBuilder();
            var nTotalChars = 0;

            var heroTeamCharIds = new HashSet<int>(DateFile.instance.GetFamily(true));
            nTotalChars += heroTeamCharIds.Count;

            foreach (var worldId in _worldIds)
            {
                var name = DateFile.instance.allWorldDate[worldId][0];
                var mapsInfo = DateFile.instance.baseWorldDate[worldId];
                var nChars = 0;

                foreach (var mapsInfoEntry in mapsInfo)
                {
                    var mapId = mapsInfoEntry.Key;
                    var mapInfo = mapsInfoEntry.Value;

                    var partSizeLength = int.Parse(DateFile.instance.partWorldMapDate[mapId][98]); // 地图边长
                    var nMapTiles = partSizeLength * partSizeLength; // 地图中的地块数量

                    for (var tileId = 0; tileId < nMapTiles; ++tileId)
                    {
                        if (!DateFile.instance.worldMapActorDate[mapId].ContainsKey(tileId)) continue;
                        var charIds = DateFile.instance.worldMapActorDate[mapId][tileId];

                        foreach (var charId in charIds)
                        {
                            if (charId < 0) continue;
                            if (int.Parse(DateFile.instance.GetActorDate(charId, 8, false)) != 1) continue;     // 固定属性 NPC
                            if (int.Parse(DateFile.instance.GetActorDate(charId, 26, false)) != 0) continue;    // 已死亡
                            if (int.Parse(DateFile.instance.GetActorDate(charId, 6, false)) != 0) continue;     // 已相枢化
                            if (heroTeamCharIds.Contains(charId)) continue;
                            ++nChars;
                        }
                    }
                }

                stats.AppendLine($"{name}: {nChars}");
                nTotalChars += nChars;
            }

            stats.AppendLine($"<总计>: {nTotalChars}");
            return stats.ToString();
        }


        private static string ShowGangStats()
        {
            var stats = new StringBuilder();
            var nTotalChars = 0;

            foreach (var gangId in _gangIds)
            {
                var name = DateFile.instance.GetGangDate(gangId, 0);
                var partId = int.Parse(DateFile.instance.GetGangDate(gangId, 3));
                var placeId = int.Parse(DateFile.instance.GetGangDate(gangId, 4));
                var nChars = 0;

                var gradesInfo = DateFile.instance.gangGroupDate[partId][placeId];
                foreach (var entry in gradesInfo)
                {
                    var grade = entry.Key;
                    var charIds = entry.Value;
                    foreach (var charId in charIds)
                    {
                        if (int.Parse(DateFile.instance.GetActorDate(charId, 26, false)) != 0) continue;    // 已死亡
                        if (int.Parse(DateFile.instance.GetActorDate(charId, 6, false)) != 0) continue;     // 已相枢化
                        if (int.Parse(DateFile.instance.GetActorDate(charId, 27, false)) == 1) continue;    // 已被劫持
                        ++nChars;
                    }
                }

                stats.AppendLine($"{name}: {nChars}");
                nTotalChars += nChars;
            }

            stats.AppendLine($"<总计>: {nTotalChars}");
            return stats.ToString();
        }
    }


    /// <summary>
    /// 难度 50% 及以下, 且耐心和悟性满足需求时, 自动读书
    /// 已读的情况下也会自动读书
    /// </summary>
    [HarmonyPatch(typeof(ReadBook), "UpdateRead")]
    public static class ReadBook_UpdateRead_Patch
    {
        /// <summary>
        /// 直接成功阅读当前书页
        /// </summary>
        /// <param name="currPage"></param>
        /// <param name="costIntelligence"></param>
        /// <param name="costPatience"></param>
        private static void ReadCurrentPage(int currPage, int costIntelligence, int costPatience)
        {
            // 加上阅读完此页获得的悟性值
            Traverse.Create(ReadBook.instance).Method("UpdateCanUseInt", 20).GetValue();

            Traverse.Create(ReadBook.instance).Method("ChangeReadLevel", 100, true).GetValue();

            Main.Logger.Log($"第 {currPage + 1} 页: 消耗悟性 {costIntelligence}, 消耗耐心 {costPatience}");
        }


        private static bool Prefix()
        {
            if (!Main.enabled || !Main.settings.autoRead) return true;

            int mainActorId = DateFile.instance.MianActorID();
            int skillId = int.Parse(DateFile.instance.GetItemDate(BuildingWindow.instance.readBookId, 32));
            int currPage = Traverse.Create(ReadBook.instance).Field("readPageIndex").GetValue<int>();

            // 已通过读书或战斗研读完此页, 则直接无消耗阅读完
            if (DateFile.instance.gongFaBookPages.ContainsKey(skillId))
            {
                var pageState = DateFile.instance.gongFaBookPages[skillId][currPage];
                if (pageState == 1 || pageState <= -100)
                {
                    ReadCurrentPage(currPage, 0, 0);
                    return false;
                }
            }

            // 获取主角的资质加造诣
            int qualificationAndAttainment;
            if (BuildingWindow.instance.studySkillTyp == 17)
            {
                int bookType = int.Parse(DateFile.instance.gongFaDate[skillId][1]);
                qualificationAndAttainment = DateFile.instance.GetActorValue(mainActorId, 601 + bookType);
            }
            else
            {
                int bookType = int.Parse(DateFile.instance.skillDate[skillId][3]);
                qualificationAndAttainment = DateFile.instance.GetActorValue(mainActorId, 501 + bookType);
            }

            int difficulty = BuildingWindow.instance.GetNeedInt(qualificationAndAttainment, skillId);
            if (difficulty > 50) return true;

            int[] pagesInfo = DateFile.instance.GetBookPage(BuildingWindow.instance.readBookId);
            int costIntelligence = 0;
            int costPatience = 0;

            if (pagesInfo[currPage] == 0) // 残页
            {
                // 前三页用思思惟, 耐心消耗固定
                if (currPage <= 2)
                {
                    costIntelligence = 35;
                    costPatience = 40;
                }
                // 之后用融融惟, 耐心消耗和书页位置有关: x, x, x, 30, 20, 10, 0, 0, 0, 0
                else
                {
                    costIntelligence = 55;
                    costPatience = Math.Max(0, (6 - currPage) * 10);
                }
            }

            if (costIntelligence > 0)
            {
                int currIntelligence = Traverse.Create(ReadBook.instance).Field("canUseInt").GetValue<int>();
                if (currIntelligence <= costIntelligence) return true;

                Traverse.Create(ReadBook.instance).Method("UpdateCanUseInt", -costIntelligence).GetValue();
            }

            if (costPatience > 0)
            {
                int currPatience = Traverse.Create(ReadBook.instance).Field("patience").GetValue<int>();
                if (currPatience <= costPatience) return true;

                Traverse.Create(ReadBook.instance).Method("ChangePatience", -costPatience).GetValue();
            }

            ReadCurrentPage(currPage, costIntelligence, costPatience);
            return false;
        }
    }


    /// <summary>
    /// 难度 100% 及以下时, 在关闭突破窗口时自动突破
    /// </summary>
    [HarmonyPatch(typeof(StudyWindow), "CloseStudyWindowButton")]
    public static class StudyWindow_CloseStudyWindowButton_Patch
    {
        /// <summary>
        /// 自动突破时的最低突破点数
        /// </summary>
        public const int MinBreakThroughPoints = 40;


        /// <summary>
        /// 返回是否容易突破 (角色资质不小于所需的资质)
        /// </summary>
        /// <returns></returns>
        public static bool IsEasyToBreakThrough()
        {
            int mainActorId = DateFile.instance.MianActorID();
            int skillId = BuildingWindow.instance.levelUPSkillId;
            int charQualification, requiredQualification;

            if (BuildingWindow.instance.studySkillTyp == 17)
            {
                var skillGrade = DateFile.instance.GetGongFaLevel(mainActorId, skillId);
                var bookType = int.Parse(DateFile.instance.gongFaDate[skillId][1]);
                charQualification = int.Parse(DateFile.instance.GetActorDate(mainActorId, 601 + bookType));
                requiredQualification = int.Parse(DateFile.instance.gongFaDate[skillId][63]) +
                    (int)(float.Parse(DateFile.instance.gongFaDate[skillId][64]) * skillGrade);
            }
            else
            {
                var skillGrade = DateFile.instance.GetSkillLevel(skillId);
                var bookType = int.Parse(DateFile.instance.skillDate[skillId][3]);
                charQualification = int.Parse(DateFile.instance.GetActorDate(mainActorId, 501 + bookType));
                requiredQualification = int.Parse(DateFile.instance.skillDate[skillId][5]) +
                    (int)(float.Parse(DateFile.instance.skillDate[skillId][6]) * skillGrade);
            }

            return charQualification >= requiredQualification;
        }


        private static bool Prefix()
        {
            if (!Main.enabled || !Main.settings.autoBreakThrough) return true;

            if (IsEasyToBreakThrough())
            {
                StudyWindow.instance.StudyEnd();
                return false;
            }

            return true;
        }
    }


    /// <summary>
    /// 难度 100% 及以下时, 修改已突破点数不低于设定值
    /// </summary>
    [HarmonyPatch(typeof(StudyWindow), "GetStudyMaxPower")]
    public static class StudyWindow_GetStudyMaxPower_Patch
    {
        private static void Postfix(ref int __result)
        {
            if (!Main.enabled || !Main.settings.autoBreakThrough) return;

            if (StudyWindow_CloseStudyWindowButton_Patch.IsEasyToBreakThrough() &&
                __result < StudyWindow_CloseStudyWindowButton_Patch.MinBreakThroughPoints)
            {
                __result = StudyWindow_CloseStudyWindowButton_Patch.MinBreakThroughPoints;
            }
        }
    }


    /// <summary>
    /// 精纯高于敌方时, 可按键秒杀敌方
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem), "Update")]
    public static class BattleSystem_Update_Patch
    {
        private static void Postfix()
        {
            if (!Main.enabled || !Main.settings.quickKill) return;

            if (Input.GetKeyDown(KeyCode.K))
            {
                var allyId = BattleSystem.instance.ActorId(true);
                var enemyId = BattleSystem.instance.ActorId(false);

                var allyConsummatePoints = int.Parse(DateFile.instance.GetActorDate(allyId, 901));
                var enemyConsummatePoints = int.Parse(DateFile.instance.GetActorDate(enemyId, 901));

                if (allyConsummatePoints > enemyConsummatePoints)
                {
                    var textSize = BattleSystem.instance.largeSize;
                    BattleSystem.instance.SetRealDamage(false, 0, 15, 100000, enemyId, textSize, true);
                }
            }
        }
    }
}
