using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ArchiveSystem;
using GameData;
using Harmony12;
using UnityEngine;
using UnityModManagerNet;
using Random = UnityEngine.Random;

namespace GayMax
{
    public class Settings : UnityModManager.ModSettings
    {
        public int babyoption;

        public string[] babyoptionText = {"随机", "太吾怀孕", "基/姬友怀孕"};

        public bool brosis;

        //其他
        public int CallMyName = 0;

        //神之宣告，其三
        public bool FFF;
        public bool GalForest;
        public int gongbase = 188000;

        //???哎，真的好长啊
        public int Gongoption;

        public string[] GongoptionText = {"正练", "逆练", "冲解"};

        public bool JadeBoy;

        public bool Jmswzyk;

        //爱之奇迹篇
        public bool JoyOfLove;

        //永恒思念篇*讲真我都不知道这个是来干啥的
        public bool OathOfEternity;

        //79
        public bool OrderDoctor;

        //真基友传说
        public bool PriceOFSalt;
        public bool SameBlood;

        //神之宣告，其一
        public bool SameDream;

        public bool SeteventOverdrive; //控制过季事件的弹出

        public bool SeteventStandBy; //控制事件

        //神之宣告，超地图炮
        public int sexoption;

        public string[] sexoptionText = {"不进行宣告…", "新生儿全是妹子", "新生儿全是汉子", "只有妹子的世界", "只有汉子的世界"};

        public bool ShadiaoEvent;
        public bool SongOfPraises;
        public bool SuperGay;

        //public bool TheRedString = false;
        //基友传说
        public bool Togayther;


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
        public static string picpath;
        public static string txtpath;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;


            //string SingleHappiness = Path.Combine(modEntry.Path, "ExtraHappiness");
            // Logger.Log(" resdir :" + SingleHappiness);
            //BaseResourceMod.Main.registModResDir(modEntry, SingleHappiness);

            txtpath = Path.Combine(modEntry.Path, "ExtraHappiness");
            picpath = Path.Combine(modEntry.Path, @"FakeHappiness\TrunEventImage");

            //BaseResourceMod.Main.registModSpriteDir(modEntry, DoubleHappiness);

            //string picindexpath = Path.Combine(modEntry.Path, @"InMySecretLife\TuenEvenPicIndex.txt");
            //Index.PicIndex = Diction.Read(picindexpath);
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            var instance = DateFile.instance;
            var flag = instance == null || !Characters.HasChar(instance.mianActorId);
            GUILayout.BeginVertical("Box");
            GUILayout.Label("★~基/姬友传说篇~★",
                new GUIStyle {normal = {textColor = new Color(0.999999f, 0.537255f, 0.537255f)}});
            GUILayout.BeginHorizontal();
            settings.Togayther = GUILayout.Toggle(settings.Togayther, "青梅与初代太吾为同性");
            settings.brosis = GUILayout.Toggle(settings.brosis, "且两人都是基/姬佬");
            settings.SuperGay = GUILayout.Toggle(settings.SuperGay, "让世界充满基/姬佬");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            GUILayout.Label("★~爱之奇迹篇~★",
                new GUIStyle {normal = {textColor = new Color(0.999999f, 0.5647058f, 0.3411764f)}});
            GUILayout.BeginHorizontal();
            settings.JoyOfLove = GUILayout.Toggle(settings.JoyOfLove, "获得爱之奇迹");
            settings.SongOfPraises = GUILayout.Toggle(settings.SongOfPraises, "将爱之奇迹传递给NPC");
            settings.SeteventOverdrive =
                GUILayout.Toggle(settings.SeteventOverdrive, "显示季节事件提示");
            settings.SeteventStandBy = GUILayout.Toggle(settings.SeteventStandBy, "显示特殊事件提示");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            settings.babyoption =
                GUILayout.Toolbar(settings.babyoption, settings.babyoptionText, GUILayout.Width(400f));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            GUILayout.Label("★~神之宣告篇~★",
                new GUIStyle {normal = {textColor = new Color(0.9999999f, 0.8078431f, 0.3411764f)}});
            GUILayout.BeginHorizontal();
            settings.SameDream = GUILayout.Toggle(settings.SameDream, "同性的后代与亲代性别相同（最优先）");
            settings.SameBlood = GUILayout.Toggle(settings.SameBlood, "同性的后代仍是基/姬佬");
            settings.Jmswzyk = GUILayout.Toggle(settings.Jmswzyk, "太吾与太吾的基/姬友必生蛐蛐");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.sexoption = GUILayout.Toolbar(settings.sexoption, settings.sexoptionText, GUILayout.Width(600f));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            settings.FFF = GUILayout.Toggle(settings.FFF, "流言蜚语退散");
            settings.GalForest = GUILayout.Toggle(settings.GalForest, "少女派势力参上");
            settings.JadeBoy = GUILayout.Toggle(settings.JadeBoy, "璇男派加入战场");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            GUILayout.Label("★~永恒思念篇~★（施工中………工期无穷大……）",
                new GUIStyle {normal = {textColor = new Color(0.60392157f, 0.8784313f, 0.3607843f)}});
            GUILayout.BeginHorizontal();
            settings.OathOfEternity = GUILayout.Toggle(settings.OathOfEternity, "开启纯爱绘卷");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            GUILayout.Label("★~七九医院开业啦！~★",
                new GUIStyle {normal = {textColor = new Color(0.3607843f, 0.8784313f, 0.6431372f)}});

            GUILayout.BeginHorizontal();
            settings.OrderDoctor = GUILayout.Toggle(settings.OrderDoctor, "现在就打进电话免费预约吧！");
            settings.ShadiaoEvent = GUILayout.Toggle(settings.ShadiaoEvent, "显示蜜汁事件提示");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            GUILayout.Label("★~这是一个不要随便勾选的神秘选项~★",
                new GUIStyle {normal = {textColor = new Color(0.4588235f, 0.89019607f, 0.999999f)}});
            GUILayout.BeginHorizontal();
            settings.PriceOFSalt = GUILayout.Toggle(settings.PriceOFSalt, "真·基/姬友传说");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();


            if (flag)
            {
                GUILayout.Label("★~存档未载入~★");
            }
            else
            {
                GUILayout.BeginVertical("Box");
                GUILayout.Label("★~恋爱最终章·继章~★（升级中………进度无穷小……）",
                    new GUIStyle {normal = {textColor = new Color(0.6666667f, 0.658823f, 0.999999f)}});
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("学习基/姬情神功", GUILayout.Width(120f)))
                    Study.LoveSong();

                if (GUILayout.Button("遗忘神功", GUILayout.Width(120f)))
                    Study.GoodNight();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                settings.Gongoption =
                    GUILayout.Toolbar(settings.Gongoption, settings.GongoptionText, GUILayout.Width(600f));
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                // GUILayout.Label("请输入你的辛运数字", GUILayout.ExpandWidth(false));
                // var mylove = GUILayout.TextField(settings.CallMyName.ToString(), 9, GUILayout.Width(100));
                // if (GUI.changed)
                // {
                //   if (!int.TryParse(mylove, out settings.CallMyName)) { settings.CallMyName = 0; }
                // }
                // int beloved = settings.CallMyName;

                // if (GUILayout.Button("呼唤爱", new GUILayoutOption[] { GUILayout.Width(120f) }))
                // ItemBox.CallOfLove(beloved);

                if (GUILayout.Button("呼唤爱·极", GUILayout.Width(120f)))
                    ItemBox.CallOfLoveMax();

                // if (GUILayout.Button("伪娘伪郎调教计划", new GUILayoutOption[] { GUILayout.Width(200f) }))
                //  ItemBox.SisterReeducation(beloved);

                // if (GUILayout.Button("这不科学", new GUILayoutOption[] { GUILayout.Width(200f) }))
                //ItemBox.Science();

                if (GUILayout.Button("特别鸣谢", GUILayout.Width(200f)))
                    ItemBox.LetterForYou();

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }


    //特别鸣谢@七街城酒[/devil](百毒贴吧) == @qweraaaaa（NGA）大佬，没有他的指点，愚蠢的作者还在愚蠢的寻找春宵代码
    //并且感谢大佬在事件代码上对我的指点。
    //按照约定将感谢信息写进代码里


    //备用目录
    internal struct Memo
    {
        public const int GreatRate = 75, //也许会增添改动功能？
            Friend = 301,
            Sister = 302,
            Parents = 303,
            Fuckdad = 304,
            Teacher = 305,
            Lover = 306,
            JOJO = 307,
            DIO = 308,
            Wife = 309,
            Child = 310,
            Beauty = 312,
            Stu = 311;

        public static List<int> Body = new List<int> {61, 62, 63, 64, 65, 66},
            Skill = new List<int> {501, 502, 503, 504, 505, 506, 507, 508, 509, 510, 511, 512, 513, 514, 515, 516},
            Mind = new List<int> {601, 602, 603, 604, 605, 606, 607, 608, 609, 610, 611, 612, 613, 614},
            FiveGold = new List<int> {1201, 1202, 1203, 1101, 1102, 1103},
            FamilyType = new List<int> {302, 303, 304, 305, 305, 310, 311},
            Jademirror = new List<int>
            {
                1, 7, 13, 19, 25, 31, 37, 43, 49, 55, 61, 67, 73, 79, 85, 91, 97, 103, 109, 115, 121, 127, 133, 139,
                145, 151, 157, 163
            };

        public const string Homo = "7", Bi = "1", Heresy = "0", Boy = "1", Gal = "2";
    }


    //函数公有化一定要实现
    public static class Interfunctional
    {
        public static FieldInfo baseActors = typeof(PeopleLifeAI).GetField("baseActors",
            BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);

        public static MethodInfo AiMoodChange = typeof(PeopleLifeAI).GetMethod("AiMoodChange",
            BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);

        public static MethodInfo AICantMove = typeof(PeopleLifeAI).GetMethod("AICantMove",
            BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
    }

    //写按钮写的意识模糊
    public static class ItemBox
    {
        public static void CallOfLove(int beloved)
        {
            var change = Main.settings.PriceOFSalt && StarFall.CanbeHomo(beloved) ? Memo.Homo : Memo.Bi;
            var isDead = int.Parse(DateFile.instance.GetActorDate(beloved, 26, false)) > 0;
            if (!isDead)
            {
                Characters.SetCharProperty(beloved, 21, change);
                Main.Logger.Log("Your love is delivered.");
            }
            else
            {
                var text = DateFile.instance.GetActorName(beloved, true);
                Main.Logger.Log("[" + text + "]" + "已经死了，" + "[" + text + "]" + "不想变弯，你考虑过" + "[" + text + "]" + "吗？");
                Main.Logger.Log("不！你只考虑你自己");
            }
        }

