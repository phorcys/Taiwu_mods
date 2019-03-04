using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace GuiScroll
{

    public static class ActorMenuActorListPatch
    {
        public static void Init(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }


        [HarmonyPatch(typeof(ActorMenu), "ShowActorMenu")]
        public static class ActorMenu_ShowActorMenu_Patch
        {
            public static bool Prefix(bool enemy)
            {
                Main.Logger.Log("显示角色菜单 ShowActorMenu " + enemy);
                return true;
            }
        }



        [HarmonyPatch(typeof(ActorMenu), "SetActorList")]
        public static class ActorMenu_SetActorList_Patch
        {
            public static bool Prefix()
            {
                Main.Logger.Log("显示角色 SetActorList");
                ActorMenu _this = ActorMenu.instance;
                var f_showActors = typeof(ActorMenu).GetField("showActors", BindingFlags.NonPublic | BindingFlags.Instance);
                var showActors = (List<int>)f_showActors.GetValue(ActorMenu.instance);

                for (int i = 0; i < showActors.Count; i++)
                {
                    Main.Logger.Log(i + " " + DateFile.instance.GetActorName(showActors[i]));
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(ActorMenu), "SortActorList")]
        public static class ActorMenu_SortActorList_Patch
        {
            public static bool Prefix()
            {
                Main.Logger.Log("显示角色列表 SortActorList");
                return true;
            }
        }


        [HarmonyPatch(typeof(ActorMenu), "UpdateActorListFavor")]
        public static class ActorMenu_UpdateActorListFavort_Patch
        {
            public static bool Prefix()
            {
                Main.Logger.Log("更新角色列表喜爱 UpdateActorListFavor");
                return true;
            }
        }


        [HarmonyPatch(typeof(ActorMenu), "UpdateActorListFace")]
        public static class ActorMenu_UpdateActorListFace_Patch
        {
            public static bool Prefix()
            {
                Main.Logger.Log("显示角色列表面容 UpdateActorListFace");
                return true;
            }
        }


        [HarmonyPatch(typeof(ActorMenu), "RemoveActorList")]
        public static class ActorMenu_RemoveActorList_Patch
        {
            public static bool Prefix()
            {
                Main.Logger.Log("删除角色列表 RemoveActorList");
                return true;
            }
        }
    }
}