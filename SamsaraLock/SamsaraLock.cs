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

namespace SamsaraLock
{
    using CharId = System.Int32;
    using OpinionValue = System.Int32;


    public class Settings : UnityModManager.ModSettings
    {
        public bool taiwuBugfix = true;
        public bool lockGenderTaiwu = true;
        public bool lockGenderAll = false;
        public bool lockGenderAlter = false;
        public bool lockFaceTaiwu = true;
        public bool lockFaceAll = false;
        public bool lockFaceFuta = true;

        public uint taiwuPreemptRate = 100;

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
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Settings.Load<Settings>(modEntry);

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

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.taiwuBugfix = GUILayout.Toggle(settings.taiwuBugfix, "修复太吾永远不会给继任者托梦的BUG");
            settings.lockGenderTaiwu = GUILayout.Toggle(settings.lockGenderTaiwu, "太吾转世性别固定。默认同性。");
            settings.lockGenderAll = GUILayout.Toggle(settings.lockGenderAll, "全人物转世性别固定。默认同性。");
            settings.lockGenderAlter = GUILayout.Toggle(settings.lockGenderAlter, "转世异性。开启了转世性别锁定的人物会转世为异性。");
            settings.lockFaceTaiwu = GUILayout.Toggle(settings.lockFaceTaiwu, "固定太吾相貌，前世为太吾的人物转世后相貌和前世相同(魅力根据相貌随机)。[需开启太吾/全人物转世性别锁定]");
            settings.lockFaceAll = GUILayout.Toggle(settings.lockFaceAll, "固定全人物相貌，所有人物转世后相貌和前世相同(魅力根据相貌随机)。[需开启全人物转世性别锁定]");

            GUILayout.BeginHorizontal();

            GUILayout.Label("[插队投胎] 每个婴儿降生时阴间的太吾都有", GUILayout.ExpandWidth(false));

            var taiwuPreemptRate = GUILayout.TextField(settings.taiwuPreemptRate.ToString(), 3, GUILayout.Width(40));
            if (GUI.changed)
            {
                if (!uint.TryParse(taiwuPreemptRate, out settings.taiwuPreemptRate)) { settings.taiwuPreemptRate = 0; }
                if (settings.taiwuPreemptRate > 100) { settings.taiwuPreemptRate = 100; }
            }