        public static void CallOfLoveMax()
        {
            var list0 = new List<int>();
            var list1 = new List<int>();
            foreach (var id in Characters.GetAllCharIds())
                if (DateFile.instance.GetActorDate(id, 26, false) == "0")
                {
                    if (StarFall.CanbeHomo(id)) list0.Add(id);
                    else list1.Add(id);
                }

            var change = Main.settings.PriceOFSalt ? Memo.Homo : Memo.Bi;
            foreach (var id in list0) Characters.SetCharProperty(id, 21, change);

            foreach (var id in list1) Characters.SetCharProperty(id, 21, Memo.Bi);

            TailOfStars.Eye = false;
            Main.Logger.Log("同化完了");
        }

        public static void SisterReeducation(int beloved)
        {
            var isDead = int.Parse(DateFile.instance.GetActorDate(beloved, 26, false)) > 0;
            if (!isDead)
            {
                var oao = DateFile.instance.GetActorDate(beloved, 17, false);
                if (oao == "1") Characters.SetCharProperty(beloved, 17, "0");
                if (oao == "0") Characters.SetCharProperty(beloved, 17, "1");
                Main.Logger.Log("调教完了");
            }
            else
            {
                var text = DateFile.instance.GetActorName(beloved, true);
                Main.Logger.Log("[" + text + "]" + "已经死了，" + "[" + text + "]" + "不想被调教，你考虑过" + "[" + text + "]" + "吗？");
                Main.Logger.Log("不！你只考虑你自己");
            }

            if (DateFile.instance.GetActorDate(beloved, 21, false) == "1") Main.Logger.Log("Your love is delivered.");
        }

        public static void LetterForYou() //感谢事件越写越长……挪到这里来吧
        {
            Main.Logger.Log("祝@七街城酒[emoji]大佬逢考必过");
            var jiyou = 0;
            var list2 = LifeLine.GetActorSocial(DateFile.instance.MianActorID(), Memo.Wife);
            if (list2.Count != 0) jiyou = list2[0];

            var actor = jiyou != 0 ? jiyou : DateFile.instance.MianActorID();
            var cardid = Index.EventIndex[31];
            int[] ThanxGift = {0, actor, cardid, actor};

            ui_MessageWindow.Instance.SetEventWindow(ThanxGift);
        }

        public static void ReScience()
        {
            var list0 = new List<int>();
            foreach (var id in Characters.GetAllCharIds())
                if (DateFile.instance.GetActorDate(id, 26, false) == "0" &&
                    DateFile.instance.GetActorDate(id, 8, false) == "1" &&
                    DateFile.instance.GetActorDate(id, 21, false) == "1")
                    list0.Add(id);

            foreach (var id in list0)
                if (!StarFall.DidHomo(id) && Random.Range(0, 10) < 5)
                    Characters.SetCharProperty(id, 21, Memo.Heresy);
        }
    }


    //301莫逆之交 302兄弟姐妹 303亲生父母 304义父义母 305授业恩师 306两情相悦
    //307恩深意重 308义结金兰 309配偶 310子嗣 312倾心爱慕 少311，推测为嫡系传人

    public static class LifeLine
    {
        public static List<int> GetActorSocial(int actorId, int socialTyp, bool getDieActor = false,
            bool getNPC = false)
        {
            var list = new List<int>();
            var list2 = new List<int>(DateFile.instance.GetLifeDateList(actorId, socialTyp));
            foreach (var key in list2)
                if (DateFile.instance.actorSocialDate.ContainsKey(key))
                {
                    var list3 = new List<int>(DateFile.instance.actorSocialDate[key]);
                    foreach (var id in list3)
                        if (id != actorId && !list.Contains(id) &&
                            (getDieActor || DateFile.instance.GetActorDate(id, 26, false) == "0") &&
                            (getNPC || DateFile.instance.GetActorDate(id, 8, false) == "1"))
                            list.Add(id);
                }

            return list;
        }


        //捉奸！*不是老婆的情人和爱慕
        public static List<int> Greenhats(int actorId, bool getdieactor)
        {
            var list0 = new List<int>();
            var list = new List<int>();
            var list2 = new List<int>();
            list0.AddRange(GetActorSocial(actorId, Memo.Wife, getdieactor));
            list.AddRange(GetActorSocial(actorId, Memo.Beauty, getdieactor));
            list.AddRange(GetActorSocial(actorId, Memo.Lover, getdieactor));
            foreach (var id in list)
                if (!list0.Contains(id) && !list2.Contains(id))
                    list2.Add(id);
            return list2;
        }

        //更少情人！*不是老婆的情人
        public static List<int> LessLover(int actorId, bool overhavean)
        {
            var list = new List<int>();
            var list1 = new List<int>();
            var list2 = new List<int>();
            list1.AddRange(GetActorSocial(actorId, Memo.Wife, overhavean));
            list2.AddRange(GetActorSocial(actorId, Memo.Lover, overhavean));
            foreach (var id in list2)
                if (!list1.Contains(id) && !list.Contains(id))
                    list.Add(id);

            if (list1.Count == 0 && list2.Count == 1 && ForbiddenLove(list2[0])) list = new List<int>();
            return list;
        }

        //你的老婆不合法！！！！
        public static int IllegalWife(int actorId, bool overhavean)
        {
            var list0 = new List<int>();
            var list1 = GetActorSocial(actorId, Memo.Wife, overhavean);
            var list2 = GetActorSocial(actorId, Memo.Lover, overhavean);
            if (list1.Count == 0 && list2.Count == 1 && ForbiddenLove(list2[0])) return list2[0];
            return 0;
        }

        //找JOJO！！！
        public static List<int> PhantomBlood(int actorId, bool getDieActor)
        {
            var Josida = GetActorSocial(actorId, Memo.Child, getDieActor);
            var JOJO = new List<int>();

            if (Josida.Count > 0)
                foreach (var id in Josida)
                {
                    var list = GetActorSocial(id, Memo.Parents, true);
                    if (list.Count > 0 && list.Contains(actorId)) JOJO.Add(id);
                }

            return JOJO;
        }

        public static bool ForbiddenLove(int jiyou)
        {
            var gangid = DateFile.instance.GetActorDate(jiyou, 19, false);
            var ganglevel = DateFile.instance.GetActorDate(jiyou, 20, false);
            var gangValueId = DateFile.instance.GetGangValueId(int.Parse(gangid), int.Parse(ganglevel));
            var jadelady = gangid == "8";
            var fivegold = Memo.FiveGold.Contains(gangValueId);
            if (jadelady || fivegold) return true;
            return false;
        }


        public static bool BloodyAbyss(int fatherId, int motherId)
        {
            var para1 = new List<int>();
            var para2 = new List<int>();
            var para3 = new List<int>();
            var para4 = new List<int>();
            para1.AddRange(GetActorSocial(fatherId, Memo.Parents, true));
            para2.AddRange(GetActorSocial(motherId, Memo.Parents, true));
            para3.AddRange(PhantomBlood(fatherId, true));
            para4.AddRange(PhantomBlood(motherId, true));
            para3.AddRange(para1);
            para4.AddRange(para2);
            if (para1.Count > 0 && para2.Count > 0)
                foreach (var id in para1)
                    if (para2.Contains(id))
                        return true;

            if (para3.Contains(motherId)) return true;
            if (para4.Contains(fatherId)) return true;
            return false;
        }
    }

    //太污世界第一家妇产科医院开业啦!
    public static class SevenNineHospital
    {
        public static Dictionary<int, List<int>> BabyBackup;

        public static void BUS(int jiyou) //秘术：胚胎保存
        {
            var six1 = false;
            if (jiyou >= 10001) six1 = DateFile.instance.HaveLifeDate(jiyou, 901);
            if (six1) BabyBackup.Add(jiyou, DateFile.instance.GetLifeDateList(jiyou, 901));
        }
    }

    //一些支援函数
    public static class StarFall
    {
        public static int TheLoverOverHavean(int taiwu, int jiyou, bool purelove)
        {
            var time = 0;
            var forlove3 = new List<int>();
            if (taiwu == jiyou || taiwu <= 0 || jiyou <= 0) return -1;
            if (DateFile.instance.HaveLifeDate(taiwu, 801) && DateFile.instance.HaveLifeDate(jiyou, 801))
            {
                var forlove1 = new List<int>();
                var forlove2 = new List<int>();

                var list1 = DateFile.instance.GetLifeDateList(taiwu, 801, true);
                foreach (var id in list1) forlove1.AddRange(DateFile.instance.GetLifeDateList(id, Memo.Wife));
                var list2 = DateFile.instance.GetLifeDateList(jiyou, 801, true);
                foreach (var id in list2) forlove2.AddRange(DateFile.instance.GetLifeDateList(id, Memo.Wife));
                if (forlove1.Count != 0 && forlove2.Count != 0)
                    foreach (var key in forlove1)
                        if (forlove2.Contains(key))
                            forlove3.Add(key);

                if (forlove3.Count != 0)
                    foreach (var key in forlove3)
                        if (DateFile.instance.actorSocialDate.ContainsKey(key))
                        {
                            var list = new List<int>(DateFile.instance.actorSocialDate[key]);
                            if (list.Count == 2 && PureLove(list[0], list[1])) time++;
                        }
            }

            if (!purelove) return forlove3.Count;
            return time;
        }

        public static bool CanbeHomo(int id)
        {
            var num = 0;
            var sex = DateFile.instance.GetActorDate(id, 14, false);
            var list1 = LifeLine.GetActorSocial(id, Memo.Wife, true);
            list1.AddRange(LifeLine.GetActorSocial(id, Memo.Beauty, true));
            list1.AddRange(LifeLine.GetActorSocial(id, Memo.Lover, true));
            foreach (var key in list1)
            {
                var sex2 = DateFile.instance.GetActorDate(key, 14, false);
                if (sex2 != sex) num++;
            }

            if (num == 0) return true;
            return false;
        }

        public static bool DidHomo(int id)
        {
            var num = 0;
            var sex = DateFile.instance.GetActorDate(id, 14, false);
            var list1 = LifeLine.GetActorSocial(id, Memo.Wife, true);
            list1.AddRange(LifeLine.GetActorSocial(id, Memo.Beauty, true));
            list1.AddRange(LifeLine.GetActorSocial(id, Memo.Lover, true));
            foreach (var key in list1)
            {
                var sex2 = DateFile.instance.GetActorDate(key, 14, false);
                if (sex2 == sex) num++;
            }

            if (num != 0) return true;
            return false;
        }

        public static int FirstTouch(int fatherId, int motherId)
        {
            //301莫逆之交 302兄弟姐妹 303亲生父母 304义父义母 305授业恩师 306两情相悦
            //307恩深意重 308义结金兰 309配偶 310子嗣 312倾心爱慕 少311，推测为嫡系传人
            //肝到意识模糊连关系代码都忘了……总之先memo
            var child = new List<int>();
            child.AddRange(LifeLine.PhantomBlood(motherId, true));
            if (child.Count == 0) return 0;
            var kamipoint = child.Count;
            var yukipoint = child.Count;
            foreach (var id in child)
            {
                if (ChildOfHimeKami(id)) kamipoint--;
                if (LifeLine.GetActorSocial(id, Memo.Parents, true).Contains(fatherId)) yukipoint--;
            }

            if (yukipoint == 0 && kamipoint == 0) return 3;
            if (yukipoint == 0 && kamipoint != 0) return 9; //我们中出了一个异端
            if (kamipoint == 0 && yukipoint != 0) return 99;
            return 999;
        }

