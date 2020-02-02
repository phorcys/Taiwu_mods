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
using System.Xml.Serialization;

namespace DreamLover
{
    public class Setting : UnityModManager.ModSettings
    {
        public bool 主动爱慕 = true;
        public bool 主动表白 = true;
        public bool 主动求婚 = true;
        public bool 太吾不爱的人也表白太吾 = false;
        public bool 不顾门派爱太吾 = false;
        public bool 已婚人士想和太吾结婚 = false;
        public bool 即使太吾已婚别人也想求婚 = false;
        public bool 即使出家也要求婚太吾 = false;
        public bool 即使太吾出家也要求婚 = false;

        public bool 男的可以来 = false, 女的可以来 = true;
        public bool 全世界都喜欢太吾 = false;

        public SerializableDictionary<int, bool> 可接受的好感度等级 = new SerializableDictionary<int, bool> {
            { 0, false}, { 1, false}, { 2, false},
            { 3, false}, { 4, true}, {5, true },
            { 6, true}, {-1, false } };

        public SerializableDictionary<int, bool> 可接受的立场等级 = new SerializableDictionary<int, bool> {
            { 0, true}, { 1, true}, { 2, false},
            { 3, false}, { 4, false}};

        public SerializableDictionary<int, bool> 可接受的魅力等级 = new SerializableDictionary<int, bool> {
            { 0, false}, { 1, false}, { 2, false}, { 3, true}, { 4, true},
            { 5, true}, { 6, true}, { 7, true}, { 8, true}, { 9, true}};

        public SerializableDictionary<int, bool> 可接受的阶层等级 = new SerializableDictionary<int, bool> {
            { 1, true}, { 2, true}, { 3, true}, { 4, true}, { 5, true},
            { 6, true}, { 7, true}, { 8, true}, { 9, true}};

        public int 入魔程度 = 0;

        public bool 兄弟姐妹 = false, 亲生父母 = false, 义父义母 = false,
                    授业恩师 = false, 义结金兰 = false,
                    儿女子嗣 = false, 嫡系传人 = false,
                    势不两立 = false, 血海深仇 = false,
                    父系血统 = false, 母系血统 = false;


        public int 年龄下限 = 14, 年龄上限 = 60;
        
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    [HarmonyPatch(typeof(UIDate), "DoChangeTrun")]
    public static class UIDate_DoChangeTrun_Patch
    {
        private static void Postfix()
        {
            if (!Main.enabled) return;

            string ageStr = DateFile.instance.GetActorDate(DateFile.instance.MianActorID(), 11, applyBonus: false);
            Main.TaiwuAge = (ageStr == null) ? -1 : int.Parse(ageStr);
            string sexStr = DateFile.instance.GetActorDate(DateFile.instance.MianActorID(), 997, applyBonus: false);
            Main.TaiwuSex = (sexStr == null) ? -1 : int.Parse(sexStr);
        }
    }

