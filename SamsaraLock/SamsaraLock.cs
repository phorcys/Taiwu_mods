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

    struct MagicNum
    {
        public const int DEAD_TAIWU_MOOD = 0x1124;  // 用来识别死亡太吾的心情魔数, 心情正常值范围是0 - 100所以魔数不能在这个范围内
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
                succession.opinion = DateFile.instance.GetActorFavor(false, oldTaiwuId, newTaiwuId, false, false);
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
                        predecessor[CharDataIndex.MOOD] = MagicNum.DEAD_TAIWU_MOOD.ToString();
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
        private static bool isTaiwu(DateFile __instance, int charId)
        {
            return int.Parse(__instance.GetActorDate(charId, CharDataIndex.MOOD, false)) == MagicNum.DEAD_TAIWU_MOOD;
        }

        private static void Postfix(DateFile __instance, ref List<int> __result)
        {
            if (!Main.enabled)
            {
                return;
            }

            try
            {
                int childId = __result[0];
                if (__instance.actorLife.ContainsKey(childId))
                {
                    var previousLives = __instance.actorLife[childId][801];
                    if (previousLives.Count > 0)
                    {
                        int previousLifeId = previousLives[previousLives.Count - 1];
                        if (Main.settings.lockGenderAll || (Main.settings.lockGenderTaiwu && isTaiwu(__instance, previousLifeId)))
                        {
                            int previousLifeGender = int.Parse(__instance.GetActorDate(previousLifeId, CharDataIndex.SEX, false));

                            // 性别：１- 男， 2 - 女， 1+2=3，性别翻转用 3-x
                            int gender = Main.settings.lockGenderAlter ? 3 - previousLifeGender : previousLifeGender;
                            __instance.actorsDate[childId][CharDataIndex.SEX] = gender.ToString();

                            // 恢复前世脸以及肤色
                            if (Main.settings.lockFaceAll || (Main.settings.lockFaceTaiwu && isTaiwu(__instance, previousLifeId)))
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }

}