        public static bool ChildOfHimeKami(int childId)
        {
            var parent = new List<int>();
            parent.AddRange(LifeLine.GetActorSocial(childId, Memo.Parents, true));
            var kamipoint = 0;
            if (parent.Count > 1)
            {
                var sex = int.Parse(DateFile.instance.GetActorDate(parent[0], 14, false));
                foreach (var id in parent)
                    kamipoint += int.Parse(DateFile.instance.GetActorDate(id, 14, false)) == sex ? 0 : 1;

                if (kamipoint == 0) return true;
            }

            return false;
        }


        /// <summary>
        ///     为了这群女/男装大佬还要专门写个判断程序，好累啊
        /// </summary>
        public static int MountainSea(int taiwu, int jiyou)
        {
            var sex = DateFile.instance.GetActorDate(jiyou, 14, false);
            var taiwufuta = DateFile.instance.GetActorDate(taiwu, 17, false) == "1";
            var loverfuta = DateFile.instance.GetActorDate(jiyou, 17, false) == "1";
            var Iwantosleep = 2;
            if (sex == "1")
            {
                if (taiwufuta && loverfuta) Iwantosleep = 2;
                else if (!taiwufuta && !loverfuta) Iwantosleep = 1;
                else Iwantosleep = 3;
            }
            else
            {
                if (taiwufuta && loverfuta) Iwantosleep = 1;
                else if (!taiwufuta && !loverfuta) Iwantosleep = 2;
                else Iwantosleep = 3;
            }

            return Iwantosleep;
        }

        /// <summary>
        ///     你发现了MOD的隐藏的彩蛋。不幸的是过季将因此变得更卡
        ///     更加不幸的是我又多了一堆getactorsociAL要改
        /// </summary>
        public static bool PureLove(int taiwu, int jiyou)
        {
            var greenhat = LifeLine.Greenhats(jiyou, true);
            var greenhat2 = LifeLine.Greenhats(taiwu, true);
            var wifi = LifeLine.GetActorSocial(taiwu, Memo.Wife, true);
            var has = LifeLine.GetActorSocial(jiyou, Memo.Wife, true);
            var flag0 = wifi.Count == 1 && has.Count == 1;
            var flag1 = greenhat.Count == 0 && greenhat2.Count == 0;
            if (flag0 && flag1) return true;
            var flag3 = false;
            var flag4 = false;
            if (DateFile.instance.HaveLifeDate(taiwu, 801))
            {
                var list1 = DateFile.instance.GetLifeDateList(taiwu, 801, true);
                flag3 = greenhat.Count == 1 && list1.Contains(greenhat[0]);
            }

            if (DateFile.instance.HaveLifeDate(jiyou, 801))
            {
                var list2 = DateFile.instance.GetLifeDateList(jiyou, 801, true);

                flag4 = greenhat2.Count == 1 && list2.Contains(greenhat2[0]);
            }

            var flag5 = flag3 && greenhat2.Count == 0 || flag4 && greenhat.Count == 0 || flag3 && flag4;
            if (flag0 && flag5) return true;

            return false;
        }

        public static bool FixedLove(int taiwu, int jiyou)
        {
            var leg = 1;
            var greenhat = LifeLine.Greenhats(jiyou, false);
            var greenhat2 = LifeLine.Greenhats(taiwu, false);
            var wifi = LifeLine.GetActorSocial(taiwu, Memo.Wife);
            var has = LifeLine.GetActorSocial(jiyou, Memo.Wife);
            var extra = LifeLine.GetActorSocial(jiyou, Memo.Wife, true);
            extra.AddRange(LifeLine.GetActorSocial(taiwu, Memo.Wife, true));
            if (wifi.Count == 1 && has.Count == 1 && greenhat.Count == 0 && greenhat2.Count == 0)
            {
                leg = 0;
            }
            else
            {
                var forbiddenlove = LifeLine.ForbiddenLove(taiwu) || LifeLine.ForbiddenLove(jiyou);
                if (wifi.Count == 0 && has.Count == 0 && greenhat.Count == 1 && greenhat2.Count == 1 &&
                    greenhat.Contains(taiwu) && greenhat2.Contains(jiyou) && forbiddenlove)
                    leg = 0;
            }

            if (extra.Count - wifi.Count - has.Count > 4) leg = 1;
            if (leg == 0) return true;
            return false;
        }


        //{time0, father1, mother2,1,1}
        public static bool FightingGold(int taiwu, int jiyou)
        {
            var sixone = DateFile.instance.HaveLifeDate(taiwu, 901);
            var sixzero = DateFile.instance.HaveLifeDate(jiyou, 901);
            var emm = 0;
            if (sixone)
            {
                var list0 = new List<int>();
                list0.AddRange(DateFile.instance.GetLifeDateList(taiwu, 901));
                if (list0[1] != jiyou) emm++;
            }

            if (sixzero)
            {
                var list0 = new List<int>();
                list0.AddRange(DateFile.instance.GetLifeDateList(jiyou, 901));
                if (list0[1] != taiwu) emm++;
            }

            if (emm == 0) return true;
            return false;
        }


        public static void Wonderfulveryday()
        {
        }

        public static string JadeMirror(int actorid, int dotimes, int motherid, int fatherid, bool extra)
        {
            var array = DateFile.instance.GetActorDate(actorid, 101, false).Split('|');

            var head = new List<int>();
            var head2 = new List<int>();
            var head3 = new List<int>();
            var body = new List<int>();
            var tail = new List<int>();
            var point = dotimes;

            var numlist = Memo.Jademirror;

            var mother = DateFile.instance.GetActorFeature(motherid);
            var father = DateFile.instance.GetActorFeature(fatherid);
            var mother2 = new List<int>();
            var father2 = new List<int>();

            foreach (var id in father)
                if (0 < id && id < 169)
                {
                    var type = (id - 1) / 6 * 6 + 1;
                    father2.Add(type);
                }

            foreach (var id in mother)
                if (0 < id && id < 169)
                {
                    var type = (id - 1) / 6 * 6 + 1;
                    mother2.Add(type);
                }

            foreach (var key in array)
            {
                var id = int.Parse(key);
                if (3000 < id && id < 3025 && !head.Contains(id))
                    head.Add(id);

                if (2000 < id && id < 2013 && !head2.Contains(id))
                    head2.Add(id);

                if (0 < id && id < 169)
                {
                    var type = (id - 1) / 6 * 6 + 1;
                    if (numlist.Contains(type)) numlist.Remove(type);

                    if (mother2.Contains(type) && father2.Contains(type) && !head3.Contains(id))
                        head3.Add(id);
                    else if (!body.Contains(id))
                        body.Add(id);
                }

                if (4004 <= id && id <= 5002 && !tail.Contains(id))
                    tail.Add(id);
            }

            if (body.Count == 0 && head3.Count < 5)
                for (var i = 0; i < dotimes && point > 0; i++)
                {
                    var newid = numlist[Random.Range(0, numlist.Count)];
                    if (!body.Contains(newid))
                    {
                        body.Add(newid);
                        if (numlist.Contains(newid)) numlist.Remove(newid);
                        point--;
                    }
                    else
                    {
                        if (numlist.Contains(newid)) numlist.Remove(newid);
                        i--;
                    }
                }
            else
                for (var i = 0; i < body.Count && point > 0; i++)
                {
                    var id = body[i] % 6;
                    var type = (body[i] - 1) / 6 * 6 + 1;
                    if (id == 0 || id == 4 || id == 5)
                    {
                        body[i] = type;
                        point--;
                    }
                }

            if (head2.Count == 0 && point > 0)
            {
                head2.Add(Random.Range(2001, 2013));
                point--;
            }

            if (point > 0)
                for (var i = 0; i < body.Count && point != 0; i++)
                {
                    var id = body[i] % 6;
                    if (id == 1)
                    {
                        body[i] = body[i] + 1;
                        point--;
                    }
                }

            if (point > 0 && body.Count + head3.Count < 7)
                while (point > 0)

                {
                    var newid = numlist[Random.Range(0, numlist.Count)];
                    if (!body.Contains(newid))
                    {
                        body.Add(newid);
                        if (numlist.Contains(newid)) numlist.Remove(newid);
                        point--;
                    }
                    else
                    {
                        if (numlist.Contains(newid)) numlist.Remove(newid);
                    }
                }

            if (extra && numlist.Contains(37)) body.Add(37);
            var test = "0|4001|9997";
            if (head.Count > 0)
                foreach (var id in head)
                    test = test + "|" + id;

            if (head2.Count > 0)
                foreach (var id in head2)
                    test = test + "|" + id;

            if (head3.Count > 0)
                foreach (var id in head3)
                    test = test + "|" + id;

            if (body.Count > 0)
                foreach (var id in body)
                    test = test + "|" + id;

            if (tail.Count > 0)
                foreach (var id in tail)
                    test = test + "|" + id;

            return test;
        }


