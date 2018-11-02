using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace GuiWarehouse
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public bool open = true; //使用鬼的仓库
    }
    public static class Main
    {
        public static bool onOpen = false;//
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            #region 基础设置
            settings = Settings.Load<Settings>(modEntry);
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            return true;
        }

        static string title = "鬼的工作间";
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(title, GUILayout.Width(300));
            Main.settings.open = GUILayout.Toggle(Main.settings.open, "使用鬼的工作间");
            GUILayout.EndHorizontal();
        }






































        [HarmonyPatch(typeof(HomeSystem), "ShowHomeSystem")]
        static class HomeSystem_ShowHomeSystem_Pached
        {
            public static  void ShowHomeSystem(bool baseHome)
            {
                Main.Logger.Log("public void ShowHomeSystem(bool baseHome)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "CloseHomeSystem")]
        static class HomeSystem_CloseHomeSystem_Pached
        {
            public static  void CloseHomeSystem()
            {
                Main.Logger.Log("public void CloseHomeSystem()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "OpenBuildingWindow")]
        static class HomeSystem_OpenBuildingWindow_Pached
        {
            private static  void OpenBuildingWindow()
            {
                Main.Logger.Log("private void OpenBuildingWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "CloseBuildingWindow")]
        static class HomeSystem_CloseBuildingWindow_Pached
        {
            public static  void CloseBuildingWindow()
            {
                Main.Logger.Log("public void CloseBuildingWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "ShowBuildingWindow")]
        static class HomeSystem_ShowBuildingWindow_Pached
        {
            public static  void ShowBuildingWindow(int partId, int placeId, int buildingIndex)
            {
                Main.Logger.Log("public void ShowBuildingWindow(int partId, int placeId, int buildingIndex)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RestMapView")]
        static class HomeSystem_RestMapView_Pached
        {
            public static  void RestMapView()
            {
                Main.Logger.Log("public void RestMapView()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetHomeList")]
        static class HomeSystem_SetHomeList_Pached
        {
            private static  void SetHomeList()
            {
                Main.Logger.Log("private void SetHomeList()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetMapChooseList")]
        static class HomeSystem_SetMapChooseList_Pached
        {
            private static  void SetMapChooseList()
            {
                Main.Logger.Log("private void SetMapChooseList()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveHomeList")]
        static class HomeSystem_RemoveHomeList_Pached
        {
            private static  void RemoveHomeList()
            {
                Main.Logger.Log("private void RemoveHomeList()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateHomeSize")]
        static class HomeSystem_UpdateHomeSize_Pached
        {
            public static  void UpdateHomeSize(int partId, int PlaceId)
            {
                Main.Logger.Log("public void UpdateHomeSize(int partId, int PlaceId)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "MakeHomeMap")]
        static class HomeSystem_MakeHomeMap_Pached
        {
            public static  void MakeHomeMap(int partId, int placeId)
            {
                Main.Logger.Log("public void MakeHomeMap(int partId, int placeId)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateHomePlace")]
        static class HomeSystem_UpdateHomePlace_Pached
        {
            public static  void UpdateHomePlace(int partId, int placeId, int buildingIndex)
            {
                Main.Logger.Log("public void UpdateHomePlace(int partId, int placeId, int buildingIndex)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateHomeAllPlace")]
        static class HomeSystem_UpdateHomeAllPlace_Pached
        {
            public static  void UpdateHomeAllPlace(int partId, int placeId)
            {
                Main.Logger.Log("public void UpdateHomeAllPlace(int partId, int placeId)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdatePlaceSize")]
        static class HomeSystem_UpdatePlaceSize_Pached
        {
            public static  void UpdatePlaceSize(int partId, int placeId)
            {
                Main.Logger.Log("public void UpdatePlaceSize(int partId, int placeId)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "GetBuildingMassage")]
        static class HomeSystem_GetBuildingMassage_Pached
        {
            public static  void GetBuildingMassage()
            {
                Main.Logger.Log("public void GetBuildingMassage()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "ShowWorkingActor")]
        static class HomeSystem_ShowWorkingActor_Pached
        {
            public static  void ShowWorkingActor(bool show)
            {
                Main.Logger.Log("public void ShowWorkingActor(bool show)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateButtonText")]
        static class HomeSystem_UpdateButtonText_Pached
        {
            public static  void UpdateButtonText()
            {
                Main.Logger.Log("public void UpdateButtonText()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "OpenStudyWindow")]
        static class HomeSystem_OpenStudyWindow_Pached
        {
            public static  void OpenStudyWindow()
            {
                Main.Logger.Log("public void OpenStudyWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "CloseStudyWindow")]
        static class HomeSystem_CloseStudyWindow_Pached
        {
            public static  void CloseStudyWindow()
            {
                Main.Logger.Log("public void CloseStudyWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateShopMassage")]
        static class HomeSystem_UpdateShopMassage_Pached
        {
            public static  void UpdateShopMassage()
            {
                Main.Logger.Log("public void UpdateShopMassage()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RestMassageSize")]
        static class HomeSystem_RestMassageSize_Pached
        {
            public static  void RestMassageSize()
            {
                Main.Logger.Log("public void RestMassageSize()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveShopBooty")]
        static class HomeSystem_RemoveShopBooty_Pached
        {
            private static  void RemoveShopBooty()
            {
                Main.Logger.Log("private void RemoveShopBooty()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "GetShopBooty")]
        static class HomeSystem_GetShopBooty_Pached
        {
            public static  void GetShopBooty(int bootyIndex)
            {
                Main.Logger.Log("public void GetShopBooty(int bootyIndex)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateShop")]
        static class HomeSystem_UpdateShop_Pached
        {
            public static  void UpdateShop(int partId, int placeId, int buildingIndex)
            {
                Main.Logger.Log("public void UpdateShop(int partId, int placeId, int buildingIndex)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "ChangeHomeShopLevel")]
        static class HomeSystem_ChangeHomeShopLevel_Pached
        {
            private static  void ChangeHomeShopLevel(int partId, int placeId, int buildingIndex, int value)
            {
                Main.Logger.Log("private void ChangeHomeShopLevel(int partId, int placeId, int buildingIndex, int value)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "AddHomeShopEvent")]
        static class HomeSystem_AddHomeShopEvent_Pached
        {
            private static  void AddHomeShopEvent(int partId, int placeId, int buildingIndex, int eventPartId, int eventPlaceId, int eventId, int placeMark)
            {
                Main.Logger.Log("private void AddHomeShopEvent(int partId, int placeId, int buildingIndex, int eventPartId, int eventPlaceId, int eventId, int placeMark)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "AddHomeShopMassage")]
        static class HomeSystem_AddHomeShopMassage_Pached
        {
            public static  void AddHomeShopMassage(int partId, int placeId, int buildingIndex, string[] massageDate)
            {
                Main.Logger.Log("public void AddHomeShopMassage(int partId, int placeId, int buildingIndex, string[] massageDate)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateNewBuildingWindow")]
        static class HomeSystem_UpdateNewBuildingWindow_Pached
        {
            public static  void UpdateNewBuildingWindow(int id)
            {
                Main.Logger.Log("public void UpdateNewBuildingWindow(int id)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UesMenpowerUp1")]
        static class HomeSystem_UesMenpowerUp1_Pached
        {
            public static  void UesMenpowerUp1()
            {
                Main.Logger.Log("public void UesMenpowerUp1()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UesMenpowerDown1")]
        static class HomeSystem_UesMenpowerDown1_Pached
        {
            public static  void UesMenpowerDown1()
            {
                Main.Logger.Log("public void UesMenpowerDown1()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateNeedTime1")]
        static class HomeSystem_UpdateNeedTime1_Pached
        {
            private static  void UpdateNeedTime1()
            {
                Main.Logger.Log("private void UpdateNeedTime1()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "GetItem")]
        static class HomeSystem_GetItem_Pached
        {
            private static  void GetItem()
            {
                Main.Logger.Log("private void GetItem()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveItems")]
        static class HomeSystem_RemoveItems_Pached
        {
            private static  void RemoveItems()
            {
                Main.Logger.Log("private void RemoveItems()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StartNewBuilding")]
        static class HomeSystem_StartNewBuilding_Pached
        {
            public static  void StartNewBuilding()
            {
                Main.Logger.Log("public void StartNewBuilding()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "MakeNewBuilding")]
        static class HomeSystem_MakeNewBuilding_Pached
        {
            public static  void MakeNewBuilding()
            {
                Main.Logger.Log("public void MakeNewBuilding()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateBuildingUpWindow")]
        static class HomeSystem_UpdateBuildingUpWindow_Pached
        {
            public static  void UpdateBuildingUpWindow()
            {
                Main.Logger.Log("public void UpdateBuildingUpWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UesMenpowerUp2")]
        static class HomeSystem_UesMenpowerUp2_Pached
        {
            public static  void UesMenpowerUp2()
            {
                Main.Logger.Log("public void UesMenpowerUp2()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UesMenpowerDown2")]
        static class HomeSystem_UesMenpowerDown2_Pached
        {
            public static  void UesMenpowerDown2()
            {
                Main.Logger.Log("public void UesMenpowerDown2()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateNeedTime2")]
        static class HomeSystem_UpdateNeedTime2_Pached
        {
            private static  void UpdateNeedTime2()
            {
                Main.Logger.Log("private void UpdateNeedTime2()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StartBuildingUp")]
        static class HomeSystem_StartBuildingUp_Pached
        {
            public static  void StartBuildingUp()
            {
                Main.Logger.Log("public void StartBuildingUp()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "MakeBuildingUp")]
        static class HomeSystem_MakeBuildingUp_Pached
        {
            public static  void MakeBuildingUp()
            {
                Main.Logger.Log("public void MakeBuildingUp()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetRemoveTyp")]
        static class HomeSystem_SetRemoveTyp_Pached
        {
            public static  void SetRemoveTyp(int typ)
            {
                Main.Logger.Log("public void SetRemoveTyp(int typ)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateRemoveTyp")]
        static class HomeSystem_UpdateRemoveTyp_Pached
        {
            private static  void UpdateRemoveTyp()
            {
                Main.Logger.Log("private void UpdateRemoveTyp()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateBuildingRemoveWindow")]
        static class HomeSystem_UpdateBuildingRemoveWindow_Pached
        {
            public static  void UpdateBuildingRemoveWindow()
            {
                Main.Logger.Log("public void UpdateBuildingRemoveWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UesMenpowerUp3")]
        static class HomeSystem_UesMenpowerUp3_Pached
        {
            public static  void UesMenpowerUp3()
            {
                Main.Logger.Log("public void UesMenpowerUp3()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UesMenpowerDown3")]
        static class HomeSystem_UesMenpowerDown3_Pached
        {
            public static  void UesMenpowerDown3()
            {
                Main.Logger.Log("public void UesMenpowerDown3()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateNeedTime3")]
        static class HomeSystem_UpdateNeedTime3_Pached
        {
            private static  void UpdateNeedTime3()
            {
                Main.Logger.Log("private void UpdateNeedTime3()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StartBuildingRemove")]
        static class HomeSystem_StartBuildingRemove_Pached
        {
            public static  void StartBuildingRemove()
            {
                Main.Logger.Log("public void StartBuildingRemove()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "MakeBuildingRemove")]
        static class HomeSystem_MakeBuildingRemove_Pached
        {
            public static  void MakeBuildingRemove()
            {
                Main.Logger.Log("public void MakeBuildingRemove()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StopRemoveBuilding")]
        static class HomeSystem_StopRemoveBuilding_Pached
        {
            public static  void StopRemoveBuilding()
            {
                Main.Logger.Log("public void StopRemoveBuilding()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "DoStopRemoveBuilding")]
        static class HomeSystem_DoStopRemoveBuilding_Pached
        {
            public static  void DoStopRemoveBuilding()
            {
                Main.Logger.Log("public void DoStopRemoveBuilding()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateWorkingActor")]
        static class HomeSystem_UpdateWorkingActor_Pached
        {
            private static  void UpdateWorkingActor(int partId, int placeId, int buildingIndex)
            {
                Main.Logger.Log("private void UpdateWorkingActor(int partId, int placeId, int buildingIndex)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "ShowWorkingAcotrWindow")]
        static class HomeSystem_ShowWorkingAcotrWindow_Pached
        {
            public static  void ShowWorkingAcotrWindow()
            {
                Main.Logger.Log("public void ShowWorkingAcotrWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "CloseActorWindow")]
        static class HomeSystem_CloseActorWindow_Pached
        {
            public static  void CloseActorWindow()
            {
                Main.Logger.Log("public void CloseActorWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "GetActor")]
        static class HomeSystem_GetActor_Pached
        {
            private static  void GetActor(int _skillTyp, bool favorChange = false)
            {
                Main.Logger.Log("private void GetActor(int _skillTyp, bool favorChange = false)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveActor")]
        static class HomeSystem_RemoveActor_Pached
        {
            private static  void RemoveActor()
            {
                Main.Logger.Log("private void RemoveActor()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetWorkingActor")]
        static class HomeSystem_SetWorkingActor_Pached
        {
            public static  void SetWorkingActor(int key)
            {
                Main.Logger.Log("public void SetWorkingActor(int key)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "ChanageWorkingAcotr")]
        static class HomeSystem_ChanageWorkingAcotr_Pached
        {
            public static  void ChanageWorkingAcotr()
            {
                Main.Logger.Log("public void ChanageWorkingAcotr()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetStudyWindow")]
        static class HomeSystem_SetStudyWindow_Pached
        {
            private static  void SetStudyWindow()
            {
                Main.Logger.Log("private void SetStudyWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateStudyValue")]
        static class HomeSystem_UpdateStudyValue_Pached
        {
            private static  void UpdateStudyValue()
            {
                Main.Logger.Log("private void UpdateStudyValue()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateActorStudy")]
        static class HomeSystem_UpdateActorStudy_Pached
        {
            private static  void UpdateActorStudy(int key, GameObject go_listActor = null)
            {
                Main.Logger.Log("private void UpdateActorStudy(int key, GameObject go_listActor = null)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveStudyActor")]
        static class HomeSystem_RemoveStudyActor_Pached
        {
            private static  void RemoveStudyActor()
            {
                Main.Logger.Log("private void RemoveStudyActor()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetStudy")]
        static class HomeSystem_SetStudy_Pached
        {
            public static  void SetStudy(int key, int index)
            {
                Main.Logger.Log("public void SetStudy(int key, int index)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "CloseSetStudyWindow")]
        static class HomeSystem_CloseSetStudyWindow_Pached
        {
            public static  void CloseSetStudyWindow()
            {
                Main.Logger.Log("public void CloseSetStudyWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetSetStudyWindow")]
        static class HomeSystem_SetSetStudyWindow_Pached
        {
            public static  void SetSetStudyWindow()
            {
                Main.Logger.Log("public void SetSetStudyWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetGongFa")]
        static class HomeSystem_SetGongFa_Pached
        {
            public static  void SetGongFa(int typ)
            {
                Main.Logger.Log("public void SetGongFa(int typ)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveGongFa")]
        static class HomeSystem_RemoveGongFa_Pached
        {
            private static  void RemoveGongFa()
            {
                Main.Logger.Log("private void RemoveGongFa()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetSkill")]
        static class HomeSystem_SetSkill_Pached
        {
            public static  void SetSkill(int typ)
            {
                Main.Logger.Log("public void SetSkill(int typ)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveSkill")]
        static class HomeSystem_RemoveSkill_Pached
        {
            private static  void RemoveSkill()
            {
                Main.Logger.Log("private void RemoveSkill()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateGongFaInformation")]
        static class HomeSystem_UpdateGongFaInformation_Pached
        {
            public static  void UpdateGongFaInformation(int id)
            {
                Main.Logger.Log("public void UpdateGongFaInformation(int id)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateSkillInformation")]
        static class HomeSystem_UpdateSkillInformation_Pached
        {
            public static  void UpdateSkillInformation(int id)
            {
                Main.Logger.Log("public void UpdateSkillInformation(int id)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StartStudy")]
        static class HomeSystem_StartStudy_Pached
        {
            public static  void StartStudy()
            {
                Main.Logger.Log("public void StartStudy()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "Study")]
        static class HomeSystem_Study_Pached
        {
            public static  void Study()
            {
                Main.Logger.Log("public void Study()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetStudySkillWindow")]
        static class HomeSystem_SetStudySkillWindow_Pached
        {
            private static  void SetStudySkillWindow()
            {
                Main.Logger.Log("private void SetStudySkillWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "MoveStudyImage")]
        static class HomeSystem_MoveStudyImage_Pached
        {
            public static  void MoveStudyImage(int index)
            {
                Main.Logger.Log("public void MoveStudyImage(int index)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateStudySkillWindow")]
        static class HomeSystem_UpdateStudySkillWindow_Pached
        {
            public static  void UpdateStudySkillWindow()
            {
                Main.Logger.Log("public void UpdateStudySkillWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetStudySkill")]
        static class HomeSystem_SetStudySkill_Pached
        {
            public static  void SetStudySkill()
            {
                Main.Logger.Log("public void SetStudySkill()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetChooseStudyWindow")]
        static class HomeSystem_SetChooseStudyWindow_Pached
        {
            private static  void SetChooseStudyWindow()
            {
                Main.Logger.Log("private void SetChooseStudyWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveStudySkill")]
        static class HomeSystem_RemoveStudySkill_Pached
        {
            public static  void RemoveStudySkill()
            {
                Main.Logger.Log("public void RemoveStudySkill()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StudySkillUp")]
        static class HomeSystem_StudySkillUp_Pached
        {
            public static  void StudySkillUp()
            {
                Main.Logger.Log("public void StudySkillUp()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateLevelUPSkillWindow")]
        static class HomeSystem_UpdateLevelUPSkillWindow_Pached
        {
            public static  void UpdateLevelUPSkillWindow()
            {
                Main.Logger.Log("public void UpdateLevelUPSkillWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveLevelUPSkill")]
        static class HomeSystem_RemoveLevelUPSkill_Pached
        {
            public static  void RemoveLevelUPSkill()
            {
                Main.Logger.Log("public void RemoveLevelUPSkill()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StartStudyGongFa")]
        static class HomeSystem_StartStudyGongFa_Pached
        {
            public static  void StartStudyGongFa()
            {
                Main.Logger.Log("public void StartStudyGongFa()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateReadBookWindow")]
        static class HomeSystem_UpdateReadBookWindow_Pached
        {
            public static  void UpdateReadBookWindow()
            {
                Main.Logger.Log("public void UpdateReadBookWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetChooseBookWindow")]
        static class HomeSystem_SetChooseBookWindow_Pached
        {
            public static  void SetChooseBookWindow()
            {
                Main.Logger.Log("public void SetChooseBookWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "CloseBookWindow")]
        static class HomeSystem_CloseBookWindow_Pached
        {
            public static  void CloseBookWindow()
            {
                Main.Logger.Log("public void CloseBookWindow()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetBook")]
        static class HomeSystem_SetBook_Pached
        {
            private static  void SetBook()
            {
                Main.Logger.Log("private void SetBook()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveBook")]
        static class HomeSystem_RemoveBook_Pached
        {
            private static  void RemoveBook()
            {
                Main.Logger.Log("private void RemoveBook()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "UpdateBookInformation")]
        static class HomeSystem_UpdateBookInformation_Pached
        {
            public static  void UpdateBookInformation(int id)
            {
                Main.Logger.Log("public void UpdateBookInformation(int id)");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetReadBookId")]
        static class HomeSystem_SetReadBookId_Pached
        {
            public static  void SetReadBookId()
            {
                Main.Logger.Log("public void SetReadBookId()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "RemoveReadBook")]
        static class HomeSystem_RemoveReadBook_Pached
        {
            public static  void RemoveReadBook()
            {
                Main.Logger.Log("public void RemoveReadBook()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StartReadBook")]
        static class HomeSystem_StartReadBook_Pached
        {
            public static  void StartReadBook()
            {
                Main.Logger.Log("public void StartReadBook()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StartAllBuildingNeedResourceCache")]
        static class HomeSystem_StartAllBuildingNeedResourceCache_Pached
        {
            public static  void StartAllBuildingNeedResourceCache()
            {
                Main.Logger.Log("public void StartAllBuildingNeedResourceCache()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "StopAllBuildingNeedResourceCache")]
        static class HomeSystem_StopAllBuildingNeedResourceCache_Pached
        {
            public static  void StopAllBuildingNeedResourceCache()
            {
                Main.Logger.Log("public void StopAllBuildingNeedResourceCache()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "ShowSkillView")]
        static class HomeSystem_ShowSkillView_Pached
        {
            public static  void ShowSkillView()
            {
                Main.Logger.Log("public void ShowSkillView()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "SetFamilySkill")]
        static class HomeSystem_SetFamilySkill_Pached
        {
            private static  void SetFamilySkill()
            {
                Main.Logger.Log("private void SetFamilySkill()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "Awake")]
        static class HomeSystem_Awake_Pached
        {
            private static  void Awake()
            {
                Main.Logger.Log("private void Awake()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "Start")]
        static class HomeSystem_Start_Pached
        {
            private static  void Start()
            {
                Main.Logger.Log("private void Start()");
            }
        }


        [HarmonyPatch(typeof(HomeSystem), "LateUpdate")]
        static class HomeSystem_LateUpdate_Pached
        {
            private static  void LateUpdate()
            {
                Main.Logger.Log("private void LateUpdate()");
            }
        }


    }
}