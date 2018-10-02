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
        public uint taiwuPreemptRate = 5;

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
            settings.taiwuBugfix     = GUILayout.Toggle(settings.taiwuBugfix,     "修复太吾永远不会给继任者托梦的BUG");
            settings.lockGenderTaiwu = GUILayout.Toggle(settings.lockGenderTaiwu, "太吾转世性别固定。默认同性。");
            settings.lockGenderAll   = GUILayout.Toggle(settings.lockGenderAll,   "全人物转世性别固定。默认同性。");
            settings.lockGenderAlter = GUILayout.Toggle(settings.lockGenderAlter, "转世异性。开启了转世性别锁定的人物会转世为异性。");
            settings.lockFaceTaiwu   = GUILayout.Toggle(settings.lockFaceTaiwu,   "固定太吾相貌，前世为太吾的人物转世后相貌和前世相同(魅力根据相貌随机)。[需开启太吾/全人物转世性别锁定]");
            settings.lockFaceAll     = GUILayout.Toggle(settings.lockFaceAll,     "固定全人物相貌，所有人物转世后相貌和前世相同(魅力根据相貌随机)。[需开启全人物转世性别锁定]");

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
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }

    struct CharDataIndex
    {
        public const int OPINION = 3,
                         MOOD = 4,

                         SEX = 14,
                         CHARM = 15,

                         FACE_COMPONENTS = 995,
                         FACE_COLORS = 996;
    }

    public class DeadTaiwuManager
    {
        public const int DEAD_TAIWU_MOOD = 0x1124;  // 用来识别死亡太吾的心情魔数, 心情正常值范围是0 - 100所以魔数不能在这个范围内

        public List<CharId> deadTaiwuList;

        public static DeadTaiwuManager instance { get; private set; }

        public DeadTaiwuManager()
        {
            scanGraveForDeadTaiwu();
        }

        public void scanGraveForDeadTaiwu()
        {
            // 检查坟场里有没有死太吾，每次读档后需要做一次
            deadTaiwuList = new List<CharId>();
            foreach (var charId in DateFile.instance.deadActors)
            {
                if (isDeadTaiwu(charId)) { deadTaiwuList.Add(charId);  }
            }

            Console.WriteLine(string.Format("Info: full scan on grave done, found {0} dead taiwus", deadTaiwuList.Count));
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
            return DateFile.instance.GetActorDate(charId, CharDataIndex.MOOD, false) == DEAD_TAIWU_MOOD.ToString();
        }

        // 把charId对应人物标记为死太吾，并且放进Cache
        public void setAsDeadTaiwu(CharId charId)
        {
            DateFile.instance.actorsDate[charId][CharDataIndex.MOOD] = DEAD_TAIWU_MOOD.ToString();
            deadTaiwuList.Add(charId);
        }

        public void clearDeadTaiwu(CharId charId)
        {
            deadTaiwuList.Remove(charId);

            // 因为初始化的时候只扫坟里的，投胎也只投坟里的，所以就算不清除标记也不会导致重复投胎
            // DateFile.instance.actorsDate[charId][CharDataIndex.MOOD] = "42";
        }

        // 判定是否要进行强制太吾降生，如果要进行的话，从坟场里拿出一个太吾ID进行强制降生
        // return: whether it is a taiwu getting delivered after the function applied any possible modification.
        public bool tryDeadTaiwuBirthPreemption(int childId, ref List<CharId> previousLives)
        {
            // 判定太吾是否要抢这次降生机会
            if (Main.settings.taiwuPreemptRate <= 0
                || deadTaiwuList.Count == 0
                || UnityEngine.Random.Range(0, 100) >= Main.settings.taiwuPreemptRate)
            {
                return false;
            }

            if (previousLives == null || previousLives.Count == 0)
            {
                if (deadTaiwuList.Count != 0)
                {
                    Console.WriteLine("Error: Martian dead taiwu in registry. Registry cleared.");
                    deadTaiwuList.Clear();
                }
                return false;
            }

            // 检查正在降生的是否已经是死太吾
            CharId preemptedCharId = previousLives[previousLives.Count - 1];
            if (deadTaiwuList.Contains(preemptedCharId))
            {
                clearDeadTaiwu(preemptedCharId);
                if (!isDeadTaiwu(preemptedCharId))
                {
                    Console.WriteLine("Error: fake taiwu in registry");
                    return false;
                }
                return true;
            }

            Console.WriteLine("Start preempting birth for taiwu.");

            var livesBackup = new List<CharId>(previousLives);
            try
            {
                DateFile data = DateFile.instance;

                // 选取一只太吾
                CharId taiwuCharId = deadTaiwuList[0];
                clearDeadTaiwu(taiwuCharId);

                // Due diligence
                if (!isDeadTaiwu(taiwuCharId))
                {
                    Console.WriteLine("Error: fake taiwu in registry");
                    return false;
                }

                // 从坟里挖出来
                if (!data.deadActors.Remove(taiwuCharId))
                {
                    Console.WriteLine("Error: trying to reincarnation character not in grave.");
                    return false;
                }

                // 塞进轮回
                previousLives.Clear();
                previousLives.AddRange(new List<int>(data.GetLifeDateList(taiwuCharId, 801, false)) { taiwuCharId });

                // 把被抢占的人塞回坟墓
                data.deadActors.Add(preemptedCharId);

                // 触发托梦事件
                triggerDreamEvent(childId, taiwuCharId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Error: Failed birth preempting attempt.");

                // 恢复原本轮回数据
                previousLives = livesBackup;

                return false;
            }

            return true;
        }

        public static bool triggerDreamEvent(CharId childId, CharId taiwuCharId)
        {
            if (DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), taiwuCharId, false, false) >= 30000)
            {
                int gangGroup = int.Parse(DateFile.instance.GetActorDate(childId, 19, false));
                UIDate.instance.changTrunEvents.Add(new int[]
                {
                    239,
                    taiwuCharId,
                    int.Parse(DateFile.instance.GetGangDate(gangGroup, 3))
                });

                return true;
            }

            return false;
        }
    }

    /// <summary>
    ///  开新游戏，读取进度时，重置死太吾管理器
    /// </summary>
    [HarmonyPatch(typeof(Loading), "LoadingScene")]
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
    [HarmonyPatch(typeof(Loading), "Update")]
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

    /// <summary>
    ///  绕过太吾身份传承时前后两代太吾之间的好感数据丢失，导致转生后没有提示的问题
    /// </summary>
    [HarmonyPatch(typeof(ActorMenu), "NewMianActor")]
    public static class ActorMenu_NewMianActor_Patch
    {
        public struct Succession
        {
            public CharId predecessor;
            public OpinionValue opinion;
        }

        public static Dictionary<CharId, Succession> taiwuOpinionsOfPredecessor = new Dictionary<CharId, Succession>();

        private static void Prefix(ActorMenu __instance)
        {
            if (!Main.enabled)
            {
                return;
            }

            try
            {
                CharId oldTaiwuId = DateFile.instance.MianActorID();
                CharId newTaiwuId = (int)typeof(ActorMenu).GetField("chooseNewActor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                // 保存前后两代太吾的关系值
                var succession = new Succession();
                succession.predecessor = oldTaiwuId;
                succession.opinion = int.Parse(DateFile.instance.GetActorDate(newTaiwuId, 3, false));
                //succession.opinion = DateFile.instance.GetActorFavor(false, oldTaiwuId, newTaiwuId, false, false);
                taiwuOpinionsOfPredecessor.Add(newTaiwuId, succession);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void Postfix(ActorMenu __instance)
        {
            if (!Main.enabled)
            {
                return;
            }

            try
            {
                CharId newTaiwuId = DateFile.instance.MianActorID();
                Succession succession;
                if (taiwuOpinionsOfPredecessor.TryGetValue(newTaiwuId, out succession))
                {
                    var predecessor = DateFile.instance.actorsDate[succession.predecessor];

                    // 恢复前后两代太吾的关系值
                    predecessor[CharDataIndex.OPINION] = succession.opinion.ToString();

                    // 标记前代为死亡太吾
                    if (Main.settings.lockFaceTaiwu)
                    {
                        DeadTaiwuManager.instance.setAsDeadTaiwu(succession.predecessor);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    /// <summary>
    ///  建立人物时拦截并修改结果
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MakeNewChildren")]
    public static class DateFile_MakeNewChildren_Patch
    {
        private static void Postfix(DateFile __instance, ref List<int> __result)
        {
            if (!Main.enabled)
            {
                return;
            }

            try
            {
                foreach (CharId childId in __result)
                {
                    // previousLives存了所有前世身份，最后一条是上一世
                    var previousLives = __instance.actorLife[childId][801];
                    if (previousLives == null || previousLives.Count == 0)
                    {
                        return;
                    }

                    // 尝试进行太吾抢占降生
                    bool isTaiwu = DeadTaiwuManager.instance.tryDeadTaiwuBirthPreemption(childId, ref previousLives);

                    if (Main.settings.lockGenderAll || (Main.settings.lockGenderTaiwu && isTaiwu))
                    {
                        int previousLifeId = previousLives[previousLives.Count - 1];
                        int previousLifeGender = int.Parse(__instance.GetActorDate(previousLifeId, CharDataIndex.SEX, false));

                        // 性别：１- 男， 2 - 女， 1+2=3，性别翻转用 3-x
                        int gender = Main.settings.lockGenderAlter ? 3 - previousLifeGender : previousLifeGender;
                        __instance.actorsDate[childId][CharDataIndex.SEX] = gender.ToString();

                        // 恢复前世脸以及肤色
                        if (Main.settings.lockFaceAll || (Main.settings.lockFaceTaiwu && isTaiwu))
                        {
                            string face_components = __instance.GetActorDate(previousLifeId, CharDataIndex.FACE_COMPONENTS, false);
                            string face_colors = __instance.GetActorDate(previousLifeId, CharDataIndex.FACE_COLORS, false);

                            int charm = __instance.GetFaceCharm(gender, Array.ConvertAll(face_components.Split(new char[] { '|' }), int.Parse));

                            __instance.actorsDate[childId][CharDataIndex.FACE_COMPONENTS] = face_components;
                            __instance.actorsDate[childId][CharDataIndex.FACE_COLORS] = face_colors;
                            __instance.actorsDate[childId][CharDataIndex.CHARM] = charm.ToString();
                            __instance.MakeActorName(childId, int.Parse(__instance.GetActorDate(childId, 29, false)), __instance.GetActorDate(childId, 5, false), true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }

}