        /// <summary>
        ///     这里copy 并 魔改了一份 AISetChildren
        ///     不拦截原函数 [HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
        ///     其他改生育率||突破生育限制的函数对此MOD无效
        /// </summary>
        public static void BondByLove(int fatherId, int motherId, int setFather, int setMother, int partId, int placeId)
        {
            var mianActorId = DateFile.instance.MianActorID();
            var jiyou = 0;
            var istaiwucp = fatherId == mianActorId || motherId == mianActorId;
            if (istaiwucp) jiyou = fatherId == mianActorId ? motherId : fatherId;
            var num = int.Parse(DateFile.instance.GetActorDate(fatherId, 24));
            var num2 = int.Parse(DateFile.instance.GetActorDate(motherId, 24));
            var rootless = DateFile.instance.GetActorFeature(fatherId)
                .Contains(int.Parse(DateFile.instance.GetActorDate(fatherId, 14, false)) != 1 ? 1002 : 1001);
            var rootless2 = DateFile.instance.GetActorFeature(motherId)
                .Contains(int.Parse(DateFile.instance.GetActorDate(motherId, 14, false)) != 1 ? 1002 : 1001);
            if (rootless) num += 1000;
            if (rootless2) num2 += 1000;

            //901=已经怀了……源码里竟然到这一步才判定怀孕，天天对着孕妇干什么啊！！！！！！！
            if (!DateFile.instance.HaveLifeDate(motherId, 901) && Random.Range(0, 15000) < num * num2)
            {
                var num3 = 100;
                var num4 = 0;

                if (!istaiwucp) //传说中的门派计划生育……
                {
                    var num5 = int.Parse(DateFile.instance.GetActorDate(fatherId, 19, false));
                    var gangLevel = int.Parse(DateFile.instance.GetActorDate(fatherId, 20, false));
                    if (setFather == 0)
                    {
                        num5 = int.Parse(DateFile.instance.GetActorDate(motherId, 19, false));
                        gangLevel = int.Parse(DateFile.instance.GetActorDate(motherId, 20, false));
                    }

                    if (num5 != 0 && num5 != 16)
                    {
                        var gangValueId = DateFile.instance.GetGangValueId(num5, gangLevel);
                        var num6 = int.Parse(DateFile.instance.presetGangGroupDateValue[gangValueId][803]);
                        if (num6 > 0)
                        {
                            var count = DateFile.instance.GetGangActor(num5, num6, false).Count;
                            var num7 = int.Parse(DateFile.instance.presetGangGroupDateValue[gangValueId][1]);
                            if (count >= num7 * 2)
                            {
                                if (count >= num7 * 3) return;

                                num4 = 50;
                            }
                        }
                    }
                    else if (num5 == 0)
                    {
                        num3 += 20; //太污村民的生育加成……
                    }
                }

                var num8 = !istaiwucp ? 50 : 25; //传说中的生育下降…

                num4 += LifeLine.PhantomBlood(fatherId, istaiwucp).Count * num8; //子女数量
                num4 += LifeLine.PhantomBlood(motherId, istaiwucp).Count * num8;

                //太污的生育加成……
                if (istaiwucp)
                {
                    var loveP = int.Parse(Characters.GetCharProperty(jiyou, 3));
                    if (FixedLove(fatherId, motherId)) num3 += 15;
                    if (PureLove(fatherId, motherId) && FirstTouch(fatherId, motherId) < 5) num3 += loveP / 1200;
                }

                //神的生育加成……
                if (ChildOfHimeKami(motherId) || ChildOfHimeKami(motherId))
                    num3 += 25;
                //当你凝视BUG，BUG也在凝视你……
                if (LifeLine.BloodyAbyss(fatherId, motherId) && FirstTouch(fatherId, motherId) != 0)
                    return;
                var num9 = num3 - num4;

                if (Random.Range(0, num3) < num9) //经过漫长的判定，可算能生了
                {
                    Interfunctional.AICantMove.Invoke(PeopleLifeAI.instance, new object[] {fatherId});
                    Interfunctional.AICantMove.Invoke(PeopleLifeAI.instance, new object[] {motherId});
                    //杂阳毁阴警告
                    DateFile.instance.ChangeActorFeature(fatherId, 4001, 4002);
                    DateFile.instance.ChangeActorFeature(motherId, 4001, 4002);

                    //身怀六甲警告
                    DateFile.instance.ChangeActorFeature(motherId, 4002, 4003);
                    //心情……还是给加吧
                    Interfunctional.AiMoodChange.Invoke(PeopleLifeAI.instance,
                        new object[] {motherId, Random.Range(0, 11)});
                    Interfunctional.AiMoodChange.Invoke(PeopleLifeAI.instance,
                        new object[] {fatherId, Random.Range(0, 11)});
                    //传说中的蛐蛐绘卷
                    if (istaiwucp && !LifeLine.BloodyAbyss(fatherId, motherId) &&
                        (Random.Range(0, 100) < (DateFile.instance.getQuquTrun - 100) / 10 ||
                         Main.settings.Jmswzyk)) //J神加持，法力无边


                    {
                        DateFile.instance.getQuquTrun = 0;
                        DateFile.instance.actorLife[motherId].Add(901, new List<int>
                        {
                            1042, //是个蛐蛐
                            fatherId,
                            motherId,
                            setFather,
                            setMother
                        });
                    }
                    else
                    {
                        DateFile.instance.actorLife[motherId].Add(901, new List<int>
                        {
                            Random.Range(9, 12), //怀孕时间
                            fatherId,
                            motherId,
                            setFather,
                            setMother
                        });
                    }

                    if (istaiwucp)

                    {
                        if (Main.settings.SeteventOverdrive && partId != 0 && placeId != 0)
                            PeopleLifeAI.instance.aiTrunEvents.Add(new[] //生成过季事件 已经加入自定义事件图标↓
                            {
                                Index.TurnEvenIndex[1 + MountainSea(motherId, fatherId)],
                                partId,
                                placeId,
                                jiyou
                            });

                        if (Main.settings.SeteventStandBy && partId != 0 && placeId != 0) //弹出弹窗事件
                        {
                            var eventid = 0; //9位数 +5位Defult
                            var extra = mianActorId == motherId ? 1 : 2;
                            var fristtouch = FirstTouch(fatherId, motherId);

                            var love = int.Parse(Characters.GetCharProperty(jiyou, 3));
                            if (PureLove(fatherId, motherId))
                            {
                                if (fristtouch == 0 && love > 52500 &&
                                    LifeLine.IllegalWife(mianActorId, false) != jiyou) eventid = 10;
                                else if (fristtouch == 3 && love > 52500) eventid = 14;
                                else eventid = 21;
                            }
                            else
                            {
                                eventid = 24;
                            }

                            eventid += extra;
                            int[] eventbundle = {0, jiyou, Index.EventIndex[eventid], jiyou};
                            UIDate.instance.aiEventDate.Add(eventbundle);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(UIDate), "ChangeTrun")]
    public static class PeopleLifeAI_DoPeopleTrunChange_Patch
    {
        private static void Prefix()
        {
            var taiwu = DateFile.instance.MianActorID();
            SevenNineHospital.BabyBackup = new Dictionary<int, List<int>>();
            SevenNineHospital.BUS(taiwu);
        }
    }

    //把同地块上所有姬/基佬CP都抓来重新春宵，正所谓没有什么是春宵一次不能解决的，如果没有，那就两次
    [HarmonyPatch(typeof(PeopleLifeAI), "DoTrunAIChange")]
    public static class PeopleLifeAi_DoTrunAiChange_Patch
    {
        private static void Prefix(PeopleLifeAI __instance, ref int actorId, ref int mapId, ref int tileId,
            ref int mainActorId, bool isTaiwuAtThisTile, ref int worldId) //等等……这个“面actorId”是什么鬼

        {
            if (!Main.enabled || !isTaiwuAtThisTile) //同地块上的npc
                return;

            var list0 = new List<int>();
            //309=配偶，306=情人，312=爱慕
            list0.AddRange(LifeLine.GetActorSocial(mainActorId, Memo.Wife));
            list0.AddRange(LifeLine.GetActorSocial(mainActorId, Memo.Lover)); //太吾的老婆&情人（活的）一览表
            var Heroticalove = list0.Contains(actorId);
            if (!Heroticalove && !Main.settings.SongOfPraises) return;

            //您的医保已经就位
            if (Heroticalove) SevenNineHospital.BUS(actorId);

            if (Heroticalove && !Main.settings.JoyOfLove) return;

            var sixone = DateFile.instance.HaveLifeDate(actorId, 901);
            //突然的自我
            var age = int.Parse(DateFile.instance.GetActorDate(actorId, 11, false));
            if (sixone || Random.Range(0, 100) >
                int.Parse(DateFile.instance.ageDate[Mathf.Clamp(age, 0, 100)][96]))
                return;

            //打开列表，寻找基/姬友
            var spouse0 = new List<int>();
            var greenhat = new List<int>();
            var darkgreenhat = new List<int>();
            var spouse = new List<int>();
            //List<int> baseActorslist = (List<int>) Interfunctional.baseActors.GetValue(__instance);
            //调用私有变量真累，虽然其实我不知道这是个啥
            // Interfunctional.baseActors.SetValue(__instance, baseActorslist);

            spouse0.AddRange(LifeLine.GetActorSocial(actorId, Memo.Wife));
            greenhat.AddRange(LifeLine.LessLover(actorId, false));
            var GF = LifeLine.IllegalWife(actorId, false);
            var FireForFun = 0;
            if (spouse0.Count != 0)
                foreach (var id in spouse0)
                {
                    //接下来是一大波判定
                    //这个不知道是个啥，应该是同地图NPC
                    // bool flag1 = baseActorslist.Contains(id);
                    //无根之人||石芯玉女警告……等等
                    //bool haveroot = (!DateFile.instance.GetActorFeature(id).Contains((int.Parse(DateFile.instance.GetActorDate(id, 14, false)) != 1) ? 1002 : 1001));
                    //身怀六甲就别来了啊
                    var sixtwo = DateFile.instance.HaveLifeDate(id, 901);
                    //bool isDead = int.Parse(DateFile.instance.GetActorDate(id, 26, false)) > 0;
                    //人物data第2项!=0→出家，和出家人是没有结果的……等等出家人根本不能结婚啊喂
                    var isnotmonk = DateFile.instance.GetActorDate(actorId, 2, false) == "0" &&
                                    DateFile.instance.GetActorDate(id, 2, false) == "0";
                    //异性恋也是没有结果的
                    var isgay = DateFile.instance.GetActorDate(actorId, 14, false) ==
                                DateFile.instance.GetActorDate(id, 14, false);
                    //幼童更是不可能
                    var aged = int.Parse(DateFile.instance.GetActorDate(actorId, 11, false)) > 14 &&
                               int.Parse(DateFile.instance.GetActorDate(id, 11, false)) > 14;
                    if (!sixtwo /*&& flag1*/ && isnotmonk && isgay && aged) spouse.Add(id);

                    if (!isgay) FireForFun++;
                }

            if (greenhat.Count != 0)
                foreach (var id in greenhat)
                {
                    //接下来又是一大波判定
                    //bool flag1 = baseActorslist.Contains(id);
                    var sixtwo = DateFile.instance.HaveLifeDate(id, 901);
                    var isnotmonk = DateFile.instance.GetActorDate(actorId, 2, false) == "0" &&
                                    DateFile.instance.GetActorDate(id, 2, false) == "0";
                    var isgay = DateFile.instance.GetActorDate(actorId, 14, false) ==
                                DateFile.instance.GetActorDate(id, 14, false);
                    var aged = int.Parse(DateFile.instance.GetActorDate(actorId, 11, false)) > 14 &&
                               int.Parse(DateFile.instance.GetActorDate(id, 11, false)) > 14;
                    if (!sixtwo /*&& flag1*/ && isnotmonk && isgay && aged) darkgreenhat.Add(id);

                    if (!isgay) FireForFun++;
                }

            if (GF > 0)
            {
                //bool flag1 = baseActorslist.Contains(GF);
                var sixtwo = DateFile.instance.HaveLifeDate(GF, 901);
                var isnotmonk = DateFile.instance.GetActorDate(actorId, 2, false) == "0" &&
                                DateFile.instance.GetActorDate(GF, 2, false) == "0";
                var isgay = DateFile.instance.GetActorDate(actorId, 14, false) ==
                            DateFile.instance.GetActorDate(GF, 14, false);
                var aged = int.Parse(DateFile.instance.GetActorDate(actorId, 11, false)) > 14 &&
                           int.Parse(DateFile.instance.GetActorDate(GF, 11, false)) > 14;
                if (!isgay) FireForFun++;
                if (sixtwo /*|| !flag1*/ || !isnotmonk || !isgay || !aged)
                    GF = -1;
            }

            if (spouse.Count + darkgreenhat.Count == 0 && GF < 0) return; //找不到人就回去了

            //接下来是找人判定，今晚宠幸谁呢？

            var lovertonight = -1;
            var actorGoodness = DateFile.instance.GetActorGoodness(actorId);
            var mood = int.Parse(DateFile.instance.GetActorDate(actorId, 4, false));

            //见异思迁（ry
            var rate = Math.Max(40,
                Memo.GreatRate + mood / 4 -
                greenhat.Count * int.Parse(DateFile.instance.goodnessDate[actorGoodness][6]));
            var rate2 = int.Parse(DateFile.instance.goodnessDate[actorGoodness][24]) * mood / Memo.GreatRate;
            var rate3 = (int.Parse(DateFile.instance.goodnessDate[actorGoodness][24]) +
                         int.Parse(DateFile.instance.goodnessDate[actorGoodness][23]) * 2) * mood / Memo.GreatRate;
            if (LifeLine.IllegalWife(mainActorId, false) == actorId)
                rate2 += int.Parse(DateFile.instance.goodnessDate[actorGoodness][24]);

            //异端审判
            rate -= FireForFun * 15;
            rate2 -= FireForFun * 20;
            if (lovertonight < 0 && spouse.Count != 0 && Random.Range(0, 100) < rate)
                lovertonight = spouse[Random.Range(0, spouse.Count)];

            if (lovertonight < 0 && GF > 0 && Random.Range(0, 100) < rate3) lovertonight = GF;

            if (lovertonight < 0 && darkgreenhat.Count != 0 && Random.Range(0, 100) < rate2)
                lovertonight = darkgreenhat[Random.Range(0, darkgreenhat.Count)];

            if (lovertonight <= 0) return;

            //可算找到人了，好累

            var istaiwucp = actorId == mainActorId || lovertonight == mainActorId;
            //虽说太吾根本不可能不可能主动……但万一呢……
            var rootless = DateFile.instance.GetActorFeature(actorId)
                .Contains(int.Parse(DateFile.instance.GetActorDate(actorId, 14, false)) != 1 ? 1002 : 1001);
            var rootless2 = DateFile.instance.GetActorFeature(lovertonight)
                .Contains(int.Parse(DateFile.instance.GetActorDate(lovertonight, 14, false)) != 1 ? 1002 : 1001);
            if (rootless && rootless2) //虽然觉得很扯……但……哎起码还是可以拥有爱情吧（ry
            {
                if (istaiwucp)
                    PeopleLifeAI.instance.aiTrunEvents.Add(new[]
                    {
                        234,
                        mapId,
                        tileId,
                        actorId
                    });

                Interfunctional.AICantMove.Invoke(PeopleLifeAI.instance, new object[] {actorId});
                Interfunctional.AICantMove.Invoke(PeopleLifeAI.instance, new object[] {lovertonight});
                Interfunctional.AiMoodChange.Invoke(PeopleLifeAI.instance,
                    new object[] {actorId, Random.Range(-10, 11)});
                Interfunctional.AiMoodChange.Invoke(PeopleLifeAI.instance,
                    new object[] {lovertonight, Random.Range(-10, 11)});
                return;
            }

            //开始分配父母……
            var sex = int.Parse(DateFile.instance.GetActorDate(actorId, 14, false));
            if (rootless && sex == 1 || rootless2 && sex == 2)
            {
                StarFall.BondByLove(lovertonight, actorId, 1, 1, mapId, tileId);
                return;
            }

            if (rootless && sex == 2 || rootless2 && sex == 1)
            {
                StarFall.BondByLove(actorId, lovertonight, 1, 1, mapId, tileId);
                return;
            }

            if (istaiwucp)
            {
                var jiyou = lovertonight == mainActorId ? actorId : lovertonight;
                if (Main.settings.babyoption == 1) StarFall.BondByLove(jiyou, mainActorId, 1, 1, mapId, tileId);
                else if (Main.settings.babyoption == 2) StarFall.BondByLove(mainActorId, jiyou, 1, 1, mapId, tileId);
                else if (Random.Range(0, 100) < 50)
                    StarFall.BondByLove(jiyou, mainActorId, 1, 1, mapId, tileId);
                else StarFall.BondByLove(mainActorId, jiyou, 1, 1, mapId, tileId);
            }
            else
            {
                if (Random.Range(0, 100) < 50)
                    StarFall.BondByLove(actorId, lovertonight, 1, 1, mapId, tileId);
                else
                    StarFall.BondByLove(lovertonight, actorId, 1, 1, mapId, tileId);
            }
        }
    }

    [HarmonyPatch(typeof(DateFile), "SetActorFameList")]
    public static class DateFile_SetActorFameList_Patch
    {
        private static bool Prefix(ref int fameId, ref int fameSize)
        {
            if (!Main.enabled) return true;
            if (Main.settings.FFF && fameId == 401 && fameSize > 0) return false;
            return true;
        }
    }

    //世上本无真正的基佬，直到…………
    [HarmonyPatch(typeof(PeopleLifeAI), "AIGetLove")]
    public static class PeopleLifeAI_AIGetLove_Patch
    {
        private static bool Prefix(DateFile __instance, ref int __result, ref int actorId, ref int loverId)
        {
            if (!Main.enabled) return true;
            var age1 = int.Parse(DateFile.instance.GetActorDate(actorId, 11, false));
            var age2 = int.Parse(DateFile.instance.GetActorDate(loverId, 11, false));
            int result;
            if (age1 <= 3 || age2 <= 3)
            {
                result = 0;
            }
            else
            {
                var taiwu = DateFile.instance.MianActorID();
                var sex1 = int.Parse(DateFile.instance.GetActorDate(actorId, 14, false));
                var sex2 = int.Parse(DateFile.instance.GetActorDate(loverId, 14, false));
                var sohappytogether = sex1 == sex2;
                var istaiwucp = actorId == taiwu || loverId == taiwu;
                var salt = DateFile.instance.GetActorDate(actorId, 21, false);
                var loverate = 0;
                if (salt != "0" && salt != Memo.Homo || !sohappytogether && salt == "0" ||
                    (sohappytogether || !Main.settings.PriceOFSalt) && salt == Memo.Homo)
                    loverate = 20;
                if (loverate > 0)
                {
                    var faith1 = int.Parse(DateFile.instance.GetActorDate(actorId, 209)) / 100 + 30;
                    var faith2 = int.Parse(DateFile.instance.GetActorDate(loverId, 209)) / 100 + 30;

                    loverate += sex1 != 2 ? 0 : 20;
                    loverate += sex1 != sex2 ? 0 : 20;
                    loverate += istaiwucp ? 0 : 20;
                    loverate += (int.Parse(DateFile.instance.GetActorDate(loverId, 15)) - 300) / 10;
                    loverate += 10 - Mathf.Abs(age1 - age2);

                    if (DateFile.instance.HaveLifeDate(actorId, 312)) loverate -= faith1;

                    loverate -= !DateFile.instance.HaveLifeDate(actorId, 306) ? 0 : faith1;
                    loverate -= !DateFile.instance.HaveLifeDate(loverId, 306) ? 0 : faith2;
                    loverate -= !DateFile.instance.HaveLifeDate(actorId, 309) ? 0 : faith1;
                    loverate -= !DateFile.instance.HaveLifeDate(loverId, 309) ? 0 : faith2;
                    loverate -= int.Parse(DateFile.instance.GetActorDate(actorId, 2, false)) == 0 ? 0 : 60;
                    loverate -= int.Parse(DateFile.instance.GetActorDate(loverId, 2, false)) == 0 ? 0 : 60;
                    loverate -= int.Parse(DateFile.instance.GetActorDate(actorId, 24)) != 0 ? 0 : 30;
                    loverate -= int.Parse(DateFile.instance.GetActorDate(loverId, 24)) != 0 ? 0 : 30;
                }

                //“在命运交错的洪流中，你是我唯一的眷恋。”
                if (istaiwucp && !DateFile.instance.HaveLifeDate(actorId, 306) &&
                    !DateFile.instance.HaveLifeDate(loverId, 306) && !DateFile.instance.HaveLifeDate(actorId, 309) &&
                    !DateFile.instance.HaveLifeDate(loverId, 309))
                    loverate += StarFall.TheLoverOverHavean(actorId, loverId, true) * 20;

                result = loverate;
            }

            __result = result;
            return false;
        }
    }

    //好想再玩一玩《幻想三国志四外传》啊……
    //这次一定四连紫丞线（虽然耧澈比较萌）……
    [HarmonyPatch(typeof(MessageEventManager), "EndEvent9001_1")]
    public static class MassageWindow_EndEvent9001_1_Patch
    {
        private static bool Prefix()
        {
            if (!Main.enabled || !Main.settings.PriceOFSalt) return true;

            var casenum = MessageEventManager.Instance.EventValue[1];
            if (casenum != 6) return true;
            var jiyou = MessageEventManager.Instance.MainEventData[1];
            var taiwu = DateFile.instance.MianActorID();
            var actorGoodness = DateFile.instance.GetActorGoodness(jiyou);


            var sex1 = DateFile.instance.GetActorDate(taiwu, 14, false);
            var sex2 = DateFile.instance.GetActorDate(jiyou, 14, false);
            var lovetest = int.Parse(DateFile.instance.GetActorDate(jiyou, 15)) +
                           (10 - Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(jiyou, 20, false)))) * 200;
            var gaygay = sex1 == sex2;
            var pasta = DateFile.instance.GetActorDate(jiyou, 21, false);
            var loverate = 0;
            if (pasta != "0" && pasta != Memo.Homo || !gaygay && pasta == "0" || gaygay && pasta == Memo.Homo)
                loverate = 200;

            if (loverate > 0)
            {
                loverate += sex1 != "2" ? 0 : 300; //妹子加成
                if (!Main.settings.FFF) loverate += sex1 != sex2 ? 0 : 300; //异端加成（好气，审判你）

                if (gaygay && pasta == Memo.Homo) loverate += 400; //我真盖纯姬也要有加成，还要加的比楼上多哼

                loverate += int.Parse(DateFile.instance.GetActorDate(taiwu, 15)); //天人加成
                loverate += DateFile.instance.GetActorFame(taiwu); //妖魔鬼怪加成（雾
                loverate += (10 - Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(taiwu, 11, false)) -
                                            int.Parse(DateFile.instance.GetActorDate(jiyou, 11, false)))) * 20; //同龄加成
                loverate += DateFile.instance.GetActorFavor(false, taiwu, jiyou) / 40; //好感加成
                //又到了喜闻乐见的绿帽判定环节
                var faith1 = int.Parse(DateFile.instance.GetActorDate(taiwu, 209)) / 10 + 300;
                var faith2 = int.Parse(DateFile.instance.GetActorDate(jiyou, 209)) / 10 + 300;
                if (DateFile.instance.HaveLifeDate(jiyou, 312))
                    loverate -= !LifeLine.GetActorSocial(jiyou, 312).Contains(taiwu) ? faith2 : -300;

                loverate -= !DateFile.instance.HaveLifeDate(taiwu, 306) ? 0 : faith1;
                loverate -= !DateFile.instance.HaveLifeDate(jiyou, 306) ? 0 : faith2;
                loverate -= !DateFile.instance.HaveLifeDate(taiwu, 309) ? 0 : faith1;
                loverate -= !DateFile.instance.HaveLifeDate(jiyou, 309) ? 0 : faith2;
                loverate -= int.Parse(DateFile.instance.GetActorDate(taiwu, 2, false)) == 0 ? 0 : 600;
                loverate -= int.Parse(DateFile.instance.GetActorDate(jiyou, 2, false)) == 0 ? 0 : 600;
                loverate -= int.Parse(DateFile.instance.GetActorDate(taiwu, 24)) != 0 ? 0 : 300;
                loverate -= int.Parse(DateFile.instance.GetActorDate(jiyou, 24)) != 0 ? 0 : 300;
            }

            if (!DateFile.instance.HaveLifeDate(jiyou, 306) && !DateFile.instance.HaveLifeDate(jiyou, 306) &&
                !DateFile.instance.HaveLifeDate(jiyou, 309) && !DateFile.instance.HaveLifeDate(jiyou, 309))
            {
                loverate += StarFall.TheLoverOverHavean(taiwu, jiyou, true) * 200;
            }

            if (loverate >= lovetest)
            {
                var SetLoveSocial = typeof(MessageEventManager).GetMethod("SetLoveSocial",
                    BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
                SetLoveSocial.Invoke(MessageEventManager.Instance,
                    new object[] {taiwu, jiyou, 9201 + actorGoodness});
                DateFile.instance.ChangeActorLifeFace(jiyou, 0, 2);
            }
            else
            {
                DateFile.instance.AddSocial(taiwu, jiyou, 312);
                DateFile.instance.SetActorMood(taiwu, -10);
                EndEventChangeMassageWindow(9206 + actorGoodness);
            }

            /*if (UIMove.instance.movePlaceActorIn)
            {
                WorldMapSystem.instance.UpdatePlaceActor(WorldMapSystem.instance.choosePartId,
                    WorldMapSystem.instance.choosePlaceId);
            }*/

            return false;
        }

        private static void EndEventChangeMassageWindow(int eventId)
        {
            MessageEventManager.Instance.MainEventData[2] = eventId; //{0 , jiyou , eventid , jiyou}
            MessageEventManager.Instance.EventValue = new List<int>();
        }
    }

    //時よ、止せ！
    //说的这么炫酷，其实只是为了拦截起名事件……(暂时)
    //0=0; 1=ma; 2=id; 3=ma; 4=child;
    [HarmonyPatch(typeof(MessageEventManager), "DoEvent")]
    public static class MassageWindow_DoEvent_Patch
    {
        public static int taiwu = DateFile.instance.MianActorID();

        private static void Prefix(ref int eventIndex)
        {
            if (!Main.settings.OathOfEternity || !Main.enabled) return;
            if (DateFile.instance.eventId[eventIndex][2] == 226 || DateFile.instance.eventId[eventIndex][2] == 227)
            {
                var sexfix = DateFile.instance.eventId[eventIndex][2] % 2;
                var identityfix = 1;
                var motherid = DateFile.instance.eventId[eventIndex][3];
                var fatherid = taiwu;
                List<int> savebaby;
                if (motherid == taiwu && SevenNineHospital.BabyBackup.TryGetValue(motherid, out savebaby))
                {
                    identityfix = 3;
                    fatherid = savebaby[1];
                    if (DateFile.instance.GetActorDate(motherid, 14, false) ==
                        DateFile.instance.GetActorDate(fatherid, 14, false))
                    {
                        DateFile.instance.eventId[eventIndex][1] = fatherid;
                        DateFile.instance.eventId[eventIndex][3] = fatherid;
                    }
                }

                if (DateFile.instance.GetActorDate(motherid, 14, false) ==
                    DateFile.instance.GetActorDate(fatherid, 14, false))
                {
                    DateFile.instance.eventId[eventIndex][2] = Index.EventIndex[50 + sexfix + identityfix];
                }
            }
        }
    }