    /// <summary>
    ///  人物AI更新时拦截并修改结果
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "DoTrunAIChange")]
    public static class PeopleLifeAI_DoTrunAIChange_Patch
    {
        // 因为这个补丁必然在游戏内执行，所以不需要额外判断
        private static bool Prefix(int actorId, int mapId, int tileId, int mainActorId, bool isTaiwuAtThisTile, int worldId, Dictionary<int, List<int>> mainActorItems, int[] aliveChars, int[] deadChars)
        {
            if (!Main.enabled) return true;

            if (!Main.settings.全世界都喜欢太吾 && !isTaiwuAtThisTile) return true;

            // 无性太吾禁止恋爱
            if (Main.TaiwuSex < 0) return true;

            // 好感度
            int 好感度等级 = DateFile.instance.GetActorFavor(false, mainActorId, actorId, true, false);
            if (Main.settings.可接受的好感度等级.TryGetValue(好感度等级, out bool 当前好感可以接受))
            {
                if (!当前好感可以接受) return true;
            }
            else
                Debug.Log(DateFile.instance.GetActorName(actorId) + "好感度为: " + 好感度等级 + "\n");

            // 年龄
            int actorAge = int.Parse(DateFile.instance.GetActorDate(actorId, 11, applyBonus: false));
            if (actorAge < Main.settings.年龄下限 || actorAge > Main.settings.年龄上限) return true;

            // 魅力
            int actorCharm = int.Parse(DateFile.instance.GetActorDate(actorId, 15, applyBonus: true));
            int 魅力等级 = actorCharm / 100;
            if (Main.settings.可接受的魅力等级.TryGetValue(魅力等级, out bool 魅力可以接受))
            {
                if (!魅力可以接受) return true;
            }
            else
                Debug.Log(DateFile.instance.GetActorName(actorId) + "魅力为: " + actorCharm + "\n");


            if(int.TryParse(DateFile.instance.GetActorDate(actorId, 20, applyBonus: false), out int 里阶层))
            {
                int 表阶层 = Mathf.Abs(里阶层);
                if (Main.settings.可接受的阶层等级.TryGetValue(表阶层, out bool 阶层可以接受))
                {
                    if (!阶层可以接受) return true;
                }
                else
                    Debug.Log(DateFile.instance.GetActorName(actorId) + "里阶层为: " + 里阶层 + ", 表阶层: " + 表阶层 + "\n");
            }

            // 性别
            int actorSex = int.Parse(DateFile.instance.GetActorDate(actorId, 997, applyBonus: false));
            if (!Main.settings.男的可以来 && (actorSex % 2 == 1)) return true;
            if (!Main.settings.女的可以来 && (actorSex % 2 == 0)) return true;

            // 不顾门派身份
            int 从属门派 = int.Parse(DateFile.instance.GetActorDate(actorId, 19, applyBonus: false));
            int actor阶层 = int.Parse(DateFile.instance.GetActorDate(actorId, 20, applyBonus: false));
            int gangValueId = DateFile.instance.GetGangValueId(从属门派, actor阶层);
            if (!(Main.settings.不顾门派爱太吾 || int.Parse(DateFile.instance.presetGangGroupDateValue[gangValueId][803]) != 0)) return true;

            // 立场
            int 立场 = DateFile.instance.GetActorGoodness(actorId);
            if (Main.settings.可接受的立场等级.TryGetValue(立场, out bool 立场可以接受))
            {
                if (!立场可以接受) return true;
            }
            else
                Debug.Log(DateFile.instance.GetActorName(actorId) + "立场为: " + 立场 + "\n");

            // 入魔人喜欢
            if(int.TryParse(DateFile.instance.GetActorDate(actorId, 6, applyBonus: false), out int 入魔程度))
            {
                if (!(入魔程度 <= Main.settings.入魔程度)) return true;
            }
            else
                Debug.Log(DateFile.instance.GetActorName(actorId) + "入魔程度解析失败\n");

            // 关系
            if (!Main.settings.兄弟姐妹 && DateFile.instance.GetActorSocial(actorId, 302).Contains(mainActorId)) return true;
            if (!Main.settings.亲生父母 && DateFile.instance.GetActorSocial(actorId, 303).Contains(mainActorId)) return true;
            if (!Main.settings.义父义母 && DateFile.instance.GetActorSocial(actorId, 304).Contains(mainActorId)) return true;
            if (!Main.settings.授业恩师 && DateFile.instance.GetActorSocial(actorId, 305).Contains(mainActorId)) return true;
            if (!Main.settings.义结金兰 && DateFile.instance.GetActorSocial(actorId, 308).Contains(mainActorId)) return true;
            if (!Main.settings.儿女子嗣 && DateFile.instance.GetActorSocial(actorId, 310).Contains(mainActorId)) return true;
            if (!Main.settings.嫡系传人 && DateFile.instance.GetActorSocial(actorId, 311).Contains(mainActorId)) return true;
            if (!Main.settings.势不两立 && DateFile.instance.GetActorSocial(actorId, 401).Contains(mainActorId)) return true;
            if (!Main.settings.血海深仇 && DateFile.instance.GetActorSocial(actorId, 402).Contains(mainActorId)) return true;
            if (!Main.settings.父系血统 && DateFile.instance.GetActorSocial(actorId, 601).Contains(mainActorId)) return true;
            if (!Main.settings.母系血统 && DateFile.instance.GetActorSocial(actorId, 602).Contains(mainActorId)) return true;

            // 如果两情相悦就结婚
            if (isTaiwuAtThisTile && Main.settings.主动求婚
                && DateFile.instance.GetActorSocial(actorId, 306).Contains(mainActorId) && DateFile.instance.GetActorSocial(mainActorId, 306).Contains(actorId)
                && !DateFile.instance.GetActorSocial(actorId, 309).Contains(mainActorId) && !DateFile.instance.GetActorSocial(mainActorId, 309).Contains(actorId)
                && (Main.settings.已婚人士想和太吾结婚 || DateFile.instance.GetActorSocial(actorId, 309).Count <= 0)
                && (Main.settings.即使太吾已婚别人也想求婚 || DateFile.instance.GetActorSocial(mainActorId, 309).Count <= 0)
                && (Main.settings.即使出家也要求婚太吾 || int.Parse(DateFile.instance.GetActorDate(actorId, 2, applyBonus: false)) == 0) // 这个人未出家
                && (Main.settings.即使太吾出家也要求婚 || int.Parse(DateFile.instance.GetActorDate(mainActorId, 2, applyBonus: false)) == 0)) // 主角未出家
            {
                PeopleLifeAI_Utils.AISetEvent(8, new int[4] { 0, actorId, 232, actorId });
                Debug.Log(DateFile.instance.GetActorName(actorId) + " 试图求婚 " + "太吾传人\n");
            }

            // 如果爱慕就表白
            if (isTaiwuAtThisTile && Main.settings.主动表白 && DateFile.instance != null
            && DateFile.instance.GetActorSocial(actorId, 312).Contains(mainActorId)
            && (Main.settings.太吾不爱的人也表白太吾 || DateFile.instance.GetActorSocial(mainActorId, 312).Contains(actorId)) // 互相倾心爱慕
            && !DateFile.instance.GetActorSocial(actorId, 306).Contains(mainActorId) && !DateFile.instance.GetActorSocial(mainActorId, 306).Contains(actorId) // 没有互相两情相悦
            && !DateFile.instance.GetActorSocial(actorId, 309).Contains(mainActorId) && !DateFile.instance.GetActorSocial(mainActorId, 309).Contains(actorId)) // 没有互相结婚
            {
                PeopleLifeAI_Utils.AISetEvent(8, new int[4] { 0, actorId, 231, actorId });
                Debug.Log(DateFile.instance.GetActorName(actorId) + " 试图表白 " + "太吾传人\n");
            }

            // 如果没有爱慕就爱慕
            if (Main.settings.主动爱慕 && !DateFile.instance.GetActorSocial(actorId, 312).Contains(mainActorId))
            {
                PeopleLifeAI_Utils.AISetOtherLove(mainActorId, actorId, mainActorId, mapId, tileId);
                Debug.Log(DateFile.instance.GetActorName(actorId) + " 喜欢上了 " + "太吾传人\n");
            }

            
            return true;
        }

    }

    public static class PeopleLifeAI_Utils
    {
        // loveID == mianActorID
        public static void AISetOtherLove(int mianActorId, int actorId, int loveId, int partId, int placeId)
        {
            CallPrivateMethod<PeopleLifeAI>(PeopleLifeAI.instance, "AISetOtherLove",
                new object[] { mianActorId, actorId, loveId, partId, placeId});
        }

        public static void AICantMove(int actorId)
        {
            CallPrivateMethod<PeopleLifeAI>(PeopleLifeAI.instance, "AICantMove",
                new object[] { actorId});
        }

        public static void AISetEvent(int typ, int[] aiEventDate)
        {
            CallPrivateMethod<PeopleLifeAI>(PeopleLifeAI.instance, "AISetEvent",
                new object[] { typ, aiEventDate });
        }

        public static T CallPrivateMethod<T>(this object instance, string name, params object[] param)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            MethodInfo method = type.GetMethod(name, flag);
            return (T)method.Invoke(instance, param);
        }
    }

    public static class ExpandUtils
    {
        public static bool RemoveAllLove()
        {
            if (DateFile.instance == null || !GameData.Characters.HasChar(DateFile.instance.MianActorID()))
                return false;
            int mainActorId = DateFile.instance.MianActorID();
            int[] actorsID = GameData.Characters.GetAllCharIds();
            foreach(int actorId in actorsID)
            {
                if(DateFile.instance.GetActorSocial(actorId, 312).Contains(mainActorId)
                && !DateFile.instance.GetActorSocial(mainActorId, 312).Contains(actorId))
                {
                    DateFile.instance.RemoveActorSocial(actorId, mainActorId, 312);
                }
            }
            return true;
        }
    }

    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Setting settings;
        public static int TaiwuAge = -1;
        public static int TaiwuSex = -1;

        private static bool 接受入邪 = false, 接受入魔 = false;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Setting.Load<Setting>(modEntry);

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal("Box");
            settings.主动爱慕 = GUILayout.Toggle(settings.主动爱慕, "主动爱慕");
            settings.主动表白 = GUILayout.Toggle(settings.主动表白, "主动表白");
            settings.主动求婚 = GUILayout.Toggle(settings.主动求婚, "主动求婚");
            GUILayout.EndHorizontal();

            if (settings.主动爱慕 || settings.主动表白 || settings.主动求婚)
            {
                GUILayout.BeginHorizontal("Box");
                settings.男的可以来 = GUILayout.Toggle(settings.男的可以来, "接受男性");
                settings.女的可以来 = GUILayout.Toggle(settings.女的可以来, "接受女性");
                settings.全世界都喜欢太吾 = GUILayout.Toggle(settings.全世界都喜欢太吾, "爱慕不受地理限制");
                GUILayout.EndHorizontal();
            }

            if (settings.主动爱慕 || settings.主动表白 || settings.主动求婚)
            {
                GUILayout.Label("通用配置");
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("接受的关系（指NPC面板中的关系）");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                settings.兄弟姐妹 = GUILayout.Toggle(settings.兄弟姐妹, "兄弟姐妹");
                settings.亲生父母 = GUILayout.Toggle(settings.亲生父母, "亲生父母");
                settings.义父义母 = GUILayout.Toggle(settings.义父义母, "义父义母");
                settings.授业恩师 = GUILayout.Toggle(settings.授业恩师, "授业恩师");
                settings.义结金兰 = GUILayout.Toggle(settings.义结金兰, "义结金兰");
                settings.儿女子嗣 = GUILayout.Toggle(settings.儿女子嗣, "儿女子嗣");
                settings.嫡系传人 = GUILayout.Toggle(settings.嫡系传人, "嫡系传人");
                settings.势不两立 = GUILayout.Toggle(settings.势不两立, "势不两立");
                settings.血海深仇 = GUILayout.Toggle(settings.血海深仇, "血海深仇");
                settings.父系血统 = GUILayout.Toggle(settings.父系血统, "父系血统");
                settings.母系血统 = GUILayout.Toggle(settings.母系血统, "母系血统");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("年龄范围：");
                var 年龄下限输入框 = GUILayout.TextField(settings.年龄下限.ToString(), 3);
                if (GUI.changed && !int.TryParse(年龄下限输入框, out settings.年龄下限))
                {
                    settings.年龄下限 = 14;
                }
                GUILayout.Label("<= 可接受年龄 <=");
                var 年龄上限输入框 = GUILayout.TextField(settings.年龄上限.ToString(), 3);
                if (GUI.changed && !int.TryParse(年龄上限输入框, out settings.年龄上限))
                {
                    settings.年龄上限 = 60;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                接受入邪 = GUILayout.Toggle(接受入邪, "是否接受入邪者");
                接受入魔 = 接受入邪 && GUILayout.Toggle(接受入魔, "是否接受入魔者");
                settings.入魔程度 = 接受入邪 ? (接受入魔 ? 2 : 1 ) : 0;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("立场等级范围：");
                settings.可接受的立场等级[2] = GUILayout.Toggle(settings.可接受的立场等级[2], "刚正");
                settings.可接受的立场等级[1] = GUILayout.Toggle(settings.可接受的立场等级[1], "仁善");
                settings.可接受的立场等级[0] = GUILayout.Toggle(settings.可接受的立场等级[0], "中庸");
                settings.可接受的立场等级[3] = GUILayout.Toggle(settings.可接受的立场等级[3], "叛逆");
                settings.可接受的立场等级[4] = GUILayout.Toggle(settings.可接受的立场等级[4], "唯我");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("好感度等级范围：");
                settings.可接受的好感度等级[-1] = GUILayout.Toggle(settings.可接受的好感度等级[-1], "素未谋面");
                settings.可接受的好感度等级[0] = GUILayout.Toggle(settings.可接受的好感度等级[0], "陌路");
                settings.可接受的好感度等级[1] = GUILayout.Toggle(settings.可接受的好感度等级[1], "冷淡");
                settings.可接受的好感度等级[2] = GUILayout.Toggle(settings.可接受的好感度等级[2], "融洽");
                settings.可接受的好感度等级[3] = GUILayout.Toggle(settings.可接受的好感度等级[3], "热忱");
                settings.可接受的好感度等级[4] = GUILayout.Toggle(settings.可接受的好感度等级[4], "喜欢");
                settings.可接受的好感度等级[5] = GUILayout.Toggle(settings.可接受的好感度等级[5], "亲密");
                settings.可接受的好感度等级[6] = GUILayout.Toggle(settings.可接受的好感度等级[6], "不渝");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("魅力等级范围：");
                settings.可接受的魅力等级[0] = GUILayout.Toggle(settings.可接受的魅力等级[0], "非人");
                settings.可接受的魅力等级[1] = GUILayout.Toggle(settings.可接受的魅力等级[1], "可憎");
                settings.可接受的魅力等级[2] = GUILayout.Toggle(settings.可接受的魅力等级[2], "不扬");
                settings.可接受的魅力等级[4] = settings.可接受的魅力等级[3] = GUILayout.Toggle(settings.可接受的魅力等级[3], "寻常");
                settings.可接受的魅力等级[5] = GUILayout.Toggle(settings.可接受的魅力等级[5], "出众");
                settings.可接受的魅力等级[6] = GUILayout.Toggle(settings.可接受的魅力等级[6], "瑾瑜/瑶碧");
                settings.可接受的魅力等级[7] = GUILayout.Toggle(settings.可接受的魅力等级[7], "龙资/凤仪");
                settings.可接受的魅力等级[8] = GUILayout.Toggle(settings.可接受的魅力等级[8], "绝世/出尘");
                settings.可接受的魅力等级[9] = GUILayout.Toggle(settings.可接受的魅力等级[9], "天人");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("阶层等级范围：");
                settings.可接受的阶层等级[1] = GUILayout.Toggle(settings.可接受的阶层等级[1], "<color=#E4504DFF>一品</color>");
                settings.可接受的阶层等级[2] = GUILayout.Toggle(settings.可接受的阶层等级[2], "<color=#F28234FF>二品</color>");
                settings.可接受的阶层等级[3] = GUILayout.Toggle(settings.可接受的阶层等级[3], "<color=#E3C66DFF>三品</color>");
                settings.可接受的阶层等级[4] = GUILayout.Toggle(settings.可接受的阶层等级[4], "<color=#AE5AC8FF>四品</color>");
                settings.可接受的阶层等级[5] = GUILayout.Toggle(settings.可接受的阶层等级[5], "<color=#63CED0FF>五品</color>");
                settings.可接受的阶层等级[6] = GUILayout.Toggle(settings.可接受的阶层等级[6], "<color=#8FBAE7FF>六品</color>");
                settings.可接受的阶层等级[7] = GUILayout.Toggle(settings.可接受的阶层等级[7], "<color=#6DB75FFF>七品</color>");
                settings.可接受的阶层等级[8] = GUILayout.Toggle(settings.可接受的阶层等级[8], "<color=#FBFBFBFF>八品</color>");
                settings.可接受的阶层等级[9] = GUILayout.Toggle(settings.可接受的阶层等级[9], "<color=#8E8E8EFF>九品</color>");
                GUILayout.EndHorizontal();
            }
            if (settings.主动表白)
            {
                GUILayout.Label("主动表白相关配置");
                GUILayout.BeginHorizontal("Box");
                settings.太吾不爱的人也表白太吾 = GUILayout.Toggle(settings.太吾不爱的人也表白太吾, "主动追求");
                GUILayout.Label("如果选中主动追求，太吾不爱慕的人也会试图表白太吾");
                GUILayout.EndHorizontal();
            }

            if (settings.主动求婚)
            {
                GUILayout.Label("主动求婚相关配置");
                GUILayout.BeginHorizontal("Box");
                settings.已婚人士想和太吾结婚 = GUILayout.Toggle(settings.已婚人士想和太吾结婚, "宁可出轨也要太吾");
                settings.即使太吾已婚别人也想求婚 = GUILayout.Toggle(settings.即使太吾已婚别人也想求婚, "为了太吾甘愿做小");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("Box");
                settings.不顾门派爱太吾 = GUILayout.Toggle(settings.不顾门派爱太吾, "无视门派规章");
                settings.即使出家也要求婚太吾 = GUILayout.Toggle(settings.即使出家也要求婚太吾, "出家难忘太吾");
                settings.即使太吾出家也要求婚 = GUILayout.Toggle(settings.即使太吾出家也要求婚, "太吾出家也要求婚");
                GUILayout.EndHorizontal();
            }
            if (!settings.主动爱慕)
            {
                GUILayout.BeginHorizontal("Box");
                GUILayout.Label("天涯何处无芳草,何必单恋一枝花");
                if (GUILayout.Button("单向爱慕太吾的人忘记这份感情", GUILayout.ExpandWidth(false)))
                {
                    ExpandUtils.RemoveAllLove();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }

    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()

        {

            return null;

        }



        public void ReadXml(System.Xml.XmlReader reader)

        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();



            if (wasEmpty)

                return;



            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)

            {

                reader.ReadStartElement("item");



                reader.ReadStartElement("key");

                TKey key = (TKey)keySerializer.Deserialize(reader);

                reader.ReadEndElement();



                reader.ReadStartElement("value");

                TValue value = (TValue)valueSerializer.Deserialize(reader);

                reader.ReadEndElement();



                this.Add(key, value);



                reader.ReadEndElement();

                reader.MoveToContent();

            }

            reader.ReadEndElement();

        }



        public void WriteXml(System.Xml.XmlWriter writer)

        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            foreach (TKey key in this.Keys)

            {

                writer.WriteStartElement("item");



                writer.WriteStartElement("key");

                keySerializer.Serialize(writer, key);

                writer.WriteEndElement();



                writer.WriteStartElement("value");

                TValue value = this[key];

                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();



                writer.WriteEndElement();

            }

        }

        #endregion

    }
}