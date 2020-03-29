using GameData;
using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DreamLover
{
    //[HarmonyPatch(typeof(MessageEventManager), "EndEvent9001_1")]
    public static class EndEvent_Rape_Patch
    {
        public static PatchModuleInfo patchModuleInfo = new PatchModuleInfo(
            typeof(MessageEventManager), "EndEvent9001_1",
            typeof(EndEvent_Rape_Patch));
        public static bool Prefix()
        {
            if (!Main.enabled) return true;

            int mainActorId = DateFile.instance.MianActorID();
            int targetId = MessageEventManager.Instance.MainEventData[1];
            int mapId = 0, tileId = 0;

            List<int> actorAtPlace = DateFile.instance.GetActorAtPlace(targetId);
            if (actorAtPlace != null)
            {
                mapId = actorAtPlace[0];
                tileId = actorAtPlace[1];
            }

            switch (MessageEventManager.Instance.EventValue[1])
            {
                case 11:
                    {
                        List<int> Values = MessageEventManager.Instance.EventValue;
                        bool 跳过战力检定 = Main.settings.rape.skipBattle;
                        bool 影响双方情绪 = Main.settings.rape.moodChange;
                        bool 结仇 = Main.settings.rape.beHated;
                        bool 单亲 = Main.settings.rape.oneParent;
                        bool 无记录 = Main.settings.rape.noLog;

                        if(!Main.settings.rape.overwriteArg)
                        {
                            if (Values.Count > 2) 跳过战力检定 = (Values[2] != 0);
                            if (Values.Count > 3) 影响双方情绪 = (Values[3] != 0);
                            if (Values.Count > 4) 结仇 = (Values[4] != 0);
                            if (Values.Count > 5) 单亲 = (Values[5] != 0);
                            if (Values.Count > 6) 无记录 = (Values[6] != 0);
                        }

                        RapeHelper.Rape(mainActorId, targetId, mapId, tileId, 跳过战力检定, 影响双方情绪, 结仇, 单亲, 无记录);
                        break;
                    }
                default:
                    return true;
            }

            WorldMapSystem.instance.UpdatePlaceActor(WorldMapSystem.instance.choosePartId, WorldMapSystem.instance.choosePlaceId);
            return false;
        }
    }
}