    //神は言っている、ここで死ぬ定めではないと・・・！
    //神说：你还不可以死在这里。。。！
    [HarmonyPatch(typeof(UIDate), "SetTrunEvent")]
    public static class UiDate_SetTrunEvent_Patch
    {
        public static void Prefix(int _eventId)
        {
            if (!Main.settings.OrderDoctor || !Main.enabled) return;
            var taiwu = DateFile.instance.MianActorID();
            var end = UIDate.instance.changTrunEvents.Count;
            for (var id = end - 1; id >= 0; id--)
            {
                if (UIDate.instance.changTrunEvents[id][0] == 254) //检测到事故发生……
                {
                    var patient = UIDate.instance.changTrunEvents[id][3]; //检测到一位病人……
                    List<int> savebaby;
                    if (SevenNineHospital.BabyBackup.TryGetValue(patient, out savebaby) && !savebaby.Contains(79)
                    ) //查找预约信息，并判定是否进行救助……
                    {
                        if (DateFile.instance.actorLife[patient].ContainsKey(901)) //进行手术……
                        {
                            foreach (var babyId in savebaby)
                            {
                                if (!DateFile.instance.actorLife[patient][901].Contains(babyId))
                                {
                                    DateFile.instance.actorLife[patient][901].Add(babyId);
                                }
                            }
                        }
                        else
                        {
                            DateFile.instance.actorLife[patient].Add(901, savebaby);
                        }

                        DateFile.instance.ChangeActorFeature(patient, 4002, 4003);
                        var setfather = savebaby[3] == 1; //对相关人员进行记忆消除……
                        var fatherid = savebaby[1];
                        if (setfather && fatherid != 0)
                        {
                            var fatherLifeRecords = LifeRecords.GetAllRecords(fatherid);
                            for (var i = fatherLifeRecords.Length - 1; i >= 0; i--)
                            {
                                if (fatherLifeRecords[i].messageId == 106)
                                {
                                    var fatherLifeRecordsList = fatherLifeRecords.ToList();
                                    fatherLifeRecordsList.RemoveAt(i);
                                    fatherLifeRecords = fatherLifeRecordsList.ToArray();
                                    LifeRecords.RemoveRecords(fatherid);
                                    LifeRecords.AddRecords(fatherid, fatherLifeRecords);
                                    break;
                                }
                            }

                            DateFile.instance.SetActorMood(fatherid, 25);
                        }

                        var patientLifeRecords = LifeRecords.GetAllRecords(patient);
                        for (var i = patientLifeRecords.Length - 1; i >= 0; i--)
                        {
                            if (patientLifeRecords[i].messageId == 105)
                            {
                                var patientLifeRecordsList = patientLifeRecords.ToList();
                                patientLifeRecordsList.RemoveAt(i);
                                patientLifeRecords = patientLifeRecordsList.ToArray();
                                LifeRecords.RemoveRecords(patient);
                                LifeRecords.AddRecords(patient, patientLifeRecords);
                                break;
                            }
                        }

                        DateFile.instance.SetActorMood(patient, 50);
                        UIDate.instance.changTrunEvents[id][0] = Index.TurnEvenIndex[5]; //报告手术结果……
                        DateFile.instance.actorLife[patient][901].Add(79); //记录相关讯息……
                        Main.Logger.Log("手术成功！");
                        if (Main.settings.ShadiaoEvent && patient == taiwu && setfather &&
                            StarFall.PureLove(taiwu, fatherid)) //判定是否生成沙雕事件……
                        {
                            int[] eventbundle = {0, fatherid, Index.EventIndex[71], fatherid};
                            DateFile.instance.eventId.Insert(0, eventbundle);
                        }
                    }
                }
            }
        }
    }


    //一切,都是十二IF双螺旋大阵的选择……
    [HarmonyPatch(typeof(MessageEventManager), "EndEvent")]
    public static class MassageWindow_EndEvent_Patch
    {
        public static int taiwu = DateFile.instance.MianActorID();

        private static void Prefix()
        {
            if (MessageEventManager.Instance.EventValue.Count > 0 && MessageEventManager.Instance.EventValue[0] != 0)

            {
                var key0 = MessageEventManager.Instance.EventValue[0]; //只占用一个EventValue
                if (key0 == 8010)
                {
                    var key = MessageEventManager.Instance.EventValue[1];
                    switch (key) //为了将来不变成另一个十二IF大阵，先弄个万化十四Case
                    {
                        case 8010:
                            EndEvent_8010();
                            break;
                        // case (8011):
                        // EndEvent_8011_LetterForYou(); break;
                        case 8012:
                            EndEvent_8012_TheBizarreColourChangingOfSixStreetCityWine();
                            break;
                        case 8013:
                            EndEvent8013_CallMyName();
                            break;
                    }
                }
            }
        }

        //秘术：结局跳转
        private static void EndEventChangeMassageWindow(int eventId)
        {
            MessageEventManager.Instance.MainEventData[2] = eventId; //{0 , jiyou , eventid , jiyou}
            MessageEventManager.Instance.EventValue = new List<int>();
        }

        private static void EndEvent_8010() //测试用分支，第一次尝试似乎成功了呢&&胎教事件备用
        {
            var num = MessageEventManager.Instance.MainEventData[1]; //jiyou id   
            var num2 = DateFile.instance.MianActorID();
            var num3 = MessageEventManager.Instance.EventValue[2]; //8010 & X
            if (DateFile.instance.HaveLifeDate(num, 901))
                DateFile.instance.actorLife[num][901].Add(num3);
        }


        private static void EndEvent_8012_TheBizarreColourChangingOfSixStreetCityWine() //@七街城酒[/devil]的奇妙变色
        {
            var actor = MessageEventManager.Instance.MainEventData[1];
            var num = MessageEventManager.Instance.EventValue[2];
            var num2 = Index.EventIndex[30];
            if (Random.Range(0, 10) < 1)
            {
                num2 = Index.EventIndex[34]; //喊！全都给我喊大佬流啤！
            }
            else
            {
                if (num == 2) num2 = Index.EventIndex[30];
                if (num == 0) num2 = Index.EventIndex[32];
                if (num == 3) num2 = Index.EventIndex[33];
            }

            EndEventChangeMassageWindow(num2);
        }

        private static void EndEvent8013_CallMyName()
        {
            var xxx = MessageEventManager.Instance.EventValue[2]; //end&…… & X
            var jiyou = MessageEventManager.Instance.MainEventData[3];
            var taiwu = DateFile.instance.MianActorID();
            var child = MessageEventManager.Instance.MainEventData[4];
            var id = xxx == 1 ? jiyou : taiwu;
            Characters.RemoveCharProperty(child, 5);
            Characters.RemoveCharProperty(child, 29);
            DateFile.instance.MakeActorName(MessageEventManager.Instance.MainEventData[4],
                int.Parse(DateFile.instance.GetActorDate(id, 29, false)), DateFile.instance.GetActorDate(id, 5));
        }
    }