            GUILayout.Label("%的机会抢占转生机会。设置为0时死去的太吾投胎机会和正常人相同。", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();

            //加入手动寻找太吾按钮
            bool flag = DateFile.instance == null || !GameData.Characters.HasChar(DateFile.instance.MianActorID());
            if (flag)
            {
                GUILayout.Label("存档未载入!", new GUILayoutOption[0]);
            }
            else
            {
                bool findlove = GUILayout.Button("手动识别未转生太吾", new GUILayoutOption[]
                {
                    GUILayout.Width(200f)
                });
                if (findlove)
                {
                   
                    DeadTaiwuManager.reset();
                    try
                    {
                        // 初始化死太吾cache
                        DeadTaiwuManager.initialize();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                }

            }


            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }

    struct CharDataIndex
    {
        public const int OPINION = 3,
                         LOVE = 21,

                         SEX = 14,
                         CHARM = 15,

                         FUTA = 17,

                         FACE_COMPONENTS = 995,
                         FACE_COLORS = 996;
    }

    public class DeadTaiwuManager
    {
        public const int DEAD_TAIWU_LOVE = 1;  // 用来识别死亡太吾的恋爱之魔数, 这到底代表什么呢？

        public List<CharId> deadTaiwuList;

        public static DeadTaiwuManager instance { get; private set; }

        public DeadTaiwuManager()
        {
            DIGTaiwu();
            scanGraveForDeadTaiwu();
        }

        //从身份上识别并标记坟场中的太污,不刷好感也OK啦
        public void DIGTaiwu()
        {

            foreach (int id in DateFile.instance.deadActors)
            {
                int num2 = int.Parse(DateFile.instance.GetActorDate(id, 19, false));
                int num3 = int.Parse(DateFile.instance.GetActorDate(id, 20, false));
                int gangValueId = DateFile.instance.GetGangValueId(num2, num3);
                if (gangValueId == 99)
                {
                    GameData.Characters.SetCharProperty(id, CharDataIndex.LOVE, DEAD_TAIWU_LOVE.ToString());
                }

            }

        }


        public void scanGraveForDeadTaiwu()
        {
            // 检查坟场里有没有死太吾，每次读档后需要做一次
            deadTaiwuList = new List<CharId>();
            foreach (var charId in DateFile.instance.deadActors)
            {
                if (isDeadTaiwu(charId))
                {
                    deadTaiwuList.Add(charId);
                    var text = DateFile.instance.GetActorName(charId, true, false);
                    Main.Logger.Log(charId.ToString() + "[" + text + "]" + "\u00A0\u00A0");
                }
            }

            Console.WriteLine(string.Format("Info: full scan on grave done, found {0} dead taiwus", deadTaiwuList.Count));
            Main.Logger.Log("Info: full scan on grave done, found " + deadTaiwuList.Count + "dead taiwus");
        }

        public static void initialize()
        {
            if (instance == null)
            {
                Console.WriteLine("Info: initialize DeadTaiwuManager.");
                instance = new DeadTaiwuManager();

            }
        }

        public static void reset()
        {
            Console.WriteLine("Info: reset DeadTaiwuManager.");
            instance = null;
        }

        public static bool isDeadTaiwu(CharId charId)
        {
            return DateFile.instance.GetActorDate(charId, CharDataIndex.LOVE, false) == DEAD_TAIWU_LOVE.ToString();
        }

        // 把charId对应人物标记为死太吾，并且放进Cache
        public void setAsDeadTaiwu(CharId charId)
        {
            GameData.Characters.SetCharProperty(charId, CharDataIndex.LOVE, DEAD_TAIWU_LOVE.ToString());
            if(!deadTaiwuList.Contains(charId))deadTaiwuList.Add(charId);
        }

        public void clearDeadTaiwu(CharId charId)
        {
            deadTaiwuList.Remove(charId);

            // 因为初始化的时候只扫坟里的，投胎也只投坟里的，所以就算不清除标记也不会导致重复投胎
            // DateFile.instance.actorsDate[charId][CharDataIndex.LOVE] = "0";
        }

    }

    /// <summary>
    ///  开新游戏，读取进度时，重置死太吾管理器
    /// </summary>
    [HarmonyPatch(typeof(ui_Loading), "LoadingScene")]
    public static class Loading_LoadingScene_Patch
    {

        private static void Prefix(bool newGame, int teachingId, int loadingDateId)
        {
            if (!Main.enabled) { return; }

            if (newGame || loadingDateId != 0)
            {
                DeadTaiwuManager.reset();
            }
        }
    }

    /// <summary>
    ///  Loading完成，数据准备完毕后初始化死太吾管理器。
    ///  DeadTaiwuManager.initialize() 内部会处理重复initialize的情况。
    /// </summary>
    [HarmonyPatch(typeof(ui_Loading), "Update")]
    public static class Loading_Update_Patch
    {
        private static void Prefix(bool ___loadingEnd)
        {
            if (!Main.enabled) { return; }

            if (___loadingEnd)
            {
                try
                {
                    // 初始化死太吾cache
                    DeadTaiwuManager.initialize();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }

    //在传剑时把太污送入名单，因为之前已经remove过了，标记应该不会失效（虽然不标记好像也OK

    [HarmonyPatch(typeof(DateFile), "ActorInherit")]
    public static class TaiwuGetdaze
    {
        private static void Prefix(ref int actorId, ref bool die)
        {
            if (!Main.enabled) { return; }
            int taiwu = DateFile.instance.MianActorID();
            if (actorId == taiwu && die)
            {
                DeadTaiwuManager.initialize();
                DeadTaiwuManager.instance.setAsDeadTaiwu(taiwu);
            }
           

        }
    }


    /// <summary>
    ///  直接和谐掉设置轮回的函数，MOD的写法变容易&去除因为门派新人造成的失效，但会导致MOD生效级降低(针对性别锁而言)
    /// 也更容易躺枪（
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "SetActorSamsara")]
    public static class DateFile_SetActorSamsara_Patch
    {
        private static bool Prefix( ref int actorId,ref int toPartId)
        {
            if (!Main.enabled) return true;
            bool istaiwu = false;
            int preactor = 0;
            //判断是否太吾转生
            if (DeadTaiwuManager.instance == null || DeadTaiwuManager.instance.deadTaiwuList.Count < 1 || UnityEngine.Random.Range(0, 100) > Main.settings.taiwuPreemptRate)
            {
                //随机抓取一个死人
                if (DateFile.instance.deadActors.Count < 1)
                    return false;
                else
                {
                    preactor = DateFile.instance.deadActors[UnityEngine.Random.Range(0, DateFile.instance.deadActors.Count)];
                    if (DeadTaiwuManager.instance != null && DeadTaiwuManager.instance.deadTaiwuList.Contains(preactor))
                        istaiwu = true;
                }
               


            }
            else
            {
                //抓走一只试图插队的太吾
                istaiwu = true;
                preactor = DeadTaiwuManager.instance.deadTaiwuList[UnityEngine.Random.Range(0, DeadTaiwuManager.instance.deadTaiwuList.Count)];
              
            }
            //生成轮回信息
            List<int> value = new List<int>(DateFile.instance.GetLifeDateList(preactor, 801, false)){preactor};
            //写入轮回信息
            DateFile.instance.actorLife[actorId].Add(801, value);
            //从奈何桥头移走死人
            DateFile.instance.deadActors.Remove(preactor);
            if (istaiwu) DeadTaiwuManager.instance.deadTaiwuList.Remove(preactor);
            //生成过季提示
            if (DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), preactor, false, false) >= 30000 || (istaiwu&&Main.settings.taiwuBugfix))
            {
                UIDate.instance.changTrunEvents.Add(new int[]
                {
                    239,
                    preactor,
                    toPartId
                });
            }
            if (Main.settings.lockGenderAll || (Main.settings.lockGenderTaiwu && istaiwu))
            {


                int previousLifeGender = int.Parse(DateFile.instance.GetActorDate(preactor, CharDataIndex.SEX, false));

                // 性别：１- 男， 2 - 女， 1+2=3，性别翻转用 3-x
                int gender = Main.settings.lockGenderAlter ? 3 - previousLifeGender : previousLifeGender;

                // 恢复前世脸以及肤色
                if (Main.settings.lockFaceAll || (Main.settings.lockFaceTaiwu && istaiwu))
                {
                   
                    string face_components = GameData.Characters.GetCharProperty(preactor, CharDataIndex.FACE_COMPONENTS);
                    string face_colors = GameData.Characters.GetCharProperty(preactor, CharDataIndex.FACE_COLORS);
                    string futa = DateFile.instance.GetActorDate(preactor, CharDataIndex.FUTA, false);
                  
                    int charm = DateFile. instance.GetFaceCharm(gender, Array.ConvertAll(face_components.Split(new char[] { '|' }), int.Parse));

                    GameData.Characters.SetCharProperty(actorId, CharDataIndex.FACE_COMPONENTS, face_components);
                    GameData.Characters.SetCharProperty(actorId, CharDataIndex.FACE_COLORS, face_colors);
                    GameData.Characters.SetCharProperty(actorId, CharDataIndex.CHARM, charm.ToString());
                    GameData.Characters.SetCharProperty(actorId, CharDataIndex.FUTA, futa);
                   

                }

                int baseactor = int.Parse(GameData.Characters.GetCharProperty(actorId, 997));
                int gender2 = int.Parse(DateFile.instance.presetActorDate[baseactor][14]);
                int brithpart = (baseactor - 1) / 2;

                if (gender2 != gender)
                {

                    //改写BUG之源997
                    GameData.Characters.SetCharProperty(actorId, 997, (brithpart * 2 + gender).ToString());

                    //重命名
                    DateFile.instance.MakeActorName(actorId, int.Parse(DateFile.instance.GetActorDate(actorId, 29, false)), DateFile.instance.GetActorDate(actorId, 5, false), true);

                }

            }
            return false;
        }

    }

}

