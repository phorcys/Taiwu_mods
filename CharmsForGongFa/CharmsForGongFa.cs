using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ArchiveSystem;
using ConchShipGame.Event;
using GameData;
using Harmony12;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;
using EventHandler = ConchShipGame.Event.EventHandler;
using Random = UnityEngine.Random;

namespace CharmsForGongFa
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool featureLock;    // 控制锁定童子之身的开关
        public int bonusOfBonus;    // 诱惑成功率加成
        public bool checkSexuality; // 生成选项前检验性取向
        public bool manToPregnant;  // 男太吾怀孕
        public bool debug;
        public string[] lessThan = new string[6];
        public string[] moreThan = new string[6];
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

        //public static string picpath;
        public static string txtpath;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            txtpath = Path.Combine(modEntry.Path, "Date");

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static void RunningFeatureChange()
        {
            DateFile.instance.ChangeActorFeature(DateFile.instance.MianActorID(), 4002, 4001);
            Logger.Log("太吾特性修改 4002 => 4001");
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            var instance = DateFile.instance;
            var SaveDataLoad = instance != null && GameData.Characters.HasChar(instance.mianActorId);

            GUILayout.BeginVertical("Box");
            GUILayout.Label("☆ 童子之身 ★",
                new GUIStyle { normal = { textColor = new Color(0.999999f, 0.537255f, 0.537255f) } });
            settings.featureLock = GUILayout.Toggle(settings.featureLock, "锁定太吾童子之身");
            if (SaveDataLoad)
            {
                if (GUILayout.Button("点击恢复太吾童子之身", new GUILayoutOption[] { GUILayout.Width(200f) }))
                    RunningFeatureChange();
            }
            else
                GUILayout.Label("存档未载入");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label("★ 魅惑之术 ☆",
                new GUIStyle { normal = { textColor = new Color(0.999999f, 0.5647058f, 0.3411764f) } });
            GUILayout.BeginHorizontal();
            settings.bonusOfBonus = (int)GUILayout.HorizontalSlider(settings.bonusOfBonus, -100, 100);
            GUILayout.Label(string.Format("魅惑之术成功率修正：{0}%", settings.bonusOfBonus),
                new GUIStyle { normal = { textColor = new Color(0.999999f, 0.537255f, 0.537255f) } });
            GUILayout.EndHorizontal();
            settings.checkSexuality = GUILayout.Toggle(settings.checkSexuality, "色诱前检验性取向");
            settings.manToPregnant = GUILayout.Toggle(settings.manToPregnant, "男性也可怀孕(仅限太吾交换功法时)");
            settings.debug = GUILayout.Toggle(settings.debug, "Debug");
            GUILayout.EndVertical();

            if (SaveDataLoad)
            {
                var taiwu = DateFile.instance.MianActorID();
                if (GUILayout.Button("点击清除新增的名誉", new GUILayoutOption[] { GUILayout.Width(200f) }))
                {
                    if (DateFile.instance.actorFame[taiwu].ContainsKey(399))
                        DateFile.instance.actorFame[taiwu].Remove(399);
                    if (DateFile.instance.actorFame[taiwu].ContainsKey(400))
                        DateFile.instance.actorFame[taiwu].Remove(400);
                }
                if (!DateFile.instance.actorFame[taiwu].ContainsKey(399) && !DateFile.instance.actorFame[taiwu].ContainsKey(400))
                {
                    GUILayout.Label("现在存档之后，可以安全地卸载本MOD了",
                        new GUIStyle { normal = { textColor = new Color(0.999999f, 0, 0) } });
                }
            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        /// <summary>
        ///  修改真阴纯阳时拦截 若是主角 固定为真阴纯阳
        /// </summary>
        [HarmonyPatch(typeof(DateFile), "ChangeActorFeature")]
        public static class DateFile_ChangeActorFeature_Patch
        {
            private static bool Prefix(int actorId, int oldFeatureId, int newFeatureId)
            {
                if (!enabled || actorId != DateFile.instance.MianActorID())  // mod 未启用 || 目标角色非太吾本人
                {
                    return true;
                }

                if (oldFeatureId == 4001 && newFeatureId == 4002)
                {   // 目标为修改太吾的 真阳纯阴 到 杂阳毁阴 拦截修改
                    Logger.Log("太吾特性修改 4001 => 4002 拦截成功");
                    return false;
                }
                // 目标为修改太吾 杂阳毁阴 到 身怀六甲， 如果太吾特性为 真阳纯阴，直接改为身怀六甲
                if (oldFeatureId == 4002 && newFeatureId == 4003 && DateFile.instance.GetActorFeature(actorId).Contains(4001))
                {
                    DateFile.instance.ChangeActorFeature(actorId, 4001, 4003);
                    Logger.Log("太吾直接怀孕");
                    return false;
                }
                // 太吾结束身怀六甲，拦截，改为回到真阳纯阴
                if (oldFeatureId == 4003 && newFeatureId == 4002)
                {
                    DateFile.instance.ChangeActorFeature(actorId, 4003, 4001);
                    Logger.Log("太吾孕后，恢复童子之身");
                    return false;
                }

                return true;
            }

        }   // End of DateFile_ChangeActorFeature_Patch

        [HarmonyPatch(typeof(LoadGame), "LoadReadonlyData")]
        public class LoadGame_LoadReadonlyData_Patch
        {
            //[HarmonyBefore("characterFolatInfo")]
            public static void Postfix()
            {
                Index.EventIndex = DateLoader.LoadEventDate(Main.txtpath, "Event_Date.txt");
                DateLoader.LoadFameDate(txtpath, "Fame_Date.txt");
                //Index.TipsMassageIndex = DateLoader.LoadTipsMassage(txtpath, "TipsMassage_Date.txt");

                Logger.Log("开始初始化事件管理器");
                DateFile.EventMethodManager.RegisterEventBase(typeof(EventExtentionHandle));
                //Index.GongFaPowerIndex = SevenNineLove.LoadGongFaPower(Main.txtpath, "GayMax_GongFaPower_Date.txt",
                //    "GayMax_GongFaAntiPower_Date.txt", true);
                //Index.GongFaIndex = SevenNineLove.LoadGongFa(Main.txtpath, "GayMax_GongFa_Date.txt", Index.GongFaPowerIndex,
                //    Main.settings.gongbase);
                //Index.TurnEvenIndex = SevenNineLove.LoadOtherDate(Main.txtpath, "GayMax_TrunEvent_Date.txt",
                //    ref DateFile.instance.trunEventDate);
            }
        }

        [HarmonyPatch(typeof(PeopleLifeAI), "DoTrunAIChange")]
        public static class PeopleLifeAi_DoTrunAiChange_Patch
        {
            private static void Prefix(PeopleLifeAI __instance, ref int actorId, ref int mapId, ref int tileId,
                ref int mainActorId, bool isTaiwuAtThisTile, ref int worldId)
            {
                foreach (var e in EventExtentionHandle.aiTurnEvents)
                    PeopleLifeAI.instance.aiTrunEvents.Add(e);
                EventExtentionHandle.aiTurnEvents.Clear();
            }
        }
        // 获取私有函数权限
        public static class Interfunctional
        {
            public static MethodInfo AiMoodChange = typeof(PeopleLifeAI).GetMethod("AiMoodChange",
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);

            public static MethodInfo AISetOtherLove = typeof(PeopleLifeAI).GetMethod("AISetOtherLove",
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);

            public static MethodInfo AIGetLove = typeof(PeopleLifeAI).GetMethod("AIGetLove",
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public class EventExtentionHandle : IEventBase
        {
            private static readonly List<int> itemList = new List<int>();
            public static List<int[]> aiTurnEvents = new List<int[]>();
            public void Init(EventMethodManager baseManager)
            {
                // 向 "(修习...)" 选项创建的事件中添加选项， 添加在 "偷师功法" 之前
                baseManager.RegisterMethod<ChoiceFilter>("-9004", 2, (int eventId, ref List<string> choice) =>
                {
                    var actorId = MessageEventManager.Instance.MainEventData[3];
                    Logger.Log($"actorId:{actorId}, 性取向:{GetSexuality(actorId)}," +
                        $" 目标性别:{GetGender(actorId)}, 太吾性别:{GetGender(DateFile.instance.MianActorID())}");
                    // 设置需要检验性取向 && 性取向是直的 && 性别相同，不添加该选项
                    if (settings.checkSexuality && GetSexuality(actorId) && GetGender(actorId) == GetGender(DateFile.instance.MianActorID()))
                        return;
                    var index = choice.FindIndex(t => t.Equals("900200003"));
                    choice.Insert(index, Index.EventIndex[1].ToString());
                });
                baseManager.RegisterMethod<EventHandler>("500001", 2, EventHandler_500001);
                baseManager.RegisterMethod<EventHandler>("500002", 2, EventHandler_500002);

                // 选项 跳转 物品选择 限定功法书
                baseManager.RegisterMethod<ItemWindowFilter>(Index.EventIndex[3].ToString(), 2,
                    (int eventId, ref List<int> items) => { items = itemList; });
                // 选项 物品选择完成 选定功法书为chosenId
                baseManager.RegisterMethod<ItemChosen>(Index.EventIndex[3].ToString(), 2, BookChosen);

                Logger.Log("初始化完成!");
            }
            // 功法选择完成，进行判定
            private static void BookChosen(int chosenId, int[] eventData, ref int toEventId)
            {
                var actorId = DateFile.instance.MianActorID();

                // 开始判定
                var GongFaId = int.Parse(DateFile.instance.GetItemDate(chosenId, 32));
                var GongFaRare = int.Parse(DateFile.instance.gongFaDate[GongFaId][2]);
                CharmsCheck(eventData[3], GongFaRare, out List<int> results);
                Logger.Log($"判定结果为results:{string.Join(", ", results)}");
                bool flag;
                if (results[0] >= 100)
                    flag = true;
                else
                {
                    var rnd = Random.Range(0, 100);
                    Logger.Log($"Rolled:{rnd}");
                    flag = rnd <= results[0];
                }

                // 下一个事件的eventData D1 对应类型为5 即[4]为要显示物品名的物品的ID [5]开始为results[0..3]
                int[] NextEventData = { 0, eventData[3], Index.EventIndex[(flag ? 5 : 6)],
                            eventData[3], chosenId, results[0], results[1], results[2], results[3]};
                DateFile.instance.eventId.Add(NextEventData);
                //Logger.Log($"跳转到下一个事件 NextEventIndex={(flag ? 5 : 6)} => {Index.EventIndex[(flag ? 5 : 6)]}");
            }
            // 500001处理 &1用于添加功法书 &2用于取消功法书和色诱事件
            private static void EventHandler_500001(int[] eventData, int[] eventValue)
            {
                if (eventValue[1] == 1)
                {
                    Logger.Log($"太吾已经下定决心牺牲色相换取功法了。");
                    var actorId = DateFile.instance.MianActorID();
                    AddGongFaBook(eventData[3], GetLearnableGongfa(eventData[3]));
                }
                else if (eventValue[1] == 2)
                {
                    Logger.Log("太吾反悔了");
                    RemoveGongFaBook(eventData[3], -1);
                    Logger.Log("临时添加的功法书已经全部移除");
                    MessageEventManager.Instance.EventValue[0] = 90071; // 其他话题 即返回谈话初始界面
                    MessageEventManager.Instance.EndEvent();
                }
            }
            // 500002处理 &1为色诱成功获取功法并处理副作用 &2为色诱失败
            private static void EventHandler_500002(int[] eventData, int[] eventValue)
            {
                // eventData [4]为 chosenId    [5][6][7][8]分别为results[0..3]
                var actorId = DateFile.instance.MianActorID();
                var chosenId = eventData[4];
                // 判定成功的分支，处理事件副作用
                if (eventValue[1] == 1)
                {
                    Logger.Log("太吾确认进行交易");
                    RemoveGongFaBook(eventData[3], chosenId);
                    Logger.Log($"临时添加的功法书已经移除，除了被选中的《{GetGongfaColorName(int.Parse(DateFile.instance.GetItemDate(chosenId, 32)))}》");
                    // 学会该功法
                    DateFile.instance.ChangeActorGongFa(actorId, int.Parse(DateFile.instance.GetItemDate(chosenId, 32)), 0, 0, 0, true);
                    // 根据判定结果，对功法书进行补全
                    int basePages = (int)((14 - int.Parse(DateFile.instance.GetItemDate(chosenId, 8))) / 2);
                    int bonus = (int)(Math.Max(0, eventData[5] - 100) / 25f);
                    DateFile.instance.SetBookPage(chosenId, 100, Math.Min((basePages + bonus), 10));
                    // 添加功法书到太吾身上
                    //DateFile.instance.actorItemsDate[actorId].Add(chosenId, 1);
                    DateFile.instance.GetItem(actorId, chosenId, 1, false, 0);
                    // 左下角提示信息，获得物品
                    //TipsWindow.instance.ShowNotifyEvent(eNotifyEvents.ItemChange, new int[] { chosenId, 1 });
                    // 显示获得物品窗口
                    //GetItemWindow.instance.ShowGetItemWindow(false, chosenId, 0, 1);
                    // 添加名誉影响 粘花惹絮
                    if (DateFile.instance.HaveActor(DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, true, false, true).Count > 2)
                    {
                        DateFile.instance.SetActorFameList(actorId, 400, 1, 0);
                        // 沉湎淫逸
                        if (DateFile.instance.actorFame[actorId][400][0] > 6)
                        {
                            DateFile.instance.SetActorFameList(actorId, 399, 1, 0);
                        }
                    }
                    // 太吾心情变化，幅度[-20, 20]
                    DateFile.instance.SetActorMood(actorId, Math.Min(Math.Max(-20, eventData[6]), 20));
                    // 太吾新增伤口
                    int total = eventData[7] + Random.Range(-10, 25);
                    if (total > 0)
                    {
                        void addInjury(int offset, int basicPower)
                        {
                            var injuryId = Random.Range(0, 8) * 6 + offset;
                            if (injuryId > 48)  // 毒质伤忽略
                                injuryId += 6;
                            if (Random.Range(0, 100) < 15)  // 15%概率转为内伤
                                injuryId += 3;
                            DateFile.instance.AddInjury(actorId, injuryId, (int)(Random.Range(0.75f, 1.25f) * basicPower));
                        }
                        int serious = total / 75; // 重伤数量
                        total %= 75;
                        int minor = total / 20; // 轻伤数量
                        if (total > 0 && Random.Range(0, 20) <= total)
                            minor++;
                        for (int i = 0; i < minor; ++i) // 添加轻伤
                        {
                            addInjury(1, 120);
                        }
                        for (int i = 0; i < serious; ++i)   // 添加重伤
                        {
                            addInjury(2, 80);
                        }
                        Logger.Log($"total= {total} 本次交易导致太吾新增 {minor} 个轻伤口，{serious} 个重伤口");
                    }
                    //Logger.Log($"本次交易将导致太吾新增 {eventData[7] / 20} 个伤口，该功能尚未完成，暂时不添加");

                    // 太吾的魅力    (真实值+加成后)/2
                    int TaiwuCharm = (int)((int.Parse(DateFile.instance.GetActorDate(actorId, 15, true)) +
                        int.Parse(DateFile.instance.GetActorDate(actorId, 15, false))));
                    // 目标魅力
                    int actorCharm = (int)((int.Parse(DateFile.instance.GetActorDate(eventData[3], 15, true)) +
                        int.Parse(DateFile.instance.GetActorDate(eventData[3], 15, false))) / 2f);
                    // 目标对太吾好感变化(一般为上升)
                    DateFile.instance.ChangeFavor(eventData[3], 10 * (TaiwuCharm - actorCharm));

                    // 心生爱慕
                    if (!DateFile.instance.GetActorSocial(eventData[3], 312, false, false).Contains(actorId) && Random.Range(0, 100) < eventData[8])
                    {
                        var place = DateFile.instance.GetActorAtPlace(actorId);
                        Interfunctional.AISetOtherLove.Invoke(PeopleLifeAI.instance, new object[] { actorId, eventData[3], actorId, place[0], place[1] });
                        bool flag = true;
                        foreach (var e in aiTurnEvents)
                            if (e.Length == 4 && e[0] == 224 && e[3] == eventData[3])
                                flag = false;
                        if (flag)
                            aiTurnEvents.Add(new int[]
                            {
                                224,
                                place[0],
                                place[1],
                                eventData[3]
                            });
                        Logger.Log("心生爱慕");
                    }

                    // 模拟生孩子
                    string recover = "";
                    int gender = GetGender(eventData[3]);
                    int taiwuGender = GetGender(actorId);
                    int fatherId, motherId;
                    // 同性则随机分配父母角色
                    if (gender != taiwuGender)
                    {
                        fatherId = gender == 1 ? eventData[3] : actorId;
                        motherId = gender == 1 ? actorId : eventData[3];
                    }
                    else
                    {
                        fatherId = Random.Range(0, 100) < 50 ? actorId : eventData[3];
                        motherId = fatherId == actorId ? eventData[3] : actorId;
                    }
                    if (settings.manToPregnant && GetGender(motherId) == 1)
                    {
                        recover = DateFile.instance.actorAttrDate[actorId][14];
                        DateFile.instance.actorAttrDate[motherId][14] = "2";
                        Logger.Log("男太吾生孩子，暂时转换成女性");
                    }
                    if (PeopleLifeAI.instance.AISetChildren(fatherId, motherId, 1, 1))
                        Logger.Log("生孩子了！！！");
                    if (recover != "")
                        DateFile.instance.actorAttrDate[motherId][14] = recover;
                }
                else if (eventValue[1] == 2)
                {
                    Logger.Log("选择了功法之后，太吾反悔了");
                    DateFile.instance.DeleteItemDate(chosenId);
                    Logger.Log("已移除选中的功法书");
                    MessageEventManager.Instance.EventValue[0] = 90071; // 其他话题 即返回谈话初始界面
                    MessageEventManager.Instance.EndEvent();
                }
                else if (eventValue[1] == 3)
                {
                    Logger.Log("选择了功法之后，太吾被嫌弃了");
                    DateFile.instance.DeleteItemDate(chosenId);
                    Logger.Log("临时添加的功法书已经全部移除");
                    // 太吾的魅力    (真实值+加成后)/2
                    int TaiwuCharm = (int)(int.Parse(DateFile.instance.GetActorDate(actorId, 15, true)) +
                        int.Parse(DateFile.instance.GetActorDate(actorId, 15, false)) / 2f);
                    // 目标魅力
                    int actorCharm = (int)((int.Parse(DateFile.instance.GetActorDate(eventData[3], 15, true)) +
                        int.Parse(DateFile.instance.GetActorDate(eventData[3], 15, false))) / 2f);
                    // 目标对太吾好感变化(一般为下降)
                    if (actorCharm > TaiwuCharm)
                        DateFile.instance.ChangeFavor(eventData[3], 10 * (TaiwuCharm - actorCharm));
                    else
                        DateFile.instance.ChangeFavor(eventData[3], 5 * (900 - TaiwuCharm) + 1);
                }
            }

            private static void AddGongFaBook(int actorId, List<int> GongFaIdList)
            {
                itemList.Clear();
                var gongFas = DateFile.instance.actorGongFas[actorId];
                foreach (int GongFaId in GongFaIdList)
                {
                    int itemId = 0;
                    //int isNiLian = Random.Range(0, 100) > 50 ? 1 : 0;
                    var gongFaData = gongFas[GongFaId];
                    itemId = Random.Range(0, gongFaData[1]) < gongFaData[2] ? GongFaId + 700000 : GongFaId + 500000;
                    if (DateFile.instance.presetitemDate[itemId].ContainsKey(32))   // 是功法
                    {
                        int item = DateFile.instance.GetItem(actorId, itemId, 1, true);
                        if (item > 0)
                        {
                            int rare = int.Parse(DateFile.instance.GetItemDate(item, 8));
                            DateFile.instance.SetBookPage(item, 100, (int)((14 - rare) / 2));
                            Items.SetItemProperty(item, 901, "3");
                            Items.SetItemProperty(item, 902, "3");
                            itemList.Add(item);
                        }
                    }
                }
                //GetGongfaColorName(GongFaId)
                Logger.Log($"添加{string.Join(", ", GongFaIdList.Select(t => GetGongfaColorName(t)))}到{DateFile.instance.GetActorName(actorId)}身上");
            }

            private static void RemoveGongFaBook(int actorId, int chosenId)
            {
                foreach (int item in itemList)
                {
                    DateFile.instance.actorItemsDate[actorId].Remove(item); // 从人物身上删除
                    if (item != chosenId)
                    {
                        DateFile.instance.DeleteItemDate(item); // 如果是不需要的，从所有物品信息中删除
                    }
                }
                itemList.Clear();
            }

            /// <summary>
            /// 获取性取向 true 即 直 
            /// </summary>
            /// <param name="actorId"></param>
            /// <returns></returns>
            private static bool GetSexuality(int actorId) => DateFile.instance.GetActorDate(actorId, 21, false) == "0" ? true : false;

            /// <summary>
            /// 获取性别
            /// </summary>
            /// <param name="actorId"></param>
            /// <returns></returns>
            private static int GetGender(int actorId)
            {
                int gender = Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(actorId, 14, false)), 0, 2);
                if (DateFile.instance.GetActorDate(actorId, 17, false) == "1" && gender > 0)
                    return gender - 1;
                return gender;
            }
            /// <summary>
            /// 色诱事件判定
            /// </summary>
            /// <param name="actorId"></param>
            /// <param name="GongFaRare"></param>
            /// <returns>result[0..2] [0]用于判定 [1]为心情变化值 [2]为受伤总量</returns>
            private static void CharmsCheck(int actorId, int GongFaRare, out List<int> results)
            {
                var debug = settings.debug;
                int Taiwu = DateFile.instance.MianActorID();    // 太吾的id
                if (debug)
                    Logger.Log("进入判定函数");

                // 太吾的魅力    (真实值+加成后)/2
                double TaiwuCharm = (int.Parse(DateFile.instance.GetActorDate(Taiwu, 15, true)) + int.Parse(DateFile.instance.GetActorDate(Taiwu, 15, false))) / 2f;
                // 目标魅力
                double actorCharm = (int.Parse(DateFile.instance.GetActorDate(actorId, 15, true)) + int.Parse(DateFile.instance.GetActorDate(actorId, 15, false))) / 2f;
                // 综合魅力参数
                double charm = (TaiwuCharm * 2 - actorCharm) / 500f;
                charm = ((charm > 0) ? 0.5 : -0.5) * Math.Pow(charm, 2);
                // 设计时以600魅力，高出目标200为基准 = 1.28   实际范围[-900, 1800] / 500f = [-1.8, 3.6]   (^2)/2 => [-1.62, 6.48]
                if (debug)
                    Logger.Log($"(TaiwuCharm({TaiwuCharm}) * 2 - actorCharm({actorCharm}) /500)^2 / 2 => charm = {charm}");

                // 基础功法品级(当前能从门派学习的品级)
                int baseRare = Math.Min(Math.Max(3, DateFile.instance.GetWorldXXLevel() + 3), 9);
                if (debug)
                    Logger.Log($"世界进度: {DateFile.instance.GetWorldXXLevel()}, baseRare={baseRare}");

                // 目标人物品级
                int actorRare = int.Parse(DateFile.instance.GetActorDate(actorId, 20));
                // 红名的配偶为-1 折算降一级
                if (actorRare == -1) actorRare = 8;
                else actorRare = 10 - actorRare;
                if (debug)
                    Logger.Log($"actorRare={actorRare}");

                // 品阶顺差 最低为0， 权重增补1
                double delRare = Math.Max(0, baseRare - actorRare) * 0.5 + 1;
                // 顺差 >= 0 权重增加， 即同身份品阶，提供加成0.5x， 每提高一级，增加0.5x
                if (baseRare >= actorRare) delRare += 0.5;
                // 身份+魅力权重加成
                double Charms = delRare * charm;
                // 期望加成 1.5 * 1.28 * 40% = 76.8% 
                // 即同身份品阶，魅力差距200，基础魅力600时，基础成功率76.8%
                if (debug)
                    Logger.Log($"Charms = delRare({delRare} * charm({charm}) => {Charms}");

                // 目标对太吾好感度
                int friend = int.Parse(DateFile.instance.GetActorDate(actorId, 3, false));
                // 好感分界线，baseFriend处无加成，向两极二次函数增长
                const double baseFriend = 7500;
                const double MaxFriend = 60000;
                // 好感倍率 两极为 3x
                double FA = friend >= baseFriend ?
                    Math.Pow((friend - baseFriend) / (MaxFriend - baseFriend), 2) * 3f : Math.Pow((baseFriend - friend) / baseFriend, 2) * 3f;
                // 期望加成 3 * 12 / 2 = 18%
                if (debug)
                    Logger.Log($"friend = {friend} FA = {FA}");

                // 太吾名誉
                int TaiwuFame = DateFile.instance.GetActorFame(Taiwu);
                // 目标名誉
                int actorFame = DateFile.instance.GetActorFame(actorId);
                // 名誉顺差
                int delFame = Math.Min(Math.Max(-200, TaiwuFame - actorFame), 200);
                // 名誉倍率 顺差最大 1.5x 逆差最大 2.0x
                double Fame = delFame > 0 ? Math.Pow(delFame / 200f, 2) * 1.5f : Math.Pow(delFame / 200f, 2) * -2f;
                // 期望加成 2 * 12% / 2 = 12%
                if (debug)
                    Logger.Log($"TaiwuFame={TaiwuFame} actorFame={actorFame} delFame={delFame} Fame={Fame}");

                // 目标心情
                int happiness = int.Parse(DateFile.instance.GetActorDate(actorId, 4));
                // 心情与寻常的距离
                int mood = happiness - 50;
                // 心情权重 两极 2x
                double Mood = Math.Pow(mood / 50f, 2) * 2f;
                // 期望加成 2 * 8 / 2 = 8%
                if (debug)
                    Logger.Log($"happiness={happiness} (happiness - 50={mood})^2 * 2 => Mood={Mood}");

                // 功法越级获取惩罚
                int[] punish = { 0, 10, 30, 60, 100, 150, 210 };

                // 基础成功率
                int bonus = (int)(Charms * 40 + Mood * 6 + FA * 8 + Fame * 12);
                // 同身份品阶，学同级功法 期望为 90+18+12+8 = 128%
                // 功法越级情况
                int rareSkip = Math.Min(Math.Max(-6, GongFaRare - baseRare), 6);
                if (debug)
                    Logger.Log($"功法越级数{rareSkip}");
                // 减去惩罚
                bonus -= (rareSkip > 0 ? 1 : -1) * punish[Math.Abs(rareSkip)];
                if (debug)
                {
                    Logger.Log($"Charms({Charms}) * 40 + Mood({Mood}) * 6 + FA({FA}) * 8 + Fame({Fame}) * 12 = bonus");
                    Logger.Log($"bonus={(int)(Charms * 40 + Mood * 6 + FA * 8 + Fame * 12)} - {(rareSkip > 0 ? 1 : -1) * punish[Math.Abs(rareSkip)]} => {bonus}");
                }

                // 太吾对目标的 魅力、身份、名誉 顺差 将导致太吾心情下降 逆差导致1.5x上升
                // 名誉反馈系数
                if (delFame < 0) Fame = -Fame;
                double forFame = Fame / 2f; // [-2, 1.5]
                // 魅力反馈系数
                double forCharm = (TaiwuCharm - actorCharm - 100) / (Math.Max(1, TaiwuCharm / 2f));  // 以太吾600，目标400为例， 倍率为0.333...
                // 身份反馈系数
                double forRare = (baseRare - actorRare) / 4f;   // 极限±2
                // 反馈系数
                double feedBack = (forFame + forCharm + forRare) / 3f; // 以太吾600，目标400为例， [-1.22.., 1.28]
                int MoodBonus = (int)(feedBack * (feedBack > 0 ? -8 : -12));
                //Interfunctional.AiMoodChange.Invoke(PeopleLifeAI.instance,
                //            new object[] { Taiwu, MoodBonus});
                if (debug)
                    Logger.Log($"forFame={forFame}\t forCharm={forCharm}\t forRare={forRare}");
                if (debug)
                    Logger.Log($"feedBack={feedBack}\t MoodBonus={MoodBonus}");

                // 目标的 心情 及 好感 将反馈导致太吾受伤的可能性    _(:з」∠)_ 
                if (friend > 7500) FA = -FA;
                // 好感反馈系数
                double forFA = FA * 1.5f;   // [-4.5, 4.5]
                // 心情反馈系数
                double forMood = mood / 50 * -3f;    // [-3, 3]
                // 太吾受伤可能性
                int possibility = Math.Max(0, (int)(forFA * 50 + forMood * 30));
                // [0, 315]
                if (debug)
                    Logger.Log($"forFA={forFA}\t forMood={forMood}\t possibility={possibility}");

                // 心生爱慕的可能性
                double getLove = 0;
                // 好感
                if (friend > baseFriend)
                {
                    getLove += Mathf.Clamp((float)(-16 * FA), 0, 30);
                    if (debug)
                        Logger.Log($"GetLove\nforFA:{Mathf.Clamp((float)(-16 * FA), 0, 30)}");
                }
                // 名誉
                if (DateFile.instance.IsFameGoodAndBad(Taiwu) == DateFile.instance.IsFameGoodAndBad(actorId))
                {
                    getLove += 5f;
                    if (debug)
                        Logger.Log($"forFame:5");
                }
                // 身份与魅力
                getLove += Mathf.Clamp((float)(Charms * 12), -40, 40);
                int GN = DateFile.instance.GetActorGoodness(actorId);
                getLove -= GN * 5;
                if (GN == 3)
                    getLove += 30;  //叛逆加成
                if (debug)
                {
                    Logger.Log($"forCharms:{Mathf.Clamp((float)(Charms * 12), -40, 40)}");
                    Logger.Log($"forGN:{(GN * -5 + (GN == 3 ? 30 : 0))}");
                }

                results = new List<int>
            {
                bonus + settings.bonusOfBonus,
                MoodBonus,
                possibility,
                (int)getLove
            }; // [0]为判定值 [1]为心情变化值 [2]为受伤总量
            }

            /// <summary>
            /// 获取功法等级
            /// </summary>
            /// <param name="gongFaId"></param>
            /// <returns></returns>
            private static int GetGongfaLevel(int gongFaId) => DateFile.instance.gongFaDate.TryGetValue(gongFaId, out var gongFa) ? int.Parse(gongFa[2]) : 0;

            /// <summary>
            /// 获取功法名称
            /// </summary>
            /// <param name="gongFaId"></param>
            /// <returns></returns>
            private static string GetGongfaName(int gongFaId) => DateFile.instance.gongFaDate.TryGetValue(gongFaId, out var gongFa) ? gongFa[0] : "";

            /// <summary>
            /// 带等级颜色的功法名称
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            private static string GetGongfaColorName(int id) => DateFile.instance.SetColoer(20001 + GetGongfaLevel(id), GetGongfaName(id));

            /// <summary>
            /// 获取列表中品级最高功法的名字与数量
            /// </summary>
            /// <param name="gongFas"></param>
            /// <returns></returns>
            private static string GetBestGongfaText(IEnumerable<int> gongFas)
            {
                var bestGongFa = new List<int>();
                int bestLevel = -1;
                foreach (int gongFaID in gongFas)
                {
                    if (GetGongfaLevel(gongFaID) == bestLevel)
                    {
                        bestGongFa.Add(gongFaID);
                    }
                    else if (GetGongfaLevel(gongFaID) > bestLevel)
                    {
                        bestGongFa.Clear();
                        bestGongFa.Add(gongFaID);
                        bestLevel = GetGongfaLevel(gongFaID);
                    }
                }
                return string.Join(", ", bestGongFa.Select(GetGongfaColorName));
            }

            /// <summary>
            /// 获取角色功法
            /// </summary>
            /// <param name="actorId"></param>
            /// <returns></returns>
            private static IEnumerable<int> GetGongfaList(int actorId)
            {
                if (DateFile.instance.actorGongFas.TryGetValue(actorId, out var gongFa))
                {
                    foreach (int gongFaID in gongFa.Keys)
                    {
                        if (gongFaID != 0)
                            yield return gongFaID;
                    }
                }
                else
                    // 避免存取死人資料時引發紅字
                    yield break;
            }

            /// <summary>
            /// 人物身上可被太吾修习的功法获取
            /// </summary>
            /// <param name="actorId"></param>
            /// <returns></returns>
            private static List<int> GetLearnableGongfa(int actorId)
            {
                Logger.Log("开始计算可学习的功法");
                var myGongFas = new HashSet<int>(GetGongfaList(DateFile.instance.MianActorID()));

                //挑出目标人物身上太吾未学会的功法
                var nGongFas = new List<int>();
                foreach (int gongFaId in GetGongfaList(actorId))
                {
                    if (!myGongFas.Contains(gongFaId))
                    {
                        if (DateFile.instance.GetGongFaLevel(actorId, gongFaId) >= 50)  // 功法修习程度 >= 50
                        {
                            nGongFas.Add(gongFaId);
                        }
                    }
                }
                return nGongFas;
            }

        } // End of EventExtentionHandle
    } // End of Main
}