    /// <summary>
    ///     受け継ぐ愛を さだめと呼ぶなら……
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MakeNewChildren")]
    public static class DateFile_MakeNewChildren_Patch
    {
        private static void Postfix(DateFile __instance, ref List<int> __result, ref int fatherId, ref int motherId,
            ref bool setFather, ref bool setMother, ref List<int> childrenValue)
        {
            if (!Main.enabled) return;
            foreach (var childId in __result)
            {
                var baseActorId = int.Parse(DateFile.instance.GetActorDate(childId, 997, false));
                var sex1 = DateFile.instance.presetActorDate[baseActorId][14];
                if (Main.settings.sexoption == 1)
                {
                    if (sex1 != Memo.Gal)
                    {
                        Characters.SetCharProperty(childId, 997, Mathf
                            .Clamp(int.Parse(Characters.GetCharProperty(childId, 997)) + 1, 2, 32).ToString());
                        /*__instance.actorsDate[childId][997] = Mathf
                            .Clamp((int.Parse(__instance.actorsDate[childId][997]) + 1), 2, 32).ToString();*/
                        __instance.MakeActorName(childId, int.Parse(__instance.GetActorDate(childId, 29, false)),
                            __instance.GetActorDate(childId, 5, false));
                    }
                }
                else if (Main.settings.sexoption == 2)
                {
                    if (sex1 != Memo.Boy)
                    {
                        Characters.SetCharProperty(childId, 997, Mathf
                            .Clamp(int.Parse(Characters.GetCharProperty(childId, 997)) - 1, 1, 31).ToString());
                        /*__instance.actorsDate[childId][997] = Mathf
                            .Clamp((int.Parse(__instance.actorsDate[childId][997]) - 1), 1, 31).ToString();*/
                        __instance.MakeActorName(childId, int.Parse(__instance.GetActorDate(childId, 29, false)),
                            __instance.GetActorDate(childId, 5, false));
                    }
                }

                if (!setFather || !setMother || fatherId <= 0 || motherId <= 0) continue;
                var taiwu = DateFile.instance.MianActorID();
                var jiyou = 10001; //人在家里躺，锅从天上来
                var ChildOfTaiwu = motherId == taiwu || fatherId == taiwu;
                var sex3 = DateFile.instance.GetActorDate(fatherId, 14, false);
                var ChildOfHimeKami = sex3 == DateFile.instance.GetActorDate(motherId, 14, false);
                if (!ChildOfHimeKami) continue;
                if (!Main.settings.SuperGay && Main.settings.SameBlood)
                {
                    var change = Main.settings.PriceOFSalt ? Memo.Homo : Memo.Bi;
                    Characters.SetCharProperty(childId, 21, change);
                }

                if (Main.settings.SameDream)
                {
                    var baseactor2 = int.Parse(Characters.GetCharProperty(childId, 997));
                    var sex2 = DateFile.instance.presetActorDate[baseactor2][14];
                    if (sex2 != sex3)
                    {
                        Characters.SetCharProperty(childId, 997,
                            DateFile.instance.GetActorDate(fatherId, 997, false));
                        __instance.MakeActorName(childId, int.Parse(__instance.GetActorDate(childId, 29, false)),
                            __instance.GetActorDate(childId, 5, false));
                    }
                }

                Characters.SetCharProperty(childId, 15, Math.Min(900,
                        int.Parse(Characters.GetCharProperty(childId, 15)) + Random.Range(50, 120))
                    .ToString());
                var FFF = 10001; //人在家里躺，锅又从天上来
                var MMM = 10001;
                var F1 = 10001;
                var M1 = 10001;
                if (ChildOfTaiwu)
                {
                    jiyou = motherId == taiwu ? fatherId : motherId;
                    var sex = DateFile.instance.GetActorDate(taiwu, 14, false);
                    if (sex == Memo.Boy)
                    {
                        FFF = taiwu;
                        MMM = jiyou;
                    }
                    else
                    {
                        FFF = jiyou;
                        MMM = taiwu;
                    }
                }
                else
                {
                    FFF = Math.Max(fatherId, motherId);
                    MMM = Math.Min(fatherId, motherId);
                }

                F1 = DateFile.instance.GetLifeDate(FFF, 601, 0);
                M1 = DateFile.instance.GetLifeDate(MMM, 602, 0);

                DateFile.instance.actorLife[childId][601][0] = F1 > 0 ? F1 : FFF;
                DateFile.instance.actorLife[childId][602][0] = M1 > 0 ? M1 : MMM;
                if (!ChildOfTaiwu) continue;
                var LoveIsForever = StarFall.PureLove(fatherId, motherId);
                var fristt = StarFall.FirstTouch(fatherId, motherId);
                if (LoveIsForever && fristt < 5)
                {
                    var loveP = int.Parse(Characters.GetCharProperty(jiyou, 3));
                    var dotime = 1;
                    var extra = false;

                    if (loveP >= 52500)
                    {
                        foreach (var key in Memo.Body)
                            Characters.SetCharProperty(childId, key,
                                (int.Parse(Characters.GetCharProperty(childId, key)) +
                                 Random.Range(2, 10)).ToString());

                        foreach (var key in Memo.Skill)
                            Characters.SetCharProperty(childId, key,
                                (int.Parse(Characters.GetCharProperty(childId, key)) +
                                 Random.Range(5, 10)).ToString());

                        foreach (var key in Memo.Mind)
                            Characters.SetCharProperty(childId, key,
                                (int.Parse(Characters.GetCharProperty(childId, key)) +
                                 Random.Range(5, 10)).ToString());
                    }

                    if (loveP >= 60000) dotime += 1;
                    Main.Logger.Log("7");
                    if (childrenValue != null && childrenValue.Count >= 6)
                        foreach (var id in childrenValue)
                        {
                            if (id == 80) dotime++;
                            if (id == 81) extra = true;
                        }

                    if (dotime > 1)
                    {
                        var fea = StarFall.JadeMirror(childId, dotime, motherId, fatherId, extra);
                        /*if (DateFile.instance.ActorFeaturesGetCache(childId, out __result))
                                {
                                    DateFile.instance.ActorFeaturesCacheReset(childId);
                                }*/
                        Characters.SetCharProperty(childId, 101, fea);
                    }
                }
            }
        }
    }

    //防BUG补丁
    [HarmonyPatch(typeof(DateFile), "MakeGangActor")]
    public static class DateFile_MakeGangActor_Patch
    {
        private static void Prefix(ref int baseActorId)
        {
            if (baseActorId == 0) baseActorId = 1;
            if (baseActorId == 33) baseActorId = 32;
        }
    }

    //茄茄的奇妙生物学
    //"DoActorMake"过于地图炮
    //然后说着就抄起了地图炮（
    [HarmonyPatch(typeof(DateFile), "DoActorMake")]
    public static class DateFile_DoActorMake_Patch
    {
        private static void Postfix(DateFile __instance, ref int baseActorId, ref int actorId)
        {
            if (!Main.enabled || baseActorId < 1 || baseActorId > 32) return;


            if (Main.settings.SuperGay)
            {
                if (!Main.settings.PriceOFSalt)
                    Characters.SetCharProperty(actorId, 21, Memo.Bi);
                else
                    Characters.SetCharProperty(actorId, 21, Memo.Homo);
            }
            else if (Main.settings.PriceOFSalt && DateFile.instance.GetActorDate(actorId, 21, false) == Memo.Bi)
            {
                var random = new System.Random();
                if (random.Next(0, 100) < 15) Characters.SetCharProperty(actorId, 21, Memo.Homo);
            }

            var sex1 = DateFile.instance.presetActorDate[baseActorId][14];
            if (actorId != 10001)
            {
                if (Main.settings.sexoption == 3)
                {
                    if (sex1 != Memo.Gal)
                        Characters.SetCharProperty(actorId, 997, Mathf
                            .Clamp(int.Parse(Characters.GetCharProperty(actorId, 997)) + 1, 2, 32).ToString());
                }
                else if (Main.settings.sexoption == 4)
                {
                    if (sex1 != Memo.Boy)
                        Characters.SetCharProperty(actorId, 997, Mathf
                            .Clamp(int.Parse(Characters.GetCharProperty(actorId, 997)) - 1, 1, 31).ToString());
                }
            }

            var change = Main.settings.PriceOFSalt ? Memo.Homo : Memo.Bi;

            if (actorId == 10001)
                if (Main.settings.brosis)
                    Characters.SetCharProperty(10001, 21, change);

            if (actorId == 10003)
            {
                if (Main.settings.brosis) Characters.SetCharProperty(10003, 21, change);

                if (Main.settings.Togayther)
                {
                    var sex = DateFile.instance.GetActorDate(10001, 997, false);
                    if (sex != "0")
                        Characters.SetCharProperty(10003, 997, sex);
                    else
                        Characters.SetCharProperty(10003, 997, Random.Range(0, 14) * 2 + sex);

                    DateFile.instance.MakeActorName(10003);
                }
            }
        }
    }

    //少女璇男改造计划，启动！
    [HarmonyPatch(typeof(DateFile), "GangActorLevelChange")]
    public static class DateFile_GangActorLevelChange_Patch
    {
        private static bool Prefix(DateFile __instance, ref int __result, ref int hostActorId, ref int gangId,
            ref int targetLevel)
        {
            if (!Main.enabled || !Main.settings.GalForest && !Main.settings.JadeBoy) return true;
            var taiwu = DateFile.instance.MianActorID();
            var gangValueId = __instance.GetGangValueId(gangId, targetLevel);
            var sexorder = int.Parse(__instance.presetGangGroupDateValue[gangValueId][101]);
            var list = new List<int>();
            if (__instance.GetGangDate(gangId, 14) == "1")
            {
                var inheritortype = new List<int>
                {
                    311,
                    310,
                    302,
                    309,
                    308
                };
                foreach (var socialTyp in inheritortype)
                {
                    if (list.Count > 0) break;
                    var inheritorlist =
                        new List<int>(LifeLine.GetActorSocial(hostActorId, socialTyp));
                    if (inheritorlist.Count > 0)
                        foreach (var inheritor in inheritorlist)
                            if (inheritor != hostActorId && inheritor != taiwu &&
                                int.Parse(__instance.GetActorDate(inheritor, 6, false)) == 0 &&
                                int.Parse(__instance.GetActorDate(inheritor, 11, false)) > 3 &&
                                int.Parse(__instance.GetActorDate(inheritor, 19, false)) == gangId &&
                                Mathf.Abs(int.Parse(__instance.GetActorDate(inheritor, 20, false))) > targetLevel &&
                                (Main.settings.JadeBoy && sexorder == 2 ||
                                 Main.settings.GalForest && sexorder == 1 || sexorder == 0 ||
                                 int.Parse(__instance.GetActorDate(inheritor, 14, false)) == sexorder))
                                list.Add(inheritor);
                }
            }

            if (list.Count <= 0)
                for (var k = targetLevel; k < 9; k++)
                {
                    if (list.Count > 0) break;

                    var list3 = new List<int>(DateFile.instance.GetGangActor(gangId, k + 1, false));
                    if (list3.Count > 0)
                        foreach (var luckydog in list3)
                            if (luckydog != hostActorId && luckydog != taiwu &&
                                int.Parse(__instance.GetActorDate(luckydog, 6, false)) == 0 &&
                                (Main.settings.JadeBoy && sexorder == 2 ||
                                 Main.settings.GalForest && sexorder == 1 || sexorder == 0 ||
                                 int.Parse(__instance.GetActorDate(luckydog, 14, false)) == sexorder))
                                list.Add(luckydog);
                }

            var result = 0;
            if (list.Count > 0)
            {
                var abliitymax = 0;
                foreach (var fighter in list)
                {
                    var ability = gangId == 8
                        ? int.Parse(__instance.GetActorDate(fighter, 15))
                        : int.Parse(__instance.GetActorDate(fighter, 993, false));
                    if (ability > abliitymax)
                    {
                        abliitymax = ability;
                        result = fighter;
                    }
                }
            }

            __result = result;
            return false;
        }
    }


    //这破MOD到底要写多长啊！？？！？！？
    public static class Study
    {
        public static void LoveSong()
        {
            if (!Main.enabled || Index.GongFaIndex.Keys.Count < 1) return;
            var taiwu = DateFile.instance.MianActorID();
            if (DateFile.instance.GetActorDate(taiwu, 21, false) == Memo.Heresy)
            {
                var change = Main.settings.PriceOFSalt && StarFall.CanbeHomo(taiwu) ? Memo.Homo : Memo.Bi;
                Characters.SetCharProperty(taiwu, 21, change);
            }

            if (!DateFile.instance.actorGongFas[taiwu].Keys.Contains(Index.GongFaIndex[1]))
            {
                int[] mew = {100, 0, 0};
                DateFile.instance.actorGongFas[taiwu].Add(Index.GongFaIndex[1], mew);
            }
        }

        public static void GoodNight()
        {
            if (!Main.enabled || Index.GongFaIndex.Keys.Count < 1) return;
            var taiwu = DateFile.instance.MianActorID();
            if (DateFile.instance.actorGongFas[taiwu].Keys.Contains(Index.GongFaIndex[1]) &&
                !DateFile.instance.GetActorEquipGongFa(taiwu)[0].Contains(Index.GongFaIndex[1]))
                DateFile.instance.actorGongFas[taiwu].Remove(Index.GongFaIndex[1]);
        }
    }

    [HarmonyPatch(typeof(DateFile), "GetGongFaLevel")]
    public static class DateFile_GetGongFaLevel_Patch
    {
        private static void Postfix(ref int __result, ref int gongFaId, ref int index)
        {
            if (!Main.enabled || Index.GongFaIndex.Keys.Count < 1) return;
            if (gongFaId == Index.GongFaIndex[1] && index == 2)
            {
                if (Main.settings.Gongoption == 1) __result = 6;
                else if (Main.settings.Gongoption == 2) __result = 5;
            }
        }
    }

    [HarmonyPatch(typeof(DateFile), "GetGongFaFLevel")]
    public static class DateFile_GetGongFaFLevel_Patch
    {
        private static void Postfix(ref int __result, ref int gongFaId, ref bool getSkillAdd)
        {
            if (!Main.enabled || Index.GongFaIndex.Keys.Count < 1) return;
            if (gongFaId == Index.GongFaIndex[1])
            {
                if (!getSkillAdd) __result = 10;
                else __result = 0;
            }
        }
    }

    [HarmonyPatch(typeof(BattleVaule), "GetAttackDef")]
    public static class BattleVaule_GetAttackDef_Patch
    {
        private static void Postfix(ref int __result, ref int attackActorId, ref int defActorId, ref int gongFaId,
            ref bool isActor, ref int defTyp)
        {
            if (!Main.enabled || Index.GongFaIndex.Keys.Count < 1) return;
            var id = int.Parse(DateFile.instance.GetActorDate(defActorId, 997, false));
            var id2 = int.Parse(DateFile.instance.GetActorDate(attackActorId, 997, false));
            var sex1 = DateFile.instance.GetActorDate(attackActorId, 21, false);

            if (gongFaId == Index.GongFaIndex[1] && id2 <= 32 && id2 >= 1 && defTyp == 1 && sex1 != Memo.Heresy)
            {
                var gongFaFTyp = DateFile.instance.GetGongFaFTyp(attackActorId, gongFaId);
                if (id <= 32 && id >= 1)
                {
                    if (gongFaFTyp == 0)
                    {
                        var sex2 = DateFile.instance.GetActorDate(defActorId, 21, false);
                        var flag = StarFall.CanbeHomo(defActorId);
                        if (sex2 == Memo.Heresy || Main.settings.PriceOFSalt && sex2 == Memo.Bi && flag)
                        {
                            var change = Main.settings.PriceOFSalt && flag ? Memo.Homo : Memo.Bi;
                            if (Characters.HasChar(defActorId))
                            {
                                Characters.SetCharProperty(defActorId, 21, change);
                                var efid = Index.GongFaPowerIndex[1][0];
                                BattleSystem.instance.ShowBattleState(efid,
                                    int.Parse(DateFile.instance.gongFaFPowerDate[efid][5]) == 0
                                        ? isActor
                                        : !isActor);
                            }
                        }
                    }
                    else if (gongFaFTyp == 1)
                    {
                        var futa1 = DateFile.instance.GetActorDate(attackActorId, 17, false);
                        var futa2 = DateFile.instance.GetActorDate(defActorId, 17, false);
                        if (Characters.HasChar(defActorId) && futa1 != futa2)
                        {
                            Characters.SetCharProperty(defActorId, 17, futa1);
                            var efid = Index.GongFaPowerIndex[1][1];
                            BattleSystem.instance.ShowBattleState(efid,
                                int.Parse(DateFile.instance.gongFaFPowerDate[efid][5]) == 0
                                    ? isActor
                                    : !isActor);
                        }
                    }
                }

                var wife = LifeLine.GetActorSocial(attackActorId, Memo.Wife, true);
                if (wife.Count != 0)
                {
                    var jiyou = wife[0];
                    var flag = false;
                    var gong = new List<int>(DateFile.instance.GetActorEquipGongFa(attackActorId)[0]);

                    foreach (var gid in gong)
                        if (DateFile.instance.gongFaDate[gid][0].Equals("永结同心双飞愿"))
                        {
                            if (DateFile.instance.GetGongFaFTyp(attackActorId, gid) == 0) flag = true;

                            break;
                        }

                    if (flag && DateFile.instance.GetActorDate(attackActorId, 14, false) ==
                        DateFile.instance.GetActorDate(jiyou, 14, false) && StarFall.PureLove(attackActorId, jiyou) &&
                        !wife.Contains(defActorId))
                    {
                        var SetRealDamage = typeof(BattleSystem).GetMethod("SetRealDamage",
                            BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
                        var num3 = BattleSystem.instance.ActorId(!isActor);
                        SetRealDamage.Invoke(BattleSystem.instance,
                            new object[] {!isActor, 1, 48, 5200, num3, 2.2f, false});
                        var efid = Index.GongFaPowerIndex[2][0];
                        BattleSystem.instance.ShowBattleState(efid,
                            int.Parse(DateFile.instance.gongFaFPowerDate[efid][5]) == 0
                                ? isActor
                                : !isActor);
                    }
                }
            }
        }
    }

    //所以这破MOD到底要写多长啊！？！？？！？！？！？？！？！？
    internal struct TailOfStars
    {
        public static double num00;
        public static bool Eye;
    }


    public class ICYou
    {
        public ICYou()
        {
            CountingStar();
        }

        public static ICYou instance { get; private set; }

        public static void Initialize()
        {
            if (instance == null) instance = new ICYou();
        }

        public static void Reset()
        {
            instance = null;
            TailOfStars.num00 = 0;
            TailOfStars.Eye = false;
        }

        public void CountingStar()
        {
            var EastPalace = 0;
            var WestPalace = 0;
            var Salt = 0;


            foreach (var id in Characters.GetAllCharIds())
                if (DateFile.instance.GetActorDate(id, 26, false) == "0")
                {
                    if (DateFile.instance.GetActorDate(id, 21, false) == Memo.Heresy) EastPalace++;
                    else WestPalace++;
                    if (DateFile.instance.GetActorDate(id, 21, false) == Memo.Homo)
                        Salt++;
                }

            double num = 99;
            if (WestPalace != 0)
            {
                num = Convert.ToDouble(EastPalace) / Convert.ToDouble(WestPalace);
                Main.Logger.Log(EastPalace.ToString());
                Main.Logger.Log(WestPalace.ToString());
                Main.Logger.Log(Salt.ToString());
                Main.Logger.Log(num.ToString());
                TailOfStars.num00 = num;
            }

            if (!Main.settings.SuperGay && Main.settings.sexoption == 0) TailOfStars.Eye = true;
        }
    }

    [HarmonyPatch(typeof(LoadGame), "LoadedReadonlyData")]
    public static class Loading_Update_Patch_GayMax
    {
        private static void Prefix()
        {
            if (!Main.enabled)
            {
            }

            else
            {
                try
                {
                    ICYou.Initialize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }

    [HarmonyPatch(typeof(LoadGame), "StartLoadingReadonlyData")]
    public static class Loading_LoadingScene_Patch
    {
        private static void Prefix()
        {
            if (!Main.enabled)
            {
            }
            else
            {
                ICYou.Reset();
            }
        }
    }

    //79我恨你……
    [HarmonyPatch(typeof(LoadGame), "LoadReadonlyData")]
    public class LoadGame_LoadReadonlyData_Patch
    {
        public static void Postfix()
        {
            Index.EventIndex = SevenNineLove.LoadEventDate(Main.txtpath, "GayMax_Event_Date.txt");
            Index.GongFaPowerIndex = SevenNineLove.LoadGongFaPower(Main.txtpath, "GayMax_GongFaPower_Date.txt",
                "GayMax_GongFaAntiPower_Date.txt", true);
            Index.GongFaIndex = SevenNineLove.LoadGongFa(Main.txtpath, "GayMax_GongFa_Date.txt", Index.GongFaPowerIndex,
                Main.settings.gongbase);
            /*Index.TurnEvenIndex = SevenNineLove.LoadOtherDate(Main.txtpath, "GayMax_TrunEvent_Date.txt",
                ref DateFile.instance.trunEventDate);*/
        }
    }

    [HarmonyPatch(typeof(EnterGame), "EnterWorld")]
    public static class EnterGame_EnterWorld_Patch_GayMax
    {
        private static void Postfix() //添加过月事件及图片
        {
            if (!SevenNineLove.IsRegistered()) SevenNineLove.Register();
        }
    }
}