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
using System.Diagnostics;
using UnityEngine.EventSystems;

namespace GuiTest
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
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
            #endregion

            HarmonyInstance harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());


            return true;
        }

        static string title = "鬼的测试";
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

            //if (GUILayout.Button("测试"))
            //{
            //    ActorMenu.instance.listActorsHolder.root.gameObject.SetActive(!ActorMenu.instance.listActorsHolder.root.gameObject.activeSelf);
            //    Main.Logger.Log(ActorMenu.instance.listActorsHolder.root.ToString());
            //    Main.Logger.Log(ActorMenu.instance.listActorsHolder.ToString());
            //}

            //if (GUILayout.Button("打印"))
            //{
            //    GuiBaseUI.Main.LogAllChild(ActorMenu.instance.listActorsHolder.root);
                
            //}
        }

        //[HarmonyPatch(typeof(DropUpdate), "Update")]
        //public static class DropUpdate_Update_Patch
        //{
        //    public static bool Prefix()
        //    {
        //        if (!Main.enabled)
        //            return true;

        //        int updateId = DropUpdate.instance.updateId;
        //        if (updateId != -1)
        //        {
        //            Main.Logger.Log("DropUpdate_Update_Patch:updateId=" + updateId);
        //        }

        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(DateFile), "DragObjectEnd")]
        //public static class DateFile_DragObjectEnd_Patch
        //{
        //    public static bool Prefix(int typ)
        //    {
        //        if (!Main.enabled)
        //            return true;

        //            Main.Logger.Log("DateFile_DragObjectEnd_Patch:typ=" + typ);

        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(DragObject), "OnBeginDrag")]
        //public static class DragObject_OnBeginDrag_Patch
        //{
        //    public static bool Prefix(PointerEventData eventData)
        //    {
        //        if (!Main.enabled)
        //            return true;

        //        Main.Logger.Log("DateFile_DragObjectEnd_Patch:eventData=" + eventData);

        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(DateFile), "ChangeTwoActorItem")]
        //public static class DateFile_ChangeTwoActorItem_Patch
        //{
        //    public static bool Prefix(int loseItemActorId, int getItemActorId, int itemId, int itemNumber = 1, int getTyp = -1, int partId = 0, int placeId = 0)
        //    {
        //        if (!Main.enabled)
        //            return true;

        //        Main.Logger.Log("loseItemActorId=" + loseItemActorId+ " getItemActorId="+ getItemActorId + " itemId=" + itemId + " itemNumber=" + itemNumber + " getTyp=" + getTyp + " partId=" + partId + " placeId=" + placeId);

        //        return true;
        //    }
        //}


//        public static bool showGongFa
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showGongFa", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (bool)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showGongFa", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static int showGongFaId
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showGongFaId", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (int)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showGongFaId", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static int showGongFaLevel
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showGongFaLevel", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (int)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showGongFaLevel", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static int showGongFaActorId
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showGongFaActorId", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (int)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showGongFaActorId", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static int showWeaponId
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showWeaponId", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (int)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showWeaponId", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static int tipsW
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("tipsW", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (int)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("tipsW", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static int tipsH
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("tipsH", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (int)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("tipsH", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static int cursorTextIndex
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("cursorTextIndex", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (int)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("cursorTextIndex", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static string baseGongFaMassage
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("baseGongFaMassage", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (string)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("baseGongFaMassage", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static string baseWeaponMassage
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("baseWeaponMassage", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (string)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("baseWeaponMassage", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static bool showWeapon
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showWeapon", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (bool)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showWeapon", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static GameObject tipsDate
//        {
//            get
//            {
//                try
//                {
//                    FieldInfo fieldInfo = typeof(WindowManage).GetField("tipsDate", BindingFlags.NonPublic | BindingFlags.Instance);
//                    return (GameObject)fieldInfo.GetValue(WindowManage.instance);
//                }
//                catch
//                {
//                    return null;
//                }
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("tipsDate", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static bool anTips
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("anTips", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (bool)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("anTips", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static string showName
//        {
//            get
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showName", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (string)fieldInfo.GetValue(WindowManage.instance);
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("showName", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static Text itemLevelText
//        {
//            get
//            {
//                try
//                {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("itemLevelText", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (Text)fieldInfo.GetValue(WindowManage.instance);
//                }
//                catch
//                {
//                    return null;
//                }
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("itemLevelText", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static Text itemMoneyText
//        {
//            get
//            {
//                try
//                {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("itemMoneyText", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (Text)fieldInfo.GetValue(WindowManage.instance);
//                }
//                catch
//                {
//                    return null;
//                }
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("itemMoneyText", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static Text informationName
//        {
//            get
//            {
//                try
//                {
//                    FieldInfo fieldInfo = typeof(WindowManage).GetField("informationName", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (Text)fieldInfo.GetValue(WindowManage.instance);
//            }
//                catch
//                {
//                return null;
//            }
//        }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("informationName", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static Text informationMassage
//        {
//            get
//            {
//                try
//                {
//                    FieldInfo fieldInfo = typeof(WindowManage).GetField("informationMassage", BindingFlags.NonPublic | BindingFlags.Instance);
//                return (Text)fieldInfo.GetValue(WindowManage.instance);
//                }
//                catch
//                {
//                    return null;
//                }
//            }
//            set
//            {
//                FieldInfo fieldInfo = typeof(WindowManage).GetField("informationMassage", BindingFlags.NonPublic | BindingFlags.Instance);
//                fieldInfo.SetValue(WindowManage.instance, value);
//            }
//        }

//        public static string Dit()
//        {
//            return WindowManage.instance.Dit();
//        }

//        public static string Mut()
//        {
//            return WindowManage.instance.Mut();
//        }

//        public static void UpdateMianPlaceMassage(int mianPartId, int minaPlaceId)
//        {
//            WindowManage.instance.UpdateMianPlaceMassage(mianPartId, minaPlaceId);
//        }
//        public static string SetMassageTitle(int id, int index1, int index2, int color = 10002)
//        {
//            return WindowManage.instance.SetMassageTitle(id, index1, index2, color);
//        }
//        private static void ShowFeaturesMassage(int featuresId)
//        {
//            Type type = WindowManage.instance.GetType();
//            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
//            var m = type.GetMethod("ShowFeaturesMassage", flags);
//            object[] parameters = new object[] { featuresId };
//            m.Invoke(WindowManage.instance, parameters);
//        }
//        private static int UpdateEquipMassage(int actorId)
//        {
//            Type type = WindowManage.instance.GetType();
//            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
//            var m = type.GetMethod("UpdateEquipMassage", flags);
//            object[] parameters = new object[] { actorId };
//            var obj = m.Invoke(WindowManage.instance, parameters);
//            return (int)obj;
//        }
//        private static void ShowItemMassage(int itemId, int itemTyp, bool setName = true, int showActorId = -1, int shopBookTyp = 0)
//        {
//            Type type = WindowManage.instance.GetType();
//            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
//            var m = type.GetMethod("UpdateEquipMassage", flags);
//            object[] parameters = new object[] { itemId, itemTyp , setName , showActorId , shopBookTyp };
//            m.Invoke(WindowManage.instance, parameters);
//        }

//        private static void ShowGongFaMassage(int skillId, int skillTyp, int levelTyp = -1, int actorId = -1, Toggle toggle = null)
//        {
//            Type type = WindowManage.instance.GetType();
//            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
//            var m = type.GetMethod("UpdateEquipMassage", flags);
//            object[] parameters = new object[] { skillId, skillTyp , levelTyp , actorId , toggle };
//            m.Invoke(WindowManage.instance, parameters);
//        }


//        [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
//        public static class WindowManage_WindowSwitch_Patch
//        {
//            public static bool Prefix(bool on, GameObject tips = null)
//            {
//                if (!Main.enabled)
//                    return true;

//                Main.Logger.Log("WindowSwitch");
//Main.Logger.Log("flag");
//                showGongFa = false;
//Main.Logger.Log("flag");                showGongFaId = 0;
//Main.Logger.Log("flag");                showGongFaLevel = 0;
//Main.Logger.Log("flag");                showGongFaActorId = 0;
//Main.Logger.Log("flag");                baseGongFaMassage = "";
//Main.Logger.Log("flag");                showWeapon = false;
//Main.Logger.Log("flag");                showWeaponId = 0;
//Main.Logger.Log("flag");                baseWeaponMassage = "";
//Main.Logger.Log("flag");                tipsW = 520;
//Main.Logger.Log("flag");                tipsH = 50;
//Main.Logger.Log("flag");                tipsDate = tips;
//Main.Logger.Log("flag");                cursorTextIndex = 1;
//Main.Logger.Log("flag");                bool flag = false;
//Main.Logger.Log("flag");                Main.Logger.Log("tips is null : " + (tips == null));
//Main.Logger.Log("flag");                if (tips == null)
//                {
//                    anTips = flag;
//Main.Logger.Log("flag");                }
//                else
//                {
//                    itemLevelText.text = "";
//Main.Logger.Log("flag");                    itemMoneyText.text = "";
//Main.Logger.Log("flag");                    showName = tips.name;
//Main.Logger.Log("flag");                    Main.Logger.Log("on = " + on);
//Main.Logger.Log("flag");                    if (on)
//                    {
//                        flag = true;
//Main.Logger.Log("flag");                        int num = DateFile.instance.MianActorID();
//Main.Logger.Log("flag");                        string[] array = tips.name.Split(',');
//Main.Logger.Log("flag");                        int num2 = (array.Length > 1) ? DateFile.instance.ParseInt(array[1]) : 0;
//Main.Logger.Log("flag");                        if ((num2 == 634 || num2 == 635) && StartBattle.instance.startBattleWindow.activeSelf && StartBattle.instance.enemyTeamId == 4)
//                        {
//                            flag = false;
//Main.Logger.Log("flag");                        }
//                        Main.Logger.Log("tips.tag " + tips.tag);
//Main.Logger.Log("flag");                        switch (tips.tag)
//                        {
//                            case "SystemIcon":
//                                informationName.text = DateFile.instance.massageDate[num2][0];
//Main.Logger.Log("flag");                                Main.Logger.Log("num2 = " + num2);
//Main.Logger.Log("flag");                                switch (num2)
//                                {
//                                    case 517:
//                                        informationMassage.text = $"{DateFile.instance.massageDate[num2][1].Split('|')[0]}{DateFile.instance.addGongFaStudyValue.ToString()}{DateFile.instance.massageDate[num2][1].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 518:
//                                        informationMassage.text = $"{DateFile.instance.massageDate[num2][1].Split('|')[0]}{DateFile.instance.addSkillStudyValue.ToString()}{DateFile.instance.massageDate[num2][1].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 519:
//                                        informationMassage.text = $"{DateFile.instance.massageDate[num2][1].Split('|')[0]}{DateFile.instance.SetColoer(20010, DateFile.instance.massageDate[519][2].Split('|')[Mathf.Abs(DateFile.instance.xxComeTyp) - 2001])}{DateFile.instance.massageDate[num2][1].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 520:
//                                        informationMassage.text = $"{DateFile.instance.massageDate[num2][1].Split('|')[0]}{DateFile.instance.SetColoer(20010, DateFile.instance.massageDate[519][2].Split('|')[Mathf.Abs(DateFile.instance.xxComeTyp) - 2001])}{DateFile.instance.massageDate[num2][1].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 523:
//                                        {
//                                            informationMassage.text = $"{DateFile.instance.massageDate[num2][1].Split('|')[0]}{WorldMapSystem.instance.MapHealingNeed(WorldMapSystem.instance.choosePartId, WorldMapSystem.instance.choosePlaceId)}{DateFile.instance.massageDate[num2][1].Split('|')[1]}";
//Main.Logger.Log("flag");                                            Text text2;
//Main.Logger.Log("flag");                                            if (!DateFile.instance.mapHealing1)
//                                            {
//                                                text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text += DateFile.instance.massageDate[num2][1].Split('|')[2];
//Main.Logger.Log("flag");                                            }
//                                            text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += "\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 524:
//                                        {
//                                            informationMassage.text = $"{DateFile.instance.massageDate[num2][1].Split('|')[0]}{WorldMapSystem.instance.MapHealingNeed(WorldMapSystem.instance.choosePartId, WorldMapSystem.instance.choosePlaceId)}{DateFile.instance.massageDate[num2][1].Split('|')[1]}";
//Main.Logger.Log("flag");                                            Text text2;
//Main.Logger.Log("flag");                                            if (!DateFile.instance.mapHealing2)
//                                            {
//                                                text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text += DateFile.instance.massageDate[num2][1].Split('|')[2];
//Main.Logger.Log("flag");                                            }
//                                            text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += "\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 525:
//                                        {
//                                            int num59 = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                            if (DateFile.instance.HaveLifeDate(num59, 1001))
//                                            {
//                                                int lifeDate = DateFile.instance.GetLifeDate(num59, 1001, 0);
//Main.Logger.Log("flag");                                                int lifeDate2 = DateFile.instance.GetLifeDate(num59, 1001, 1);
//Main.Logger.Log("flag");                                                string text9 = DateFile.instance.SetColoer(10002, DateFile.instance.identityDate[lifeDate][0]);
//Main.Logger.Log("flag");                                                informationName.text = string.Format("{0}{1} {2}", DateFile.instance.massageDate[num2][0], text9, DateFile.instance.SetColoer(20005, "（" + lifeDate2 + "%）"));
//Main.Logger.Log("flag");                                                if (lifeDate2 >= 100)
//                                                {
//                                                    informationMassage.text = $"{DateFile.instance.GetActorName(num59)}{DateFile.instance.massageDate[525][4].Split('|')[0]}{DateFile.instance.GetActorName()}{DateFile.instance.massageDate[525][4].Split('|')[1]}{DateFile.instance.identityDate[lifeDate][98] + text9}{DateFile.instance.massageDate[525][4].Split('|')[2]}\n\n{DateFile.instance.massageDate[525][1].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                                }
//                                                else
//                                                {
//                                                    informationMassage.text = $"{DateFile.instance.GetActorName(num59)}{DateFile.instance.massageDate[525][3].Split('|')[0]}{DateFile.instance.GetActorName()}{DateFile.instance.massageDate[525][3].Split('|')[1]}{DateFile.instance.identityDate[lifeDate][98] + text9}{DateFile.instance.massageDate[525][3].Split('|')[2]}\n\n{DateFile.instance.massageDate[525][1].Split('|')[0]}\n";
//Main.Logger.Log("flag");                                                }
//                                            }
//                                            else
//                                            {
//                                                informationName.text = $"{DateFile.instance.massageDate[num2][0]}{DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[7][0].Split('|')[0])}";
//Main.Logger.Log("flag");                                                informationMassage.text = $"{DateFile.instance.GetActorName(num59)}{DateFile.instance.massageDate[525][2].Split('|')[0]}{DateFile.instance.GetActorName()}{DateFile.instance.massageDate[525][2].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 611:
//                                        informationMassage.text = (StorySystem.instance.storySystem.activeInHierarchy ? (DateFile.instance.massageDate[num2][2] + "\n") : (DateFile.instance.massageDate[num2][1] + "\n"));
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 630:
//                                        {
//                                            string arg = StorySystem.instance.UpdateToStoryButtonMassage();
//Main.Logger.Log("flag");                                            informationMassage.text = $"{arg}{DateFile.instance.massageDate[num2][1]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 642:
//                                        {
//                                            string arg3 = HomeSystem.instance.UpdateCanBuildingButtonMassage();
//Main.Logger.Log("flag");                                            informationMassage.text = $"{arg3}{DateFile.instance.massageDate[num2][1]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 643:
//                                        {
//                                            string arg2 = HomeSystem.instance.UpdateCanUpButtonMassage();
//Main.Logger.Log("flag");                                            informationMassage.text = $"{arg2}{DateFile.instance.massageDate[num2][1]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 644:
//                                        informationName.text = DateFile.instance.massageDate[num2][HomeSystem.instance.removeTyp * 2];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[num2][1 + HomeSystem.instance.removeTyp * 2] + "\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 658:
//                                        informationMassage.text = $"{MakeSystem.instance.buttonMassage}{DateFile.instance.massageDate[num2][1]}\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 659:
//                                        informationMassage.text = $"{MakeSystem.instance.buttonMassage}{DateFile.instance.massageDate[num2][1]}\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 660:
//                                        informationMassage.text = $"{MakeSystem.instance.buttonMassage}{DateFile.instance.massageDate[num2][1]}\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 661:
//                                        informationMassage.text = $"{MakeSystem.instance.buttonMassage}{DateFile.instance.massageDate[num2][1]}\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 707:
//                                        {
//                                            string text8 = "";
//Main.Logger.Log("flag");                                            if ((DateFile.instance.newActorSurname ?? "").Length == 0)
//                                            {
//                                                text8 = text8 + DateFile.instance.massageDate[num2][2].Split('|')[0] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            if ((DateFile.instance.newActorName ?? "").Length == 0)
//                                            {
//                                                text8 = text8 + DateFile.instance.massageDate[num2][2].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            if (NewGame.instance.GetAbilityP() > 0)
//                                            {
//                                                text8 = text8 + DateFile.instance.massageDate[num2][2].Split('|')[2] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            informationMassage.text = $"{text8}{DateFile.instance.massageDate[num2][1]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 713:
//                                        {
//                                            int actorQiTyp = DateFile.instance.GetActorQiTyp(num);
//Main.Logger.Log("flag");                                            informationName.text = $"{DateFile.instance.massageDate[num2][2]}\n{DateFile.instance.SetColoer(DateFile.instance.ParseInt(DateFile.instance.qiValueStateDate[actorQiTyp][98]), DateFile.instance.qiValueStateDate[actorQiTyp][0])}";
//Main.Logger.Log("flag");                                            informationMassage.text = string.Format("{0}\n\n{1}\n\n{2}\n{3}\n{4}\n", DateFile.instance.qiValueStateDate[actorQiTyp][99], DateFile.instance.massageDate[num2][1], DateFile.instance.massageDate[5002][3].Split('|')[0], string.Format("{0} {1}", DateFile.instance.massageDate[5002][3].Split('|')[1], DateFile.instance.SetColoer(20004, "+" + DateFile.instance.ParseInt(DateFile.instance.qiValueStateDate[actorQiTyp][6]))), string.Format("{0} {1}", DateFile.instance.massageDate[5002][3].Split('|')[2], DateFile.instance.SetColoer(20004, "+" + DateFile.instance.ParseInt(DateFile.instance.qiValueStateDate[actorQiTyp][7]))));
//Main.Logger.Log("flag");                                            int num18;
//Main.Logger.Log("flag");                                            for (int num74 = 0; num74 < 6; num74 = num18 + 1)
//                                            {
//                                                int num75 = DateFile.instance.ParseInt(DateFile.instance.qiValueStateDate[actorQiTyp][8 + num74]);
//Main.Logger.Log("flag");                                                if (num75 > 0)
//                                                {
//                                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                    text2.text += string.Format("{0} {1}\n", DateFile.instance.massageDate[5002][3].Split('|')[3 + num74], DateFile.instance.SetColoer(20005, "+" + num75));
//Main.Logger.Log("flag");                                                }
//                                                num18 = num74;
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 758:
//                                        {
//                                            int actorMianQi = DateFile.instance.GetActorMianQi(ActorMenu.instance.acotrId);
//Main.Logger.Log("flag");                                            int num76 = actorMianQi / 2000;
//Main.Logger.Log("flag");                                            int num77 = DateFile.instance.ParseInt(DateFile.instance.ageDate[num76][302]);
//Main.Logger.Log("flag");                                            informationMassage.text = string.Format("{0}{1}{2}{3}{4}", DateFile.instance.massageDate[num2][1].Split('|')[0], DateFile.instance.SetColoer(DateFile.instance.ParseInt(DateFile.instance.ageDate[num76][303]), DateFile.instance.ageDate[num76][301]), DateFile.instance.massageDate[num2][1].Split('|')[1], DateFile.instance.SetColoer((num77 > 0) ? 20004 : 20010, ((num77 > 0) ? "+" : "-") + Mathf.Abs(num77) + "%"), DateFile.instance.massageDate[num2][1].Split('|')[2]);
//Main.Logger.Log("flag");                                            int num78 = Mathf.Clamp(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(ActorMenu.instance.acotrId, 40)), 0, 8000);
//Main.Logger.Log("flag");                                            Text text2;
//Main.Logger.Log("flag");                                            if (num78 > 0)
//                                            {
//                                                text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text += $"{DateFile.instance.massageDate[758][4].Split('|')[0]}{num78 / 50}{DateFile.instance.massageDate[758][4].Split('|')[1]}{num78 / 10}{DateFile.instance.massageDate[758][4].Split('|')[2]}";
//Main.Logger.Log("flag");                                            }
//                                            int num79 = DateFile.instance.ActorIsInBattle(ActorMenu.instance.acotrId);
//Main.Logger.Log("flag");                                            bool isActor2 = num79 == 0 || num79 == 1;
//Main.Logger.Log("flag");                                            if (ActorMenu.instance.actorMenu.activeInHierarchy)
//                                            {
//                                                isActor2 = !ActorMenu.instance.isEnemy;
//Main.Logger.Log("flag");                                            }
//                                            int num80 = BattleVaule.instance.GetDeferDefuse(isActor2, ActorMenu.instance.acotrId, num79 != 0, 2, 0);
//Main.Logger.Log("flag");                                            if (actorMianQi > num80)
//                                            {
//                                                num80 /= 2;
//Main.Logger.Log("flag");                                            }
//                                            if (num80 > 0)
//                                            {
//                                                text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text += $"{DateFile.instance.massageDate[758][3].Split('|')[0]}{num80 / 10}{DateFile.instance.massageDate[758][3].Split('|')[1]}";
//Main.Logger.Log("flag");                                            }
//                                            if (num76 >= 4)
//                                            {
//                                                text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text += DateFile.instance.massageDate[758][2];
//Main.Logger.Log("flag");                                            }
//                                            text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += "\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 762:
//                                        informationMassage.text = ((DateFile.instance.ParseInt(DateFile.instance.enemyTeamDate[StartBattle.instance.enemyTeamId][5]) == 0) ? (DateFile.instance.massageDate[num2][2] + "\n") : (DateFile.instance.massageDate[num2][1] + "\n"));
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 763:
//                                        informationMassage.text = $"{DateFile.instance.massageDate[num2][1].Split('|')[0]}{50}{DateFile.instance.massageDate[num2][1].Split('|')[1]}{20}{DateFile.instance.massageDate[num2][1].Split('|')[2]}\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case 770:
//                                        {
//                                            int num68 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num, 706, addValue: false));
//Main.Logger.Log("flag");                                            informationMassage.text = $"{DateFile.instance.massageDate[num2][1]}\n\n{DateFile.instance.massageDate[num2][2].Split('|')[0]}{DateFile.instance.SetColoer(20003, num68.ToString())}\n{DateFile.instance.massageDate[num2][2].Split('|')[1]}{DateFile.instance.SetColoer(20003, (num68 / 10).ToString())}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 773:
//                                        {
//                                            int num60 = DateFile.instance.homeBuildingsDate[HomeSystem.instance.homeMapPartId][HomeSystem.instance.homeMapPlaceId][HomeSystem.instance.homeMapbuildingIndex][7];
//Main.Logger.Log("flag");                                            informationName.text = DateFile.instance.massageDate[num2][0].Split('|')[num60];
//Main.Logger.Log("flag");                                            informationMassage.text = DateFile.instance.massageDate[num2][1].Split('|')[num60] + "\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 801:
//                                        informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n";
//Main.Logger.Log("flag");                                        if (DateFile.instance.mianPartId != HomeSystem.instance.homeMapPartId || DateFile.instance.mianPlaceId != HomeSystem.instance.homeMapPlaceId)
//                                        {
//                                            Text text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += DateFile.instance.massageDate[num2][2];
//Main.Logger.Log("flag");                                        }
//                                        break;
//Main.Logger.Log("flag");                                    case 802:
//                                        informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n";
//Main.Logger.Log("flag");                                        if (DateFile.instance.mianPartId != HomeSystem.instance.homeMapPartId || DateFile.instance.mianPlaceId != HomeSystem.instance.homeMapPlaceId)
//                                        {
//                                            Text text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += DateFile.instance.massageDate[num2][2];
//Main.Logger.Log("flag");                                        }
//                                        break;
//Main.Logger.Log("flag");                                    case 803:
//                                        informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n";
//Main.Logger.Log("flag");                                        if (DateFile.instance.mianPartId != HomeSystem.instance.homeMapPartId || DateFile.instance.mianPlaceId != HomeSystem.instance.homeMapPlaceId)
//                                        {
//                                            Text text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += DateFile.instance.massageDate[num2][2];
//Main.Logger.Log("flag");                                        }
//                                        break;
//Main.Logger.Log("flag");                                    case 804:
//                                        informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n";
//Main.Logger.Log("flag");                                        if (DateFile.instance.mianPartId != HomeSystem.instance.homeMapPartId || DateFile.instance.mianPlaceId != HomeSystem.instance.homeMapPlaceId)
//                                        {
//                                            Text text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += DateFile.instance.massageDate[num2][2];
//Main.Logger.Log("flag");                                        }
//                                        break;
//Main.Logger.Log("flag");                                    case 806:
//                                        informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n";
//Main.Logger.Log("flag");                                        if (DateFile.instance.mianPartId != HomeSystem.instance.homeMapPartId || DateFile.instance.mianPlaceId != HomeSystem.instance.homeMapPlaceId)
//                                        {
//                                            Text text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += DateFile.instance.massageDate[num2][2];
//Main.Logger.Log("flag");                                        }
//                                        break;
//Main.Logger.Log("flag");                                    case 809:
//                                        {
//                                            informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n\n";
//Main.Logger.Log("flag");                                            int num18;
//Main.Logger.Log("flag");                                            for (int num61 = 0; num61 < 6; num61 = num18 + 1)
//                                            {
//                                                int num62 = DateFile.instance.storyDebuffs[61 + num61];
//Main.Logger.Log("flag");                                                if (num62 > 0)
//                                                {
//                                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                    text2.text += string.Format("{0}{1}{2}{3}\n", Dit(), DateFile.instance.actorAttrDate[num61][0], DateFile.instance.massageDate[10][3], DateFile.instance.SetColoer(20010, "-" + num62));
//Main.Logger.Log("flag");                                                }
//                                                num18 = num61;
//Main.Logger.Log("flag");                                            }
//                                            for (int num63 = 0; num63 < 6; num63 = num18 + 1)
//                                            {
//                                                int num64 = DateFile.instance.storyDebuffs[81 + num63];
//Main.Logger.Log("flag");                                                if (num64 > 0)
//                                                {
//                                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                    text2.text += string.Format("{0}{1}{2}{3}\n", Dit(), DateFile.instance.actorAttrDate[num63 + 6][0], DateFile.instance.massageDate[10][3], DateFile.instance.SetColoer(20010, "-" + num64));
//Main.Logger.Log("flag");                                                }
//                                                num18 = num63;
//Main.Logger.Log("flag");                                            }
//                                            int count5 = DateFile.instance.storyBuffDate.Count;
//Main.Logger.Log("flag");                                            for (int num65 = 0; num65 < count5; num65 = num18 + 1)
//                                            {
//                                                int num66 = num65 + 1;
//Main.Logger.Log("flag");                                                int num67 = DateFile.instance.storyDebuffs[-num66];
//Main.Logger.Log("flag");                                                if (num67 > 0)
//                                                {
//                                                    num67 *= DateFile.instance.ParseInt(DateFile.instance.storyBuffDate[num66][98]);
//Main.Logger.Log("flag");                                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                    text2.text += string.Format("{0}{1}{2}\n", Dit(), DateFile.instance.storyBuffDate[num66][0], DateFile.instance.SetColoer(20010, "-" + num67 + DateFile.instance.storyBuffDate[num66][99]));
//Main.Logger.Log("flag");                                                }
//                                                num18 = num65;
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 810:
//                                        {
//                                            informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n\n";
//Main.Logger.Log("flag");                                            int num18;
//Main.Logger.Log("flag");                                            for (int num52 = 0; num52 < 6; num52 = num18 + 1)
//                                            {
//                                                int num53 = DateFile.instance.storyBuffs[61 + num52];
//Main.Logger.Log("flag");                                                if (num53 > 0)
//                                                {
//                                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                    text2.text += string.Format("{0}{1}{2}{3}\n", Dit(), DateFile.instance.actorAttrDate[num52][0], DateFile.instance.massageDate[10][3], DateFile.instance.SetColoer(20005, "+" + num53));
//Main.Logger.Log("flag");                                                }
//                                                num18 = num52;
//Main.Logger.Log("flag");                                            }
//                                            for (int num54 = 0; num54 < 6; num54 = num18 + 1)
//                                            {
//                                                int num55 = DateFile.instance.storyBuffs[81 + num54];
//Main.Logger.Log("flag");                                                if (num55 > 0)
//                                                {
//                                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                    text2.text += string.Format("{0}{1}{2}{3}\n", Dit(), DateFile.instance.actorAttrDate[num54 + 6][0], DateFile.instance.massageDate[10][3], DateFile.instance.SetColoer(20005, "+" + num55));
//Main.Logger.Log("flag");                                                }
//                                                num18 = num54;
//Main.Logger.Log("flag");                                            }
//                                            int count4 = DateFile.instance.storyBuffDate.Count;
//Main.Logger.Log("flag");                                            for (int num56 = 0; num56 < count4; num56 = num18 + 1)
//                                            {
//                                                int num57 = num56 + 1;
//Main.Logger.Log("flag");                                                int num58 = DateFile.instance.storyBuffs[-num57];
//Main.Logger.Log("flag");                                                if (num58 > 0)
//                                                {
//                                                    num58 *= DateFile.instance.ParseInt(DateFile.instance.storyBuffDate[num57][98]);
//Main.Logger.Log("flag");                                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                    text2.text += string.Format("{0}{1}{2}\n", Dit(), DateFile.instance.storyBuffDate[num57][0], DateFile.instance.SetColoer(20005, "+" + num58 + DateFile.instance.storyBuffDate[num57][99]));
//Main.Logger.Log("flag");                                                }
//                                                num18 = num56;
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 829:
//                                        {
//                                            informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n\n";
//Main.Logger.Log("flag");                                            int num69 = DateFile.instance.ParseInt(DateFile.instance.gameLevelDate[DateFile.instance.enemyBorn][1]);
//Main.Logger.Log("flag");                                            int num70 = DateFile.instance.ParseInt(DateFile.instance.gameLevelDate[DateFile.instance.enemyBorn][2]);
//Main.Logger.Log("flag");                                            int num71 = DateFile.instance.ParseInt(DateFile.instance.gameLevelDate[DateFile.instance.enemyBorn][5]);
//Main.Logger.Log("flag");                                            int num72 = DateFile.instance.ParseInt(DateFile.instance.gameLevelDate[DateFile.instance.enemyBorn][3]);
//Main.Logger.Log("flag");                                            int num73 = DateFile.instance.ParseInt(DateFile.instance.gameLevelDate[DateFile.instance.enemyBorn][7]);
//Main.Logger.Log("flag");                                            if (num69 != 100)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.massageDate[num2][2].Split('|')[0] + DateFile.instance.SetColoer((num69 > 100) ? 20005 : ((num69 == 100) ? 20002 : 20010), (num69 > 100) ? ("+" + (num69 - 100) + "%\n") : ("-" + Mathf.Abs(num69 - 100) + "%\n"));
//Main.Logger.Log("flag");                                            }
//                                            if (num70 != 100)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.massageDate[num2][2].Split('|')[1] + DateFile.instance.SetColoer((num70 > 100) ? 20005 : ((num70 == 100) ? 20002 : 20010), (num70 > 100) ? ("+" + (num70 - 100) + "%\n") : ("-" + Mathf.Abs(num70 - 100) + "%\n"));
//Main.Logger.Log("flag");                                            }
//                                            if (num71 != 100)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.massageDate[num2][2].Split('|')[2] + DateFile.instance.SetColoer((num71 > 100) ? 20005 : ((num71 == 100) ? 20002 : 20010), (num71 > 100) ? ("+" + (num71 - 100) + "%\n") : ("-" + Mathf.Abs(num71 - 100) + "%\n"));
//Main.Logger.Log("flag");                                            }
//                                            if (num72 != 100)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.massageDate[num2][2].Split('|')[3] + DateFile.instance.SetColoer((num72 > 100) ? 20005 : ((num72 == 100) ? 20002 : 20010), (num72 > 100) ? ("+" + (num72 - 100) + "%\n") : ("-" + Mathf.Abs(num72 - 100) + "%\n"));
//Main.Logger.Log("flag");                                            }
//                                            if (num73 != 100)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.massageDate[num2][2].Split('|')[4] + DateFile.instance.SetColoer((num73 > 100) ? 20005 : ((num73 == 100) ? 20002 : 20010), (num73 > 100) ? ("+" + (num73 - 100) + "%\n") : ("-" + Mathf.Abs(num73 - 100) + "%\n"));
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 830:
//                                        {
//                                            informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n\n";
//Main.Logger.Log("flag");                                            int num50 = DateFile.instance.ParseInt(DateFile.instance.gameLevelDate[DateFile.instance.enemyBorn][6]);
//Main.Logger.Log("flag");                                            int num51 = DateFile.instance.ParseInt(DateFile.instance.gameLevelDate[DateFile.instance.enemyBorn][4]);
//Main.Logger.Log("flag");                                            if (num50 != 100)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.massageDate[num2][2].Split('|')[0] + DateFile.instance.SetColoer((num50 > 100) ? 20005 : ((num50 == 100) ? 20002 : 20010), (num50 > 100) ? ("+" + (num50 - 100) + "%\n") : ("-" + Mathf.Abs(num50 - 100) + "%\n"));
//Main.Logger.Log("flag");                                            }
//                                            if (num51 != 100)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.massageDate[num2][2].Split('|')[1] + DateFile.instance.SetColoer((num51 > 100) ? 20005 : ((num51 == 100) ? 20002 : 20010), (num51 > 100) ? ("+" + (num51 - 100) + "%\n") : ("-" + Mathf.Abs(num51 - 100) + "%\n"));
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        }
//                                    default:
//                                        informationMassage.text = DateFile.instance.massageDate[num2][1] + "\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                }
//                                break;
//Main.Logger.Log("flag");                            case "GangBookPower":
//                                informationName.text = DateFile.instance.readBookDate[GongFaTreeWindow.showGangId + 1000][0];
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.readBookDate[GongFaTreeWindow.showGangId + 1000][99] + "\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "ScoreBootyIcon":
//                                informationName.text = DateFile.instance.scoreBootyDate[num2][0];
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.scoreBootyDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "TrunEventIcon":
//                                {
//                                    int num86 = DateFile.instance.ParseInt(DateFile.instance.trunEventDate[num2][1]);
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.trunEventDate[num2][0];
//Main.Logger.Log("flag");                                    Main.Logger.Log("num86 = " + num86);
//Main.Logger.Log("flag");                                    switch (num86)
//                                    {
//                                        case 1:
//                                            informationMassage.text = $"{DateFile.instance.SetColoer(10002, $"{DateFile.instance.partWorldMapDate[DateFile.instance.ParseInt(array[2])][0]}-{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 98)}{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 0)}")}{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(20008, DateFile.instance.massageDate[10][0].Split('|')[0] + DateFile.instance.basehomePlaceDate[DateFile.instance.ParseInt(array[4])][0] + DateFile.instance.massageDate[10][0].Split('|')[1])}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        case 2:
//                                            informationMassage.text = $"{DateFile.instance.SetColoer(10002, DateFile.instance.GetActorName(DateFile.instance.ParseInt(array[4])))}{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(10002, $"{DateFile.instance.partWorldMapDate[DateFile.instance.ParseInt(array[2])][0]}-{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 98)}{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 0)}")}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}{DateFile.instance.SetColoer(20001 + DateFile.instance.ParseInt(DateFile.instance.GetItemDate(DateFile.instance.ParseInt(array[5]), 8)), DateFile.instance.massageDate[10][0].Split('|')[0] + DateFile.instance.GetItemDate(DateFile.instance.ParseInt(array[5]), 0, otherMassage: false) + DateFile.instance.massageDate[10][0].Split('|')[1])}{DateFile.instance.trunEventDate[num2][99].Split('|')[2]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        case 3:
//                                            informationMassage.text = $"{DateFile.instance.SetColoer(10002, DateFile.instance.GetActorName(DateFile.instance.ParseInt(array[4])))}{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(10002, $"{DateFile.instance.partWorldMapDate[DateFile.instance.ParseInt(array[2])][0]}-{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 98)}{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 0)}")}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}{DateFile.instance.SetColoer(20008, DateFile.instance.resourceDate[DateFile.instance.ParseInt(array[5])][0])}{DateFile.instance.trunEventDate[num2][99].Split('|')[2]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        case 4:
//                                            informationMassage.text = $"{DateFile.instance.SetColoer(10002, DateFile.instance.GetActorName(DateFile.instance.ParseInt(array[4])))}{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(10002, $"{DateFile.instance.partWorldMapDate[DateFile.instance.ParseInt(array[2])][0]}-{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 98)}{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 0)}")}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}{DateFile.instance.SetColoer(20001 + DateFile.instance.ParseInt(DateFile.instance.gongFaDate[DateFile.instance.ParseInt(array[5])][2]), DateFile.instance.massageDate[11][4].Split('|')[0] + DateFile.instance.gongFaDate[DateFile.instance.ParseInt(array[5])][0] + DateFile.instance.massageDate[11][4].Split('|')[1])}{DateFile.instance.trunEventDate[num2][99].Split('|')[2]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        case 5:
//                                            informationMassage.text = $"{DateFile.instance.SetColoer(10002, DateFile.instance.GetActorName(DateFile.instance.ParseInt(array[4])))}{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(10002, $"{DateFile.instance.partWorldMapDate[DateFile.instance.ParseInt(array[2])][0]}-{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 98)}{DateFile.instance.GetNewMapDate(DateFile.instance.ParseInt(array[2]), DateFile.instance.ParseInt(array[3]), 0)}")}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        case 6:
//                                            informationMassage.text = $"{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(10002, DateFile.instance.GetActorName(DateFile.instance.ParseInt(array[2])))}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}{DateFile.instance.SetColoer(10002, DateFile.instance.partWorldMapDate[DateFile.instance.ParseInt(array[3])][0])}{DateFile.instance.trunEventDate[num2][99].Split('|')[2]}{DateFile.instance.SetColoer(10002, DateFile.instance.GetActorName(DateFile.instance.ParseInt(array[2])))}{DateFile.instance.trunEventDate[num2][99].Split('|')[3]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        case 7:
//                                            informationMassage.text = $"{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(20001 + DateFile.instance.ParseInt(DateFile.instance.GetItemDate(DateFile.instance.ParseInt(array[2]), 8)), DateFile.instance.GetItemDate(DateFile.instance.ParseInt(array[2]), 0, otherMassage: false))}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        case 8:
//                                            informationMassage.text = $"{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(10002, DateFile.instance.GetActorName(DateFile.instance.ParseInt(array[2])))}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}{DateFile.instance.SetColoer(20001 + DateFile.instance.ParseInt(DateFile.instance.GetItemDate(DateFile.instance.ParseInt(array[3]), 8)), DateFile.instance.GetItemDate(DateFile.instance.ParseInt(array[3]), 0, otherMassage: false))}{DateFile.instance.trunEventDate[num2][99].Split('|')[2]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        case 9:
//                                            {
//                                                int num88 = DateFile.instance.ParseInt(array[3]);
//Main.Logger.Log("flag");                                                string text13 = DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(num88, 1)][1001];
//Main.Logger.Log("flag");                                                informationName.text = DateFile.instance.trunEventDate[num2][0] + text13;
//Main.Logger.Log("flag");                                                informationMassage.text = $"{DateFile.instance.SetColoer(10002, DateFile.instance.GetActorName(DateFile.instance.ParseInt(array[2])))}{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(10002, DateFile.instance.massageDate[10][0].Split('|')[0] + DateFile.instance.partWorldMapDate[DateFile.instance.ParseInt(DateFile.instance.GetGangDate(num88, 3))][0] + DateFile.instance.massageDate[10][0].Split('|')[1])}{DateFile.instance.SetColoer(10002, DateFile.instance.GetGangDate(num88, 0))}{DateFile.instance.SetColoer(10002, text13)}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                                break;
//Main.Logger.Log("flag");                                            }
//                                        case 10:
//                                            {
//                                                string text12 = DateFile.instance.partWorldMapDate[DateFile.instance.xxPartIds[0]][0];
//Main.Logger.Log("flag");                                                int num18;
//Main.Logger.Log("flag");                                                for (int num87 = 1; num87 < DateFile.instance.xxPartIds.Count; num87 = num18 + 1)
//                                                {
//                                                    text12 += $"{DateFile.instance.massageDate[10][5]}{DateFile.instance.partWorldMapDate[DateFile.instance.xxPartIds[num87]][0]}";
//Main.Logger.Log("flag");                                                    num18 = num87;
//Main.Logger.Log("flag");                                                }
//                                                informationMassage.text = $"{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(10002, text12)}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                                break;
//Main.Logger.Log("flag");                                            }
//                                        case 11:
//                                            informationMassage.text = $"{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(20010, DateFile.instance.massageDate[519][2].Split('|')[Mathf.Abs(DateFile.instance.xxComeTyp) - 2001])}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        case 12:
//                                            informationMassage.text = $"{DateFile.instance.trunEventDate[num2][99].Split('|')[0]}{DateFile.instance.SetColoer(10002, DateFile.instance.partWorldMapDate[DateFile.instance.ParseInt(array[2])][0])}{DateFile.instance.trunEventDate[num2][99].Split('|')[1]}{DateFile.instance.ParseInt(array[3])}{DateFile.instance.trunEventDate[num2][99].Split('|')[2]}{DateFile.instance.ParseInt(array[4])}{DateFile.instance.trunEventDate[num2][99].Split('|')[3]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        default:
//                                            informationMassage.text = DateFile.instance.trunEventDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                    }
//                                    informationMassage.text = informationMassage.text.Replace("MN", DateFile.instance.GetActorName());
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "PartWorldButton":
//                                {
//                                    int num21 = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                    int num22 = DateFile.instance.baseWorldDate[num2][num21][1];
//Main.Logger.Log("flag");                                    int[] moveTime = WorldMapSystem.instance.GetMoveTime(num2, num21);
//Main.Logger.Log("flag");                                    string text3 = (num22 != -1) ? DateFile.instance.SetColoer(10002, DateFile.instance.GetNewMapDate(num21, num22, 0)) : DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[7][0].Split('|')[0]);
//Main.Logger.Log("flag");                                    informationName.text = $"{DateFile.instance.allWorldDate[num2][0]}\n{DateFile.instance.SetColoer(20008, DateFile.instance.partWorldMapDate[num21][0])}";
//Main.Logger.Log("flag");                                    if (num21 == DateFile.instance.mianPartId)
//                                    {
//                                        informationMassage.text = $"{DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[201][2])}\n\n{SetMassageTitle(8009, 0, 3)}{Dit()}{DateFile.instance.massageDate[8009][0].Split('|')[0]}{text3}\n";
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        informationMassage.text = $"{SetMassageTitle(8009, 0, 3)}{Dit()}{DateFile.instance.massageDate[8009][0].Split('|')[0]}{text3}\n{Dit()}{DateFile.instance.massageDate[8009][0].Split('|')[1]}{DateFile.instance.SetColoer(20005, moveTime[0].ToString())}\n{Dit()}{DateFile.instance.massageDate[8009][0].Split('|')[2]}{DateFile.instance.SetColoer(20008, moveTime[1].ToString())}\n";
//Main.Logger.Log("flag");                                        if (moveTime[2] > 0)
//                                        {
//                                            Text text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += $"\n{DateFile.instance.massageDate[8009][2].Split('|')[0]}{moveTime[2]}{DateFile.instance.massageDate[8009][2].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                        }
//                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "MiniPartWorldButton":
//                                {
//                                    int key = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                    tipsW = 260;
//Main.Logger.Log("flag");                                    tipsH = 20;
//Main.Logger.Log("flag");                                    informationName.text = $"{DateFile.instance.allWorldDate[num2][0]}\n{DateFile.instance.SetColoer(20008, DateFile.instance.partWorldMapDate[key][0])}";
//Main.Logger.Log("flag");                                    informationMassage.text = "";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "PlaceActor":
//                                {
//                                    int num41 = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[1]);
//Main.Logger.Log("flag");                                    tipsW = 1040;
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.GetActorName(num41) + DateFile.instance.massageDate[13][3].Split('|')[(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num41, 26, addValue: false)) > 0) ? 1 : 0];
//Main.Logger.Log("flag");                                    informationMassage.text = "";
//Main.Logger.Log("flag");                                    if (DateFile.instance.actorLifeMassage.ContainsKey(num41))
//                                    {
//                                        int count3 = DateFile.instance.actorLifeMassage[num41].Count;
//Main.Logger.Log("flag");                                        int num42 = Mathf.Max(DateFile.instance.GetActorFavor( false, num, num41), 0);
//Main.Logger.Log("flag");                                        int num18;
//Main.Logger.Log("flag");                                        for (int num43 = Mathf.Max(count3 - 5, 0); num43 < count3; num43 = num18 + 1)
//                                        {
//                                            int[] array8 = DateFile.instance.actorLifeMassage[num41][num43];
//Main.Logger.Log("flag");                                            if (num41 != num && num42 < 30000 * DateFile.instance.ParseInt(DateFile.instance.actorMassageDate[array8[0]][4]) / 100)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text += string.Format(DateFile.instance.SetColoer(20002, "·") + " {0}{1}：{2}\n", DateFile.instance.massageDate[16][1] + DateFile.instance.SetColoer(10002, array8[1].ToString()) + DateFile.instance.massageDate[16][3], DateFile.instance.SetColoer(20002, DateFile.instance.solarTermsDate[array8[2]][0]), DateFile.instance.SetColoer(10001, DateFile.instance.massageDate[12][2]));
//Main.Logger.Log("flag");                                            }
//                                            else
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                Text text6 = text2;
//Main.Logger.Log("flag");                                                string text7 = text2.text;
//Main.Logger.Log("flag");                                                string format = DateFile.instance.SetColoer(20002, "·") + " {0}{1}：" + DateFile.instance.actorMassageDate[array8[0]][1] + "\n";
//Main.Logger.Log("flag");                                                object[] args = ActorMenu.instance.SetMassageText(num41, array8).ToArray();
//Main.Logger.Log("flag");                                                text6.text = text7 + string.Format(format, args);
//Main.Logger.Log("flag");                                            }
//                                            num18 = num43;
//Main.Logger.Log("flag");                                        }
//                                    }
//                                    else
//                                    {
//                                        informationMassage.text = DateFile.instance.massageDate[7][0].Split('|')[0] + "\n";
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "PlaceMassage":
//                                {
//                                    int choosePartId = WorldMapSystem.instance.choosePartId;
//Main.Logger.Log("flag");                                    int choosePlaceId = WorldMapSystem.instance.choosePlaceId;
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.GetNewMapDate(choosePartId, choosePlaceId, 0);
//Main.Logger.Log("flag");                                    if (DateFile.instance.PlaceIsBad(choosePartId, choosePlaceId))
//                                    {
//                                        Text text2 = informationName;
//Main.Logger.Log("flag");                                        text2.text += DateFile.instance.massageDate[6][1];
//Main.Logger.Log("flag");                                    }
//                                    informationMassage.text = DateFile.instance.GetNewMapDate(choosePartId, choosePlaceId, 99) + "\n";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "ResourceIcon":
//                                UIDate.instance.UpdateResourceText(tips.gameObject);
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "SimpleItemIcon":
//                                informationName.text = DateFile.instance.GetItemDate(num2, 0);
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.GetItemDate(num2, 99) + "\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "PoisonIcon":
//                                {
//                                    informationName.text = DateFile.instance.poisonDate[num2][0];
//Main.Logger.Log("flag");                                    int num19 = ActorMenu.instance.MaxPoison(ActorMenu.instance.acotrId, num2);
//Main.Logger.Log("flag");                                    if (DateFile.instance.ParseInt(DateFile.instance.presetActorDate[DateFile.instance.ParseInt(DateFile.instance.GetActorDate(ActorMenu.instance.acotrId, 997, addValue: false))][41 + num2]) == -1)
//                                    {
//                                        num19 = -1;
//Main.Logger.Log("flag");                                    }
//                                    informationMassage.text = $"{DateFile.instance.SetColoer(DateFile.instance.ParseInt(DateFile.instance.poisonDate[num2][2]), DateFile.instance.poisonDate[num2][0] + DateFile.instance.massageDate[760][2])}{DateFile.instance.SetColoer(20003, (num19 == -1) ? DateFile.instance.massageDate[7][2] : num19.ToString())}\n\n";
//Main.Logger.Log("flag");                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                    text2.text = text2.text + DateFile.instance.poisonDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "AddToTeam":
//                                {
//                                    informationName.text = DateFile.instance.massageDate[3003][0];
//Main.Logger.Log("flag");                                    string text5 = "";
//Main.Logger.Log("flag");                                    if (ActorMenu.instance.cantChanageTeam)
//                                    {
//                                        text5 = text5 + DateFile.instance.massageDate[3003][2] + "\n\n";
//Main.Logger.Log("flag");                                    }
//                                    else if (StartBattle.instance.startBattleWindow.activeSelf)
//                                    {
//                                        text5 = text5 + DateFile.instance.massageDate[3003][3] + "\n\n";
//Main.Logger.Log("flag");                                    }
//                                    else if (BattleSystem.instance.battleWindow.activeSelf)
//                                    {
//                                        text5 = text5 + DateFile.instance.massageDate[3003][4] + "\n\n";
//Main.Logger.Log("flag");                                    }
//                                    else if (SkillBattleSystem.instance.skillBattleWindow.activeSelf)
//                                    {
//                                        text5 = text5 + DateFile.instance.massageDate[3014][4] + "\n\n";
//Main.Logger.Log("flag");                                    }
//                                    informationMassage.text = $"{text5}{DateFile.instance.massageDate[3003][1]}\n";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "RemoveFromTeam":
//                                {
//                                    informationName.text = DateFile.instance.massageDate[3004][0];
//Main.Logger.Log("flag");                                    string text17 = "";
//Main.Logger.Log("flag");                                    if (ActorMenu.instance.cantChanageTeam)
//                                    {
//                                        text17 = text17 + DateFile.instance.massageDate[3003][2] + "\n\n";
//Main.Logger.Log("flag");                                    }
//                                    else if (StartBattle.instance.startBattleWindow.activeSelf)
//                                    {
//                                        text17 = text17 + DateFile.instance.massageDate[3003][3] + "\n\n";
//Main.Logger.Log("flag");                                    }
//                                    else if (BattleSystem.instance.battleWindow.activeSelf)
//                                    {
//                                        text17 = text17 + DateFile.instance.massageDate[3003][4] + "\n\n";
//Main.Logger.Log("flag");                                    }
//                                    else if (SkillBattleSystem.instance.skillBattleWindow.activeSelf)
//                                    {
//                                        text17 = text17 + DateFile.instance.massageDate[3014][4] + "\n\n";
//Main.Logger.Log("flag");                                    }
//                                    informationMassage.text = $"{text17}{DateFile.instance.massageDate[3004][1]}\n";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "ActorAttrIcon":
//                                informationName.text = DateFile.instance.actorAttrDate[num2][0];
//Main.Logger.Log("flag");                                Main.Logger.Log("num2 " + num2);
//Main.Logger.Log("flag");                                switch (num2)
//                                {
//                                    case 12:
//                                        {
//                                            int num97 = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                            string arg7 = (DateFile.instance.ParseInt(DateFile.instance.actorAttrDate[num2][1]) == 1) ? string.Format("{0}{1}\n\n", DateFile.instance.SetColoer(10002, DateFile.instance.massageDate[1002][3]), (DateFile.instance.GetAttrAgeVaule(ActorMenu.instance.acotrId, num2) >= 100) ? DateFile.instance.SetColoer(20004, DateFile.instance.GetAttrAgeVaule(ActorMenu.instance.acotrId, num2) + "%") : DateFile.instance.SetColoer(20010, DateFile.instance.GetAttrAgeVaule(ActorMenu.instance.acotrId, num2) + "%")) : "";
//Main.Logger.Log("flag");                                            informationMassage.text = $"{arg7}{DateFile.instance.actorAttrDate[num2][99]}\n";
//Main.Logger.Log("flag");                                            string text15 = "";
//Main.Logger.Log("flag");                                            List<string> list = new List<string>(DateFile.instance.GetActorDate(num97, 1, addValue: false).Split('|'));
//Main.Logger.Log("flag");                                            int num18;
//Main.Logger.Log("flag");                                            for (int num98 = 0; num98 < list.Count; num98 = num18 + 1)
//                                            {
//                                                int num99 = DateFile.instance.ParseInt(list[num98]);
//Main.Logger.Log("flag");                                                if (num99 != 0)
//                                                {
//                                                    text15 += $"{Dit()}{DateFile.instance.generationDate[num99][0]}\n";
//Main.Logger.Log("flag");                                                }
//                                                num18 = num98;
//Main.Logger.Log("flag");                                            }
//                                            List<int> list2 = new List<int>(DateFile.instance.legendBookId.Keys);
//Main.Logger.Log("flag");                                            for (int num100 = 0; num100 < list2.Count; num100 = num18 + 1)
//                                            {
//                                                if (DateFile.instance.legendBookId[list2[num100]] == num97)
//                                                {
//                                                    text15 += $"{Dit()}{DateFile.instance.generationDate[101 + num100][0]}\n";
//Main.Logger.Log("flag");                                                }
//                                                num18 = num100;
//Main.Logger.Log("flag");                                            }
//                                            if (text15 != "")
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + "\n" + text15;
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 19:
//                                        {
//                                            int key15 = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                            if (DateFile.instance.actorFame.ContainsKey(key15) && DateFile.instance.actorFame[key15].Count > 0)
//                                            {
//                                                string text16 = "";
//Main.Logger.Log("flag");                                                List<int> list3 = new List<int>(DateFile.instance.actorFame[key15].Keys);
//Main.Logger.Log("flag");                                                int num18;
//Main.Logger.Log("flag");                                                for (int num101 = 0; num101 < list3.Count; num101 = num18 + 1)
//                                                {
//                                                    int key16 = list3[num101];
//Main.Logger.Log("flag");                                                    int num102 = Mathf.Min(DateFile.instance.actorFame[key15][key16][0], DateFile.instance.ParseInt(DateFile.instance.actorFameDate[key16][4]));
//Main.Logger.Log("flag");                                                    int num103 = DateFile.instance.ParseInt(DateFile.instance.actorFameDate[key16][1]) * num102;
//Main.Logger.Log("flag");                                                    int coloer = (num103 > 0) ? 20004 : 20010;
//Main.Logger.Log("flag");                                                    text16 += string.Format("{5}{0} {1}   {2} {3} {4}\n", DateFile.instance.SetColoer(coloer, DateFile.instance.actorFameDate[key16][0]), DateFile.instance.SetColoer(coloer, ((num103 > 0) ? "+" : "-") + Mathf.Abs(num103).ToString()), DateFile.instance.massageDate[0][4].Split('|')[0], DateFile.instance.SetColoer(20005, DateFile.instance.actorFame[key15][key16][1].ToString()), DateFile.instance.massageDate[0][4].Split('|')[1], Dit());
//Main.Logger.Log("flag");                                                    num18 = num101;
//Main.Logger.Log("flag");                                                }
//                                                informationMassage.text = $"{DateFile.instance.actorAttrDate[num2][99]}\n\n{text16}";
//Main.Logger.Log("flag");                                            }
//                                            else
//                                            {
//                                                informationMassage.text = DateFile.instance.actorAttrDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        }
//                                    case 21:
//                                        informationMassage.text = DateFile.instance.actorAttrDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                        if (array.Length >= 3)
//                                        {
//                                            int num92 = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                            if (num92 != num && DateFile.instance.GetActorFavor( false, num, num92) >= 0)
//                                            {
//                                                string text14 = "";
//Main.Logger.Log("flag");                                                if (DateFile.instance.GetActorGoodness(num) == DateFile.instance.GetActorGoodness(num92))
//                                                {
//                                                    text14 = text14 + DateFile.instance.massageDate[7][5].Split('|')[0] + "\n";
//Main.Logger.Log("flag");                                                }
//                                                if (DateFile.instance.GetLifeDateList(num92, 202).Contains(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num, 19, addValue: false))))
//                                                {
//                                                    text14 = text14 + DateFile.instance.massageDate[7][5].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                                }
//                                                int num18;
//Main.Logger.Log("flag");                                                for (int num93 = 0; num93 < 12; num93 = num18 + 1)
//                                                {
//                                                    if (DateFile.instance.GetActorSocial(num92, 301 + num93, getDieActor: true).Contains(num))
//                                                    {
//                                                        text14 = text14 + DateFile.instance.massageDate[7][5].Split('|')[2 + num93] + "\n";
//Main.Logger.Log("flag");                                                    }
//                                                    num18 = num93;
//Main.Logger.Log("flag");                                                }
//                                                if (DateFile.instance.GetLifeDate(num92, 601, 0) == DateFile.instance.GetLifeDate(num, 601, 0) || DateFile.instance.GetLifeDate(num92, 602, 0) == DateFile.instance.GetLifeDate(num, 602, 0))
//                                                {
//                                                    text14 = text14 + DateFile.instance.massageDate[7][5].Split('|')[14] + "\n";
//Main.Logger.Log("flag");                                                }
//                                                if (DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num92, 19, addValue: false)) == 16)
//                                                {
//                                                    text14 = text14 + DateFile.instance.massageDate[7][5].Split('|')[15] + "\n";
//Main.Logger.Log("flag");                                                }
//                                                int num94 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num, 997, addValue: false));
//Main.Logger.Log("flag");                                                int num95 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num92, 997, addValue: false));
//Main.Logger.Log("flag");                                                if (num94 % 2 == 0)
//                                                {
//                                                    num94--;
//Main.Logger.Log("flag");                                                }
//                                                if (num95 % 2 == 0)
//                                                {
//                                                    num95--;
//Main.Logger.Log("flag");                                                }
//                                                if (num94 == num95)
//                                                {
//                                                    text14 = text14 + DateFile.instance.massageDate[7][5].Split('|')[16] + "\n";
//Main.Logger.Log("flag");                                                }
//                                                if (DateFile.instance.GetLifeDateList(num92, 201).Contains(16))
//                                                {
//                                                    text14 = text14 + DateFile.instance.massageDate[7][5].Split('|')[18] + "\n";
//Main.Logger.Log("flag");                                                }
//                                                if (DateFile.instance.HaveLifeDate(num92, 401) && DateFile.instance.GetActorSocial(num92, 401, getDieActor: true).Contains(num))
//                                                {
//                                                    text14 = text14 + DateFile.instance.massageDate[7][5].Split('|')[17] + "\n";
//Main.Logger.Log("flag");                                                }
//                                                if (DateFile.instance.HaveLifeDate(num92, 402) && DateFile.instance.GetActorSocial(num92, 402, getDieActor: true).Contains(num))
//                                                {
//                                                    text14 = text14 + DateFile.instance.massageDate[7][5].Split('|')[20] + "\n";
//Main.Logger.Log("flag");                                                }
//                                                if (text14 != "")
//                                                {
//                                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                    text2.text = text2.text + "\n" + text14;
//Main.Logger.Log("flag");                                                }
//                                                int num96 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num92, 210, addValue: false));
//Main.Logger.Log("flag");                                                if (num96 > 0)
//                                                {
//                                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                    text2.text += $"\n{DateFile.instance.massageDate[7][5].Split('|')[19]}{DateFile.instance.SetColoer(20010, num96.ToString())}\n{DateFile.instance.massageDate[7017][4]}\n";
//Main.Logger.Log("flag");                                                }
//                                            }
//                                        }
//                                        break;
//Main.Logger.Log("flag");                                    default:
//                                        {
//                                            string arg6 = (DateFile.instance.ParseInt(DateFile.instance.actorAttrDate[num2][1]) == 1) ? string.Format("{0}{1}\n\n", DateFile.instance.SetColoer(10002, DateFile.instance.massageDate[1002][3]), (DateFile.instance.GetAttrAgeVaule(ActorMenu.instance.acotrId, num2) >= 100) ? DateFile.instance.SetColoer(20004, DateFile.instance.GetAttrAgeVaule(ActorMenu.instance.acotrId, num2) + "%") : DateFile.instance.SetColoer(20010, DateFile.instance.GetAttrAgeVaule(ActorMenu.instance.acotrId, num2) + "%")) : "";
//Main.Logger.Log("flag");                                            informationMassage.text = $"{arg6}{DateFile.instance.actorAttrDate[num2][99]}\n";
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                }
//                                break;
//Main.Logger.Log("flag");                            case "ActorWorkTyp":
//                                {
//                                    informationName.text = DateFile.instance.massageDate[7017][2].Split('|')[0];
//Main.Logger.Log("flag");                                    string text20 = (num2 == 18) ? DateFile.instance.massageDate[6][5] : DateFile.instance.baseSkillDate[num2 - 501][0];
//Main.Logger.Log("flag");                                    informationMassage.text = $"{DateFile.instance.massageDate[7017][3].Split('|')[0]}{DateFile.instance.SetColoer(20008, text20)}{DateFile.instance.massageDate[7017][3].Split('|')[1]}\n\n{DateFile.instance.massageDate[7017][3].Split('|')[2]}\n";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "ActorFeature":
//                                ShowFeaturesMassage(num2);
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "ActorEquipingItem":
//                                UpdateEquipMassage(ActorMenu.instance.actorMenu.activeInHierarchy ? ActorMenu.instance.acotrId : DateFile.instance.MianActorID());
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "ActorEquip":
//                                ShowItemMassage(num2, 0);
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "ActorItem":
//                                Main.Logger.Log("num2 " + num2);
//Main.Logger.Log("flag");                                switch (num2)
//                                {
//                                    case -96:
//                                        informationName.text = DateFile.instance.massageDate[8005][1].Split('|')[0];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[8005][1].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case -97:
//                                        informationName.text = DateFile.instance.massageDate[8005][0].Split('|')[0];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[8005][0].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case -98:
//                                        informationName.text = DateFile.instance.massageDate[8001][5].Split('|')[1];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[8001][5].Split('|')[2] + "\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    case -99:
//                                        informationName.text = DateFile.instance.massageDate[8001][4].Split('|')[0];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[8001][4].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                        break;
//Main.Logger.Log("flag");                                    default:
//                                        {
//                                            int shopBookTyp = 0;
//Main.Logger.Log("flag");                                            if (array.Length >= 4)
//                                            {
//                                                if (array[3] == "NeedFavor")
//                                                {
//                                                    shopBookTyp = 1;
//Main.Logger.Log("flag");                                                }
//                                                if (array[3] == "NeedGongFa")
//                                                {
//                                                    shopBookTyp = 2;
//Main.Logger.Log("flag");                                                }
//                                            }
//                                            ShowItemMassage(num2, 1,  true, -1, shopBookTyp);
//Main.Logger.Log("flag");                                            break;
//Main.Logger.Log("flag");                                        }
//                                }
//                                break;
//Main.Logger.Log("flag");                            case "StartBattleItem":
//                                if (num2 == 0)
//                                {
//                                    if (StartBattle.instance.actorUseItemId > 0)
//                                    {
//                                        ShowItemMassage(StartBattle.instance.actorUseItemId, 1);
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        informationName.text = DateFile.instance.massageDate[4002][0];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[4002][1] + "\n";
//Main.Logger.Log("flag");                                    }
//                                }
//                                else if (StartBattle.instance.enemyUseItemId > 0)
//                                {
//                                    ShowItemMassage(StartBattle.instance.enemyUseItemId, 1);
//Main.Logger.Log("flag");                                }
//                                else
//                                {
//                                    informationName.text = DateFile.instance.massageDate[4002][0];
//Main.Logger.Log("flag");                                    informationMassage.text = DateFile.instance.massageDate[4002][2] + "\n";
//Main.Logger.Log("flag");                                }
//                                break;
//Main.Logger.Log("flag");                            case "BattleStateMassage":
//                                {
//                                    int num110 = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                    int key18 = BattleSystem.instance.ActorId(num2 == 0, otherActor: false);
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.massageDate[8008][4].Split('|')[num110];
//Main.Logger.Log("flag");                                    string text18 = "";
//Main.Logger.Log("flag");                                    List<int> list4 = new List<int>(DateFile.instance.battlerStates[key18][num110].Keys);
//Main.Logger.Log("flag");                                    List<int> list5 = new List<int>(DateFile.instance.battleStateDate[1].Keys);
//Main.Logger.Log("flag");                                    int num18;
//Main.Logger.Log("flag");                                    for (int num111 = 0; num111 < list4.Count; num111 = num18 + 1)
//                                    {
//                                        int key19 = list4[num111];
//Main.Logger.Log("flag");                                        if (DateFile.instance.ParseInt(DateFile.instance.battleStateDate[key19][2]) == 1)
//                                        {
//                                            int num112 = DateFile.instance.ParseInt(DateFile.instance.battleStateDate[key19][1]);
//Main.Logger.Log("flag");                                            int num113 = DateFile.instance.battlerStates[key18][num110][key19];
//Main.Logger.Log("flag");                                            text18 += string.Format("{0}{2}{3}{1}\n", DateFile.instance.SetColoer(10002, DateFile.instance.battleStateDate[key19][0]), DateFile.instance.SetColoer(10002, Mut()), (num112 > 0) ? DateFile.instance.SetColoer(20003, " (" + DateFile.instance.battleStateDate[num112][0] + ") ") : "", DateFile.instance.SetColoer((num113 > 100) ? 20005 : ((num113 < 100) ? 20010 : 20002), " (" + num113 + "%) "));
//Main.Logger.Log("flag");                                            for (int num114 = 0; num114 < list5.Count; num114 = num18 + 1)
//                                            {
//                                                int key20 = list5[num114];
//Main.Logger.Log("flag");                                                if (DateFile.instance.buffAttrDate.ContainsKey(key20))
//                                                {
//                                                    int num115 = DateFile.instance.ParseInt(DateFile.instance.battleStateDate[key19][key20]);
//Main.Logger.Log("flag");                                                    if (num115 != 0)
//                                                    {
//                                                        int num116 = num115 * num113 / 100;
//Main.Logger.Log("flag");                                                        string str5 = (DateFile.instance.ParseInt(DateFile.instance.buffAttrDate[key20][1]) == 1) ? Mathf.Abs(num116).ToString() : ((float)Mathf.Abs(num116) / DateFile.instance.ParseFloat(DateFile.instance.buffAttrDate[key20][1])).ToString((DateFile.instance.ParseInt(DateFile.instance.buffAttrDate[key20][1]) == 100) ? "f2" : "f1");
//Main.Logger.Log("flag");                                                        text18 += string.Format(" {0}{1}{2}\n", Dit(), DateFile.instance.buffAttrDate[key20][0], DateFile.instance.SetColoer((num116 > 0) ? DateFile.instance.ParseInt(DateFile.instance.buffAttrDate[key20][3]) : ((num116 < 0) ? DateFile.instance.ParseInt(DateFile.instance.buffAttrDate[key20][4]) : 20002), ((num116 < 0) ? "-" : "+") + str5 + DateFile.instance.buffAttrDate[key20][2]));
//Main.Logger.Log("flag");                                                    }
//                                                }
//                                                num18 = num114;
//Main.Logger.Log("flag");                                            }
//                                            text18 += "\n";
//Main.Logger.Log("flag");                                        }
//                                        num18 = num111;
//Main.Logger.Log("flag");                                    }
//                                    informationMassage.text = text18;
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "StoryFoodIcon":
//                                if (StorySystem.instance.useFoodId > 0)
//                                {
//                                    ShowItemMassage(StorySystem.instance.useFoodId, 1);
//Main.Logger.Log("flag");                                }
//                                else
//                                {
//                                    informationName.text = DateFile.instance.massageDate[8008][num2].Split('|')[0];
//Main.Logger.Log("flag");                                    informationMassage.text = DateFile.instance.massageDate[8008][num2].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                }
//                                break;
//Main.Logger.Log("flag");                            case "StoryItem":
//                                {
//                                    int num90 = DateFile.instance.ParseInt(tipsDate.name.Split(',')[1]);
//Main.Logger.Log("flag");                                    int num91 = StorySystem.instance.useItemId[num90];
//Main.Logger.Log("flag");                                    int id4 = DateFile.instance.ParseInt(DateFile.instance.baseStoryDate[StorySystem.instance.storySystemStoryId][201 + num90]);
//Main.Logger.Log("flag");                                    if (num91 > 0)
//                                    {
//                                        ShowItemMassage(num91, 1);
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        informationName.text = DateFile.instance.massageDate[3002][0] + DateFile.instance.GetItemDate(id4, 0, otherMassage: false);
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[3002][1] + "\n";
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "BaseSkillIcon":
//                                {
//                                    int key11 = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[1]);
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.baseSkillDate[key11][0];
//Main.Logger.Log("flag");                                    informationMassage.text = DateFile.instance.baseSkillDate[key11][99] + "\n";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "SkillIcon":
//                                {
//                                    int key10 = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[1]);
//Main.Logger.Log("flag");                                    int skillId = DateFile.instance.ParseInt(DateFile.instance.baseSkillDate[key10][1 + num2]);
//Main.Logger.Log("flag");                                    ShowGongFaMassage(skillId, 0);
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "BattleSkillIcon":
//                                ShowGongFaMassage(DateFile.instance.ParseInt(DateFile.instance.baseSkillDate[SkillBattleSystem.instance.battleSkillTyp][1 + num2]), 0);
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "StudySkillIcon":
//                                ShowGongFaMassage(num2, 0);
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "BaseGongFaIcon":
//                                {
//                                    int key5 = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[1]) + 101;
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.baseSkillDate[key5][0];
//Main.Logger.Log("flag");                                    informationMassage.text = DateFile.instance.baseSkillDate[key5][99] + "\n";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "GongFaIcon":
//                                ShowGongFaMassage(DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[1]), 1, -1, -1, tips.transform.parent.GetComponent<Toggle>());
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "GongFaTreeIcon":
//                                ShowGongFaMassage(DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[1]), 1, 0, 0);
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "EquipGongFaIcon":
//                                {
//                                    int num117 = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                    int num118 = DateFile.instance.GetActorEquipGongFa(num)[num2][num117];
//Main.Logger.Log("flag");                                    if (num118 > 0 || (num2 == 0 && num117 == 0))
//                                    {
//                                        ShowGongFaMassage(num118, 1);
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        int num119 = (num2 == 0) ? 2 : 0;
//Main.Logger.Log("flag");                                        informationName.text = DateFile.instance.massageDate[3012][num119];
//Main.Logger.Log("flag");                                        string text19 = "";
//Main.Logger.Log("flag");                                        if (BattleSystem.instance.battleWindow.activeSelf)
//                                        {
//                                            text19 = text19 + DateFile.instance.massageDate[3011][2] + "\n\n";
//Main.Logger.Log("flag");                                        }
//                                        informationMassage.text = $"{text19}{DateFile.instance.massageDate[3012][1 + num119]}\n";
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "BattleState":
//                                informationName.text = DateFile.instance.SetColoer(DateFile.instance.ParseInt(DateFile.instance.gongFaFPowerDate[num2][1]), DateFile.instance.gongFaFPowerDate[num2][0]);
//Main.Logger.Log("flag");                                informationMassage.text = $"{DateFile.instance.gongFaFPowerDate[num2][99]}{DateFile.instance.massageDate[5001][5]}\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "BattleGongFaTyp":
//                                informationName.text = DateFile.instance.actorAttrDate[num2][0];
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.actorAttrDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "BattlerMoveGongFa":
//                                {
//                                    int num35 = 0;
//Main.Logger.Log("flag");                                    int num36 = 50;
//Main.Logger.Log("flag");                                    if (num2 == 0)
//                                    {
//                                        int count = BattleVaule.instance.actorMoveCosts.Count;
//Main.Logger.Log("flag");                                        if (count > 0)
//                                        {
//                                            int num37 = 0;
//Main.Logger.Log("flag");                                            int num18;
//Main.Logger.Log("flag");                                            for (int num38 = 0; num38 < count; num38 = num18 + 1)
//                                            {
//                                                num37 += BattleVaule.instance.actorMoveCosts[num38][1];
//Main.Logger.Log("flag");                                                num18 = num38;
//Main.Logger.Log("flag");                                            }
//                                            num35 = BattleVaule.instance.actorMoveCosts[0][0];
//Main.Logger.Log("flag");                                            num36 = num37 / count;
//Main.Logger.Log("flag");                                        }
//                                    }
//                                    else
//                                    {
//                                        int count2 = BattleVaule.instance.enemyMoveCosts.Count;
//Main.Logger.Log("flag");                                        if (count2 > 0)
//                                        {
//                                            int num39 = 0;
//Main.Logger.Log("flag");                                            int num18;
//Main.Logger.Log("flag");                                            for (int num40 = 0; num40 < count2; num40 = num18 + 1)
//                                            {
//                                                num39 += BattleVaule.instance.enemyMoveCosts[num40][1];
//Main.Logger.Log("flag");                                                num18 = num40;
//Main.Logger.Log("flag");                                            }
//                                            num35 = BattleVaule.instance.enemyMoveCosts[0][0];
//Main.Logger.Log("flag");                                            num36 = num39 / count2;
//Main.Logger.Log("flag");                                        }
//                                    }
//                                    if (num35 != 0)
//                                    {
//                                        informationName.text = DateFile.instance.gongFaDate[num35][0];
//Main.Logger.Log("flag");                                        informationMassage.text = string.Format("{0}{1}\n\n{2}\n\n{3}\n", DateFile.instance.massageDate[5002][5], DateFile.instance.SetColoer((num36 > 100) ? 20004 : ((num36 == 100) ? 20003 : 20010), num36 + "%"), DateFile.instance.SetColoer(20002, DateFile.instance.gongFaDate[num35][99]), DateFile.instance.massageDate[5002][4].Split('|')[1]);
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        informationName.text = DateFile.instance.massageDate[5002][4].Split('|')[0];
//Main.Logger.Log("flag");                                        informationMassage.text = string.Format("{0}{1}\n\n{2}\n", DateFile.instance.massageDate[5002][5], DateFile.instance.SetColoer((num36 > 100) ? 20004 : ((num36 == 100) ? 20003 : 20010), num36 + "%"), DateFile.instance.massageDate[5002][4].Split('|')[1]);
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "BattlerDefGongFa":
//                                {
//                                    int key4 = (num2 == 0) ? BattleVaule.instance.actorDefGongFa[0] : BattleVaule.instance.enemyDefGongFa[0];
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.gongFaDate[key4][0];
//Main.Logger.Log("flag");                                    informationMassage.text = string.Format("{6}\n\n{0}{1}\n{2}{3}\n{4}{5}\n", Dit(), DateFile.instance.massageDate[5004][2].Split('|')[0], Dit(), DateFile.instance.massageDate[5004][2].Split('|')[1], Dit(), DateFile.instance.massageDate[5004][2].Split('|')[2], DateFile.instance.SetColoer(20002, DateFile.instance.gongFaDate[key4][99]));
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "ChooseAttackPart":
//                                informationName.text = ((num2 == 7) ? DateFile.instance.massageDate[1][5].Split('|')[0] : (DateFile.instance.massageDate[1][3] + DateFile.instance.SetColoer(20010, DateFile.instance.bodyInjuryDate[num2][0])));
//Main.Logger.Log("flag");                                informationMassage.text = ((num2 == 7) ? (DateFile.instance.massageDate[1][5].Split('|')[1] + "\n") : string.Format("{0}{1}\n\n{2}\n", DateFile.instance.massageDate[1][4], DateFile.instance.SetColoer(20003, DateFile.instance.ParseInt(DateFile.instance.bodyInjuryDate[num2][97]) + BattleSystem.instance.attackPartChooseObbs[num2] + "%"), DateFile.instance.bodyInjuryDate[num2][206]));
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "DamagePart":
//                                {
//                                    bool isActor3 = DateFile.instance.ParseInt(array[2]) == 0;
//Main.Logger.Log("flag");                                    int num125 = BattleSystem.instance.ActorId(isActor3, DateFile.instance.ParseInt(array[3]) == 1);
//Main.Logger.Log("flag");                                    int num126 = BattleVaule.instance.DamagePartValue(isActor3, num125, num2, upDamage: false, showEffect: false);
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.massageDate[5004][0].Split('|')[num2];
//Main.Logger.Log("flag");                                    informationMassage.text = string.Format("{0}{1}{2}\n\n{3} {4}\n{5}{6}{7} {8}\n", DateFile.instance.massageDate[5004][1].Split('|')[0], DateFile.instance.SetColoer(20010, BattleSystem.instance.battlerDamagePart[num125][num2].ToString()), DateFile.instance.massageDate[12][4], DateFile.instance.massageDate[5004][1].Split('|')[3], DateFile.instance.SetColoer(20010, num126 * 2 + "%"), DateFile.instance.massageDate[5004][1].Split('|')[1], DateFile.instance.buffAttrDate[DateFile.instance.ParseInt(DateFile.instance.bodyInjuryDate[Mathf.Min(num2, 7)][102])][0], DateFile.instance.massageDate[5004][1].Split('|')[2], DateFile.instance.SetColoer(20010, num126 + "%"));
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "QiDamagePart":
//                                {
//                                    bool isActor = DateFile.instance.ParseInt(array[2]) == 0;
//Main.Logger.Log("flag");                                    int num33 = BattleSystem.instance.ActorId(isActor, DateFile.instance.ParseInt(array[3]) == 1);
//Main.Logger.Log("flag");                                    int num34 = BattleVaule.instance.QiDamagePartValue(isActor, num33, num2, upDamage: false, showEffect: false);
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.massageDate[5004][3].Split('|')[num2];
//Main.Logger.Log("flag");                                    informationMassage.text = string.Format("{0}{1}{2}\n\n{3} {4}\n{5}{6}{7} {8}\n", DateFile.instance.massageDate[5004][4].Split('|')[0], DateFile.instance.SetColoer(20010, BattleSystem.instance.battlerQiDamagePart[num33][num2].ToString()), DateFile.instance.massageDate[12][4], DateFile.instance.massageDate[5004][4].Split('|')[3], DateFile.instance.SetColoer(20010, num34 * 2 + "%"), DateFile.instance.massageDate[5004][4].Split('|')[1], DateFile.instance.buffAttrDate[DateFile.instance.ParseInt(DateFile.instance.bodyInjuryDate[Mathf.Min(num2, 7)][101])][0], DateFile.instance.massageDate[5004][4].Split('|')[2], DateFile.instance.SetColoer(20010, num34 + "%"));
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "BattlerUseWeapon":
//                                UpdateEquipMassage(BattleSystem.instance.ActorId(DateFile.instance.ParseInt(array[1]) == 0, otherActor: false));
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "ActorInjury":
//                                {
//                                    int key14 = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[1]);
//Main.Logger.Log("flag");                                    if (DateFile.instance.ActorIsInBattle(ActorMenu.instance.acotrId) != 0)
//                                    {
//                                        if (DateFile.instance.battleActorsInjurys.ContainsKey(ActorMenu.instance.acotrId) && DateFile.instance.battleActorsInjurys[ActorMenu.instance.acotrId].ContainsKey(key14))
//                                        {
//                                            informationName.text = DateFile.instance.injuryDate[key14][0];
//Main.Logger.Log("flag");                                            string arg4 = DateFile.instance.massageDate[5003][0] + DateFile.instance.SetColoer(20010, ((float)DateFile.instance.battleActorsInjurys[ActorMenu.instance.acotrId][key14][0] / 10f).ToString("f1") + "%") + DateFile.instance.SetColoer(20008, $" ( {(float)DateFile.instance.battleActorsInjurys[ActorMenu.instance.acotrId][key14][1] / 10f:f1}%{DateFile.instance.massageDate[5003][1]})");
//Main.Logger.Log("flag");                                            informationMassage.text = $"{arg4}\n\n{DateFile.instance.injuryDate[key14][99]}\n";
//Main.Logger.Log("flag");                                        }
//                                        else
//                                        {
//                                            flag = false;
//Main.Logger.Log("flag");                                        }
//                                    }
//                                    else if (DateFile.instance.actorInjuryDate.ContainsKey(ActorMenu.instance.acotrId) && DateFile.instance.actorInjuryDate[ActorMenu.instance.acotrId].ContainsKey(key14))
//                                    {
//                                        informationName.text = DateFile.instance.injuryDate[key14][0];
//Main.Logger.Log("flag");                                        string arg5 = DateFile.instance.massageDate[5003][0] + DateFile.instance.SetColoer(20010, ((float)DateFile.instance.actorInjuryDate[ActorMenu.instance.acotrId][key14] / 10f).ToString("f1") + "%");
//Main.Logger.Log("flag");                                        informationMassage.text = $"{arg5}\n\n{DateFile.instance.injuryDate[key14][99]}\n";
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        flag = false;
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "StoryTerrain":
//                                informationName.text = DateFile.instance.storyTerrain[num2][0];
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.storyTerrain[num2][99] + "\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "TeamActor":
//                                {
//                                    tipsW = 120;
//Main.Logger.Log("flag");                                    tipsH = 20;
//Main.Logger.Log("flag");                                    int num81 = DateFile.instance.acotrTeamDate[num2];
//Main.Logger.Log("flag");                                    if (num81 >= 0)
//                                    {
//                                        informationName.text = DateFile.instance.GetActorName(num81);
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        informationName.text = DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[7][0].Split('|')[0]);
//Main.Logger.Log("flag");                                    }
//                                    informationMassage.text = "";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "PeopleActor":
//                                tipsW = 120;
//Main.Logger.Log("flag");                                tipsH = 20;
//Main.Logger.Log("flag");                                informationName.text = DateFile.instance.GetActorName(num2);
//Main.Logger.Log("flag");                                informationMassage.text = "";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "ShopBootyActor":
//                                informationName.text = DateFile.instance.GetActorName(num2);
//Main.Logger.Log("flag");                                informationMassage.text = $"{DateFile.instance.massageDate[7017][0].Split('|')[0]}{array[2]}{DateFile.instance.massageDate[7017][0].Split('|')[1]}\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "HomePlaceIcon":
//                                {
//                                    int key8 = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                    int key9 = DateFile.instance.ParseInt(array[3]);
//Main.Logger.Log("flag");                                    int[] array9 = DateFile.instance.homeBuildingsDate[num2][key8][key9];
//Main.Logger.Log("flag");                                    if (array9[0] > 0)
//                                    {
//                                        informationName.text = DateFile.instance.basehomePlaceDate[array9[0]][0];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.basehomePlaceDate[array9[0]][99] + "\n";
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        flag = false;
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "BattleTypToggle":
//                                informationName.text = DateFile.instance.battleTypDate[num2][0];
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.battleTypDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                if (StartBattle.instance.enemyTeamId == 2 || StartBattle.instance.enemyTeamId == 3 || StartBattle.instance.enemyTeamId == 4)
//                                {
//                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                    text2.text = text2.text + "\n" + DateFile.instance.massageDate[8009][3] + "\n";
//Main.Logger.Log("flag");                                }
//                                else if ((num2 == 1 || num2 == 101) && DateFile.instance.ParseInt(DateFile.instance.enemyTeamDate[StartBattle.instance.enemyTeamId][97]) != 0)
//                                {
//                                    Text text2 = informationMassage;
//Main.Logger.Log("flag");                                    text2.text = text2.text + "\n" + DateFile.instance.massageDate[8009][4] + "\n";
//Main.Logger.Log("flag");                                }
//                                break;
//Main.Logger.Log("flag");                            case "SetQiIcon":
//                                {
//                                    informationName.text = DateFile.instance.massageDate[5001][0].Split('|')[num2];
//Main.Logger.Log("flag");                                    string text21 = "";
//Main.Logger.Log("flag");                                    int num120 = num;
//Main.Logger.Log("flag");                                    int num121 = 0;
//Main.Logger.Log("flag");                                    if (array.Length > 2)
//                                    {
//                                        bool flag5 = DateFile.instance.ParseInt(array[2]) == 0;
//Main.Logger.Log("flag");                                        num120 = BattleSystem.instance.ActorId(flag5, otherActor: false);
//Main.Logger.Log("flag");                                        num121 = (flag5 ? BattleSystem.instance.actorGongFaSp[num120][num2] : BattleSystem.instance.enemyGongFaSp[num120][num2]);
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        if (BattleSystem.instance.battleWindow.activeSelf)
//                                        {
//                                            text21 = text21 + DateFile.instance.massageDate[3011][3] + "\n\n";
//Main.Logger.Log("flag");                                        }
//                                        num121 = DateFile.instance.GetMaxGongFaSp(num, num2);
//Main.Logger.Log("flag");                                    }
//                                    int num122 = BattleVaule.instance.QiPower(num120, num121, num2) - 100;
//Main.Logger.Log("flag");                                    int num123 = BattleVaule.instance.QiAddValue(num120, num121, num2);
//Main.Logger.Log("flag");                                    string text22 = num123.ToString();
//Main.Logger.Log("flag");                                    int num124 = num2;
//Main.Logger.Log("flag");                                    if (num124 == 1)
//                                    {
//                                        text22 = ((float)num123 / 100f).ToString();
//Main.Logger.Log("flag");                                    }
//                                    informationMassage.text = string.Format("{0}{1}\n\n{5} {6} {7}\n{2} {3} {4}\n", text21, DateFile.instance.massageDate[5001][1].Split('|')[num2], DateFile.instance.massageDate[5001][3].Split('|')[num2], DateFile.instance.SetColoer(20004, text22), DateFile.instance.massageDate[5001][4], DateFile.instance.massageDate[5001][2].Split('|')[num2], DateFile.instance.SetColoer(20004, num122 + "%"), DateFile.instance.massageDate[5001][4]);
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "RatedIcon":
//                                informationName.text = DateFile.instance.battleRatedDate[num2][0];
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.battleRatedDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "NewGameIcon":
//                                informationName.text = DateFile.instance.massageDate[7002][1].Split('|')[num2];
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.massageDate[7002][0].Split('|')[num2] + "\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "AbilityIcon":
//                                {
//                                    int num108 = DateFile.instance.ParseInt(DateFile.instance.abilityDate[num2][1]);
//Main.Logger.Log("flag");                                    int num109 = DateFile.instance.ParseInt(array[0]);
//Main.Logger.Log("flag");                                    bool flag3 = (num109 == 0 && NewGame.instance.GetAbilityP() >= DateFile.instance.ParseInt(DateFile.instance.abilityDate[num2][2])) || num109 == 1;
//Main.Logger.Log("flag");                                    bool flag4 = NewGame.instance.GetAbilityLevel(num108) >= DateFile.instance.ParseInt(DateFile.instance.abilityDate[num2][3]);
//Main.Logger.Log("flag");                                    int[] array12 = new int[3]
//                                    {
//                        20005,
//                        20008,
//                        20007
//                                    };
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.abilityDate[num2][0];
//Main.Logger.Log("flag");                                    string arg8 = DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[7001][4].Split('|')[0] + (flag3 ? DateFile.instance.abilityDate[num2][2] : DateFile.instance.SetColoer(20010, DateFile.instance.abilityDate[num2][2])));
//Main.Logger.Log("flag");                                    string arg9 = DateFile.instance.SetColoer(array12[num108], $"{DateFile.instance.massageDate[7001][3].Split('|')[num108]}{(flag4 ? DateFile.instance.abilityDate[num2][3] : DateFile.instance.SetColoer(20010, NewGame.instance.GetAbilityLevel(num108).ToString()))} / {DateFile.instance.ParseInt(DateFile.instance.abilityDate[num2][3])}");
//Main.Logger.Log("flag");                                    informationMassage.text = $"{arg8}\n{arg9}\n\n{DateFile.instance.abilityDate[num2][99]}\n";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "MianPlace":
//                                {
//                                    int mianPartId = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[2]);
//Main.Logger.Log("flag");                                    int minaPlaceId = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[3]);
//Main.Logger.Log("flag");                                    UpdateMianPlaceMassage(mianPartId, minaPlaceId);
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "WorkIcon":
//                                {
//                                    int num106 = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[3]);
//Main.Logger.Log("flag");                                    int num107 = DateFile.instance.worldMapWorkState[DateFile.instance.mianPartId][num106] - 1;
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.massageDate[151][0].Split('|')[num107];
//Main.Logger.Log("flag");                                    informationMassage.text = $"{DateFile.instance.massageDate[114][0].Split('|')[0]}{DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, num106, 98)}{DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, num106, 0)}{DateFile.instance.massageDate[114][0].Split('|')[1]}{DateFile.instance.GetNewMapDate(DateFile.instance.mianPartId, num106, 12)}{DateFile.instance.massageDate[114][0].Split('|')[2]}{DateFile.instance.massageDate[151][0].Split('|')[num107]}{DateFile.instance.massageDate[157][1]}{DateFile.instance.massageDate[151][1]}\n";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "StoryIcon":
//                                {
//                                    int key12 = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[3]);
//Main.Logger.Log("flag");                                    int key13 = DateFile.instance.worldMapState[DateFile.instance.mianPartId][key12][0];
//Main.Logger.Log("flag");                                    int num89 = DateFile.instance.worldMapState[DateFile.instance.mianPartId][key12][1];
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.baseStoryDate[key13][0];
//Main.Logger.Log("flag");                                    if (num89 == -1)
//                                    {
//                                        informationMassage.text = DateFile.instance.baseStoryDate[key13][99] + "\n";
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        informationMassage.text = $"{DateFile.instance.SetColoer(20005, num89.ToString())}{DateFile.instance.massageDate[3][0]}\n\n{DateFile.instance.baseStoryDate[key13][99]}\n";
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "HomeIcon":
//                                {
//                                    informationName.text = $"{DateFile.instance.allWorldDate[num2 - 1][0]}{DateFile.instance.placeWorldDate[num2][0]}\n{DateFile.instance.SetColoer(20008, DateFile.instance.placeWorldDate[num2 + 100][0])}";
//Main.Logger.Log("flag");                                    string text10 = "";
//Main.Logger.Log("flag");                                    string[] array10 = DateFile.instance.placeWorldDate[num2][99].Split('|');
//Main.Logger.Log("flag");                                    int num18;
//Main.Logger.Log("flag");                                    for (int num82 = 0; num82 < array10.Length; num82 = num18 + 1)
//                                    {
//                                        text10 = text10 + array10[num82] + "\n";
//Main.Logger.Log("flag");                                        num18 = num82;
//Main.Logger.Log("flag");                                    }
//                                    string text11 = "";
//Main.Logger.Log("flag");                                    string[] array11 = DateFile.instance.placeWorldDate[num2 + 100][99].Split('|');
//Main.Logger.Log("flag");                                    for (int num83 = 0; num83 < array11.Length; num83 = num18 + 1)
//                                    {
//                                        text11 = text11 + array11[num83] + "\n";
//Main.Logger.Log("flag");                                        num18 = num83;
//Main.Logger.Log("flag");                                    }
//                                    informationMassage.text = $"{text10}\n{text11}";
//Main.Logger.Log("flag");                                    break;
//Main.Logger.Log("flag");                                }
//                            case "HomeTypIcon":
//                                informationName.text = DateFile.instance.homeTypDate[num2][0];
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.homeTypDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "SkillMassage":
//                                {
//                                    tipsW = 220;
//Main.Logger.Log("flag");                                    int id3 = DateFile.instance.ParseInt(array[2]);
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.massageDate[7003][4].Split('|')[(num2 >= 100) ? (num2 - 100) : num2];
//Main.Logger.Log("flag");                                    informationMassage.text = "";
//Main.Logger.Log("flag");                                    if (num2 == 0 || num2 == 100)
//                                    {
//                                        int num18;
//Main.Logger.Log("flag");                                        for (int num46 = 0; num46 < 16; num46 = num18 + 1)
//                                        {
//                                            int num47 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(id3, 501 + num46, num2 == 0));
//Main.Logger.Log("flag");                                            Text text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += $"{Dit()}{DateFile.instance.SetColoer(20002 + Mathf.Clamp((num47 - 20) / 10, 0, 8), DateFile.instance.baseSkillDate[num46][0] + DateFile.instance.massageDate[7003][4].Split('|')[2] + Mut() + num47)}\n";
//Main.Logger.Log("flag");                                            num18 = num46;
//Main.Logger.Log("flag");                                        }
//                                    }
//                                    else
//                                    {
//                                        int num18;
//Main.Logger.Log("flag");                                        for (int num48 = 0; num48 < 14; num48 = num18 + 1)
//                                        {
//                                            int num49 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(id3, 601 + num48, num2 == 1));
//Main.Logger.Log("flag");                                            Text text2 = informationMassage;
//Main.Logger.Log("flag");                                            text2.text += $"{Dit()}{DateFile.instance.SetColoer(20002 + Mathf.Clamp((num49 - 20) / 10, 0, 8), DateFile.instance.baseSkillDate[num48 + 101][0] + DateFile.instance.massageDate[7003][4].Split('|')[2] + Mut() + num49)}\n";
//Main.Logger.Log("flag");                                            num18 = num48;
//Main.Logger.Log("flag");                                        }
//                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "StudyButton":
//                                {
//                                    int key6 = DateFile.instance.ParseInt(tips.transform.parent.name.Split(',')[1]);
//Main.Logger.Log("flag");                                    int key7 = DateFile.instance.actorStudyDate[key6][num2][1];
//Main.Logger.Log("flag");                                    switch (DateFile.instance.actorStudyDate[key6][num2][0])
//                                    {
//                                        case 1:
//                                            informationName.text = DateFile.instance.massageDate[7003][5].Split('|')[0];
//Main.Logger.Log("flag");                                            if (DateFile.instance.actorStudyDate[key6][num2][2] > 0)
//                                            {
//                                                informationMassage.text = DateFile.instance.SetColoer(20008, $"{DateFile.instance.baseSkillDate[key7][0]}{Mut()} +{DateFile.instance.actorStudyDate[key6][num2][2]}") + "\n\n";
//Main.Logger.Log("flag");                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.baseSkillDate[key7][99] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            else
//                                            {
//                                                informationMassage.text = DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[7003][5].Split('|')[1]);
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        case 2:
//                                            {
//                                                informationName.text = DateFile.instance.skillDate[key7][0];
//Main.Logger.Log("flag");                                                informationMassage.text = DateFile.instance.SetColoer(20008, $"{DateFile.instance.baseSkillDate[DateFile.instance.ParseInt(DateFile.instance.skillDate[key7][3])][0]}{DateFile.instance.massageDate[7003][4].Split('|')[2]}{Mut()} +{DateFile.instance.actorStudyDate[key6][num2][2]}") + "\n\n";
//Main.Logger.Log("flag");                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.skillDate[key7][99] + "\n";
//Main.Logger.Log("flag");                                                break;
//Main.Logger.Log("flag");                                            }
//                                        case 3:
//                                            {
//                                                informationName.text = DateFile.instance.gongFaDate[key7][0];
//Main.Logger.Log("flag");                                                informationMassage.text = DateFile.instance.SetColoer(20008, $"{DateFile.instance.baseSkillDate[DateFile.instance.ParseInt(DateFile.instance.gongFaDate[key7][61])][0]}{DateFile.instance.massageDate[7003][4].Split('|')[2]}{Mut()} +{DateFile.instance.actorStudyDate[key6][num2][2]}") + "\n\n";
//Main.Logger.Log("flag");                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + DateFile.instance.gongFaDate[key7][99] + "\n";
//Main.Logger.Log("flag");                                                break;
//Main.Logger.Log("flag");                                            }
//                                        default:
//                                            if (tips.transform.Find("StudySkillIcon," + num2).gameObject.activeSelf)
//                                            {
//                                                int num44 = ActorMenu.instance.ActorResource(DateFile.instance.MianActorID())[6];
//Main.Logger.Log("flag");                                                int num45 = HomeSystem.instance.StudyNeedPrestige(num2);
//Main.Logger.Log("flag");                                                informationName.text = DateFile.instance.massageDate[7004][3].Split('|')[0];
//Main.Logger.Log("flag");                                                informationMassage.text = ((num44 < num45) ? string.Format("{0}{1}{2}{3}{4}\n", DateFile.instance.massageDate[7005][2].Split('|')[0], DateFile.instance.SetColoer(20010, num44.ToString()), DateFile.instance.SetColoer(20007, " / " + num45.ToString()), DateFile.instance.massageDate[7005][2].Split('|')[1], DateFile.instance.massageDate[7004][3].Split('|')[1]) : string.Format("{0}{1}{2}{3}{4}\n", DateFile.instance.massageDate[7005][2].Split('|')[0], DateFile.instance.SetColoer(20004, num44.ToString()), DateFile.instance.SetColoer(20007, " / " + num45.ToString()), DateFile.instance.massageDate[7005][2].Split('|')[1], DateFile.instance.massageDate[7004][3].Split('|')[1]));
//Main.Logger.Log("flag");                                            }
//                                            else
//                                            {
//                                                informationName.text = DateFile.instance.massageDate[7004][4].Split('|')[0];
//Main.Logger.Log("flag");                                                informationMassage.text = DateFile.instance.massageDate[7004][4].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "StudyDiskIcon":
//                                {
//                                    int num20 = StudyWindow.instance.studyDiskId[num2][3];
//Main.Logger.Log("flag");                                    if (num20 > 0)
//                                    {
//                                        informationName.text = DateFile.instance.studyDiskDate[StudyWindow.instance.studyDiskId[num2][0]][0];
//Main.Logger.Log("flag");                                        if (StudyWindow.instance.studyDiskId[num2][0] == 0 || StudyWindow.instance.studyDiskId[num2][0] == 1)
//                                        {
//                                            Text text2 = informationName;
//Main.Logger.Log("flag");                                            text2.text += ((HomeSystem.instance.studySkillTyp == 17) ? ("\n" + DateFile.instance.SetColoer(20003, DateFile.instance.gongFaDate[HomeSystem.instance.levelUPSkillId][70 + StudyWindow.instance.studyDiskId[num2][0]])) : ("\n" + DateFile.instance.SetColoer(20003, DateFile.instance.skillDate[HomeSystem.instance.levelUPSkillId][12 + StudyWindow.instance.studyDiskId[num2][0]])));
//Main.Logger.Log("flag");                                        }
//                                        string str4 = "";
//Main.Logger.Log("flag");                                        switch (num20)
//                                        {
//                                            case 2:
//                                                str4 = DateFile.instance.SetColoer(20005, DateFile.instance.massageDate[7007][4].Split('|')[0]) + "\n\n";
//Main.Logger.Log("flag");                                                break;
//Main.Logger.Log("flag");                                            case 3:
//                                                str4 = DateFile.instance.SetColoer(20004, DateFile.instance.massageDate[7007][4].Split('|')[1]) + "\n\n";
//Main.Logger.Log("flag");                                                break;
//Main.Logger.Log("flag");                                            case 4:
//                                                str4 = DateFile.instance.SetColoer(20010, DateFile.instance.massageDate[7007][4].Split('|')[2]) + "\n\n";
//Main.Logger.Log("flag");                                                break;
//Main.Logger.Log("flag");                                        }
//                                        str4 += $"{DateFile.instance.SetColoer(20003, $"{DateFile.instance.massageDate[7007][4].Split('|')[3]}{StudyWindow.instance.GetStudyObbs(num2)}%")}\n{DateFile.instance.SetColoer(20004, $"{DateFile.instance.massageDate[7007][4].Split('|')[4]}+{StudyWindow.instance.GetStudyPower(num2)}")}\n\n";
//Main.Logger.Log("flag");                                        informationMassage.text = ((StudyWindow.instance.studyDiskId[num2][0] == 0 || StudyWindow.instance.studyDiskId[num2][0] == 1) ? (DateFile.instance.studyDiskDate[StudyWindow.instance.studyDiskId[num2][0]][99] + "\n") : $"{str4}{DateFile.instance.studyDiskDate[StudyWindow.instance.studyDiskId[num2][0]][99]}\n");
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        informationName.text = DateFile.instance.massageDate[7007][3].Split('|')[0];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[7007][3].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "BookPageIcon":
//                                if (HomeSystem.instance.readBookId > 0)
//                                {
//                                    informationName.text = $"{DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 0)}\n{DateFile.instance.massageDate[7009][2].Split('|')[0]}{DateFile.instance.massageDate[8][0].Split('|')[num2 + 1]}{DateFile.instance.massageDate[7009][2].Split('|')[1]}";
//Main.Logger.Log("flag");                                    int[] bookPage = DateFile.instance.GetBookPage(HomeSystem.instance.readBookId);
//Main.Logger.Log("flag");                                    int key17 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(HomeSystem.instance.readBookId, 32));
//Main.Logger.Log("flag");                                    informationMassage.text = ((bookPage[num2] == 0) ? (DateFile.instance.SetColoer(20010, DateFile.instance.massageDate[7009][4].Split('|')[0]) + "\n") : (DateFile.instance.SetColoer(20004, DateFile.instance.massageDate[7009][4].Split('|')[1]) + "\n"));
//Main.Logger.Log("flag");                                    if (HomeSystem.instance.studySkillTyp == 17)
//                                    {
//                                        int num104 = DateFile.instance.gongFaBookPages.ContainsKey(key17) ? DateFile.instance.gongFaBookPages[key17][num2] : 0;
//Main.Logger.Log("flag");                                        Text text2 = informationMassage;
//Main.Logger.Log("flag");                                        text2.text += ((num104 == 1 || num104 <= -100) ? (DateFile.instance.SetColoer(20005, DateFile.instance.massageDate[7009][4].Split('|')[3]) + "\n\n" + DateFile.instance.massageDate[7011][5] + DateFile.instance.SetColoer(20002, "100%\n")) : (DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[7009][4].Split('|')[2]) + "\n\n" + DateFile.instance.massageDate[7011][5] + DateFile.instance.SetColoer(20002, Mathf.Abs(num104) + "%\n")));
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        int num105 = DateFile.instance.skillBookPages.ContainsKey(key17) ? DateFile.instance.skillBookPages[key17][num2] : 0;
//Main.Logger.Log("flag");                                        Text text2 = informationMassage;
//Main.Logger.Log("flag");                                        text2.text += ((num105 == 1 || num105 <= -100) ? (DateFile.instance.SetColoer(20005, DateFile.instance.massageDate[7009][4].Split('|')[3]) + "\n\n" + DateFile.instance.massageDate[7011][5] + DateFile.instance.SetColoer(20002, "100%\n")) : (DateFile.instance.SetColoer(20003, DateFile.instance.massageDate[7009][4].Split('|')[2]) + "\n\n" + DateFile.instance.massageDate[7011][5] + DateFile.instance.SetColoer(20002, Mathf.Abs(num105) + "%\n")));
//Main.Logger.Log("flag");                                    }
//                                }
//                                else
//                                {
//                                    informationName.text = DateFile.instance.massageDate[7009][2].Split('|')[0] + DateFile.instance.massageDate[8][0].Split('|')[num2 + 1] + DateFile.instance.massageDate[7009][2].Split('|')[1];
//Main.Logger.Log("flag");                                    informationMassage.text = DateFile.instance.massageDate[7009][3] + "\n";
//Main.Logger.Log("flag");                                }
//                                break;
//Main.Logger.Log("flag");                            case "ReadState":
//                                informationName.text = DateFile.instance.readBookDate[num2][0];
//Main.Logger.Log("flag");                                informationMassage.text = DateFile.instance.readBookDate[num2][99] + "\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            case "MakeChooseItem":
//                                {
//                                    int num85 = (num2 == 0) ? MakeSystem.instance.mianItemId : MakeSystem.instance.secondItemId;
//Main.Logger.Log("flag");                                    if (num85 > 0)
//                                    {
//                                        ShowItemMassage(num85, 1);
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        informationName.text = DateFile.instance.massageDate[7013][0].Split('|')[0];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[7013][0].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "MakeThirdItem":
//                                {
//                                    int num84 = MakeSystem.instance.thirdItemId[num2];
//Main.Logger.Log("flag");                                    if (num84 > 0)
//                                    {
//                                        ShowItemMassage(num84, 1);
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        informationName.text = DateFile.instance.massageDate[7013][0].Split('|')[0];
//Main.Logger.Log("flag");                                        informationMassage.text = DateFile.instance.massageDate[7013][0].Split('|')[1] + "\n";
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "MakeItemTyoIcon":
//                                {
//                                    int num23 = (num2 > 0) ? DateFile.instance.homeBuildingsDate[HomeSystem.instance.homeMapPartId][HomeSystem.instance.homeMapPlaceId][HomeSystem.instance.homeMapbuildingIndex][9] : MakeSystem.instance.makeItemTyp;
//Main.Logger.Log("flag");                                    informationName.text = DateFile.instance.makeItemDate[num23][0];
//Main.Logger.Log("flag");                                    if (num2 > 0 || num23 <= 0 || MakeSystem.instance.secondItemId <= 0)
//                                    {
//                                        informationMassage.text = DateFile.instance.makeItemDate[num23][99] + "\n";
//Main.Logger.Log("flag");                                    }
//                                    else if (MakeSystem.instance.secondItemId == 5003 && num23 == 801)
//                                    {
//                                        informationMassage.text = string.Format("{0}\n{1}\n", string.Format("{0}\n\n{1}\n", DateFile.instance.massageDate[7013][5], DateFile.instance.SetColoer(20001 + DateFile.instance.ParseInt(DateFile.instance.GetItemDate(5004, 8)), "     ·" + DateFile.instance.GetItemDate(5004, 0, otherMassage: false))), DateFile.instance.makeItemDate[MakeSystem.instance.makeItemTyp][99]);
//Main.Logger.Log("flag");                                    }
//                                    else
//                                    {
//                                        int num24 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(MakeSystem.instance.secondItemId, 8));
//Main.Logger.Log("flag");                                        int key2 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(MakeSystem.instance.secondItemId, 48));
//Main.Logger.Log("flag");                                        int[] array5 = new int[3]
//                                        {
//                            MakeSystem.instance.GetItemNeedSkillValue(MakeSystem.instance.secondItemId, MakeSystem.instance.baseMakeTyp, HomeSystem.instance.homeMapPartId, HomeSystem.instance.homeMapPlaceId, HomeSystem.instance.homeMapbuildingIndex),
//                            0,
//                            0
//                                        };
//Main.Logger.Log("flag");                                        array5[1] = array5[0] + num24 * 20;
//Main.Logger.Log("flag");                                        array5[2] = array5[1] + (num24 + 1) * 20;
//Main.Logger.Log("flag");                                        if (num23 != 1501 && num23 != 1502)
//                                        {
//                                            num24 = ((num23 != 901) ? Mathf.Clamp(num24 - 2, 0, 6) : Mathf.Clamp(num24 / 2 - 1, 0, 3));
//Main.Logger.Log("flag");                                        }
//                                        else
//                                        {
//                                            num24 = Mathf.Max(Mathf.Min(MakeSystem.instance.GetSkillLevel(MakeSystem.instance.baseMakeTyp, num, addPower: true) / MakeSystem.instance.maxFoodLevel, 7) - num24, 0);
//Main.Logger.Log("flag");                                            int num25 = MakeSystem.instance.secondItemId + num24;
//Main.Logger.Log("flag");                                            int num26 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num25, 8));
//Main.Logger.Log("flag");                                            array5[0] = MakeSystem.instance.GetItemNeedSkillValue(num25, MakeSystem.instance.baseMakeTyp, HomeSystem.instance.homeMapPartId, HomeSystem.instance.homeMapPlaceId, HomeSystem.instance.homeMapbuildingIndex);
//Main.Logger.Log("flag");                                            array5[1] = array5[0] + num26 * 20;
//Main.Logger.Log("flag");                                            array5[2] = array5[1] + (num26 + 1) * 20;
//Main.Logger.Log("flag");                                        }
//                                        bool flag2 = false;
//Main.Logger.Log("flag");                                        int[] buildingNeighbor = HomeSystem.instance.GetBuildingNeighbor(HomeSystem.instance.homeMapPartId, HomeSystem.instance.homeMapPlaceId, HomeSystem.instance.homeMapbuildingIndex);
//Main.Logger.Log("flag");                                        int num18;
//Main.Logger.Log("flag");                                        for (int num27 = 0; num27 < buildingNeighbor.Length; num27 = num18 + 1)
//                                        {
//                                            int key3 = buildingNeighbor[num27];
//Main.Logger.Log("flag");                                            if (DateFile.instance.homeBuildingsDate[HomeSystem.instance.homeMapPartId][HomeSystem.instance.homeMapPlaceId].ContainsKey(key3))
//                                            {
//                                                int[] array6 = DateFile.instance.homeBuildingsDate[HomeSystem.instance.homeMapPartId][HomeSystem.instance.homeMapPlaceId][key3];
//Main.Logger.Log("flag");                                                int num28 = array6[0];
//Main.Logger.Log("flag");                                                if (num28 > 0 && DateFile.instance.ParseInt(DateFile.instance.basehomePlaceDate[num28][79]) == MakeSystem.instance.baseMakeTyp)
//                                                {
//                                                    flag2 = true;
//Main.Logger.Log("flag");                                                    break;
//Main.Logger.Log("flag");                                                }
//                                            }
//                                            num18 = num27;
//Main.Logger.Log("flag");                                        }
//                                        string[] array7 = DateFile.instance.makeItemDate[num23][key2].Split('|');
//Main.Logger.Log("flag");                                        string text4 = DateFile.instance.massageDate[7013][5] + "\n\n";
//Main.Logger.Log("flag");                                        int num29 = flag2 ? 3 : 2;
//Main.Logger.Log("flag");                                        int num30 = DateFile.instance.ParseInt(DateFile.instance.baseSkillDate[MakeSystem.instance.baseMakeTyp - 1][14]);
//Main.Logger.Log("flag");                                        for (int num31 = 0; num31 < num29; num31 = num18 + 1)
//                                        {
//                                            if (num31 > 0)
//                                            {
//                                                text4 = ((num30 <= 0) ? (text4 + $"\n{DateFile.instance.massageDate[658][3].Split('|')[0]} {DateFile.instance.SetColoer(20003, array5[num31].ToString())} {DateFile.instance.massageDate[658][3].Split('|')[1]}\n\n") : (text4 + $"\n{DateFile.instance.massageDate[658][4].Split('|')[0]} {DateFile.instance.SetColoer(20003, array5[num31].ToString())} {DateFile.instance.massageDate[658][4].Split('|')[1]} {DateFile.instance.SetColoer(20003, (num31 + 1).ToString())} {DateFile.instance.massageDate[658][4].Split('|')[2]}\n\n"));
//Main.Logger.Log("flag");                                            }
//                                            for (int num32 = 0; num32 < array7.Length; num32 = num18 + 1)
//                                            {
//                                                int id2 = DateFile.instance.ParseInt(array7[num32]) + num24 + num31;
//Main.Logger.Log("flag");                                                text4 = text4 + DateFile.instance.SetColoer(20001 + DateFile.instance.ParseInt(DateFile.instance.GetItemDate(id2, 8)), "     ·" + DateFile.instance.GetItemDate(id2, 0, otherMassage: false)) + "\n";
//Main.Logger.Log("flag");                                                num18 = num32;
//Main.Logger.Log("flag");                                            }
//                                            num18 = num31;
//Main.Logger.Log("flag");                                        }
//                                        informationMassage.text = $"{text4}\n{DateFile.instance.makeItemDate[MakeSystem.instance.makeItemTyp][99]}\n";
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "MassageChooseNeed":
//                                {
//                                    informationName.text = DateFile.instance.massageDate[8011][1].Split('|')[0];
//Main.Logger.Log("flag");                                    informationMassage.text = "";
//Main.Logger.Log("flag");                                    string a = DateFile.instance.eventDate[num2][6];
//Main.Logger.Log("flag");                                    if (a != "" && a != "0")
//                                    {
//                                        string[] array2 = DateFile.instance.eventDate[num2][6].Split('|');
//Main.Logger.Log("flag");                                        int num18;
//Main.Logger.Log("flag");                                        for (int num3 = 0; num3 < array2.Length; num3 = num18 + 1)
//                                        {
//                                            string text = "";
//Main.Logger.Log("flag");                                            string[] array3 = array2[num3].Split('#');
//Main.Logger.Log("flag");                                            for (int num4 = 0; num4 < array3.Length; num4 = num18 + 1)
//                                            {
//                                                if (num4 != 0)
//                                                {
//                                                    text += DateFile.instance.massageDate[8011][1].Split('|')[1];
//Main.Logger.Log("flag");                                                }
//                                                string[] array4 = array3[num4].Split('&');
//Main.Logger.Log("flag");                                                Main.Logger.Log("array4[0] " + array4[0]);
//Main.Logger.Log("flag");                                                switch (array4[0])
//                                                {
//                                                    case "ATTR":
//                                                        {
//                                                            int num8 = DateFile.instance.ParseInt(array4[1]);
//Main.Logger.Log("flag");                                                            int num9 = DateFile.instance.ParseInt(array4[2]);
//Main.Logger.Log("flag");                                                            int num10 = num8;
//Main.Logger.Log("flag");                                                            if (num10 == 16)
//                                                            {
//                                                                string str3 = "";
//Main.Logger.Log("flag");                                                                Main.Logger.Log("num9 " + num9);
//Main.Logger.Log("flag");                                                                switch (num9)
//                                                                {
//                                                                    case 0:
//                                                                        str3 = DateFile.instance.massageDate[9][0].Split('|')[2];
//Main.Logger.Log("flag");                                                                        break;
//Main.Logger.Log("flag");                                                                    case 1:
//                                                                        str3 = DateFile.instance.massageDate[9][0].Split('|')[1];
//Main.Logger.Log("flag");                                                                        break;
//Main.Logger.Log("flag");                                                                    case 251:
//                                                                        str3 = DateFile.instance.massageDate[9][0].Split('|')[0];
//Main.Logger.Log("flag");                                                                        break;
//Main.Logger.Log("flag");                                                                    case 501:
//                                                                        str3 = DateFile.instance.massageDate[9][0].Split('|')[3];
//Main.Logger.Log("flag");                                                                        break;
//Main.Logger.Log("flag");                                                                    case 751:
//                                                                        str3 = DateFile.instance.massageDate[9][0].Split('|')[4];
//Main.Logger.Log("flag");                                                                        break;
//Main.Logger.Log("flag");                                                                }
//                                                                text = text + DateFile.instance.massageDate[8011][2].Split('|')[0] + str3;
//Main.Logger.Log("flag");                                                            }
//                                                            break;
//Main.Logger.Log("flag");                                                        }
//                                                    case "ATTB":
//                                                        {
//                                                            int num11 = DateFile.instance.ParseInt(array4[1]);
//Main.Logger.Log("flag");                                                            int num12 = DateFile.instance.ParseInt(array4[2]);
//Main.Logger.Log("flag");                                                            int num13 = num11;
//Main.Logger.Log("flag");                                                            if (num13 == 14)
//                                                            {
//                                                                text += DateFile.instance.massageDate[8011][2].Split('|')[num12];
//Main.Logger.Log("flag");                                                            }
//                                                            break;
//Main.Logger.Log("flag");                                                        }
//                                                    case "OATTMAX":
//                                                        {
//                                                            int num16 = DateFile.instance.ParseInt(array4[1]);
//Main.Logger.Log("flag");                                                            int num17 = DateFile.instance.ParseInt(array4[2]);
//Main.Logger.Log("flag");                                                            Main.Logger.Log("num16 " + num16);
//Main.Logger.Log("flag");                                                            switch (num16)
//                                                            {
//                                                                case 401:
//                                                                    text += $"{DateFile.instance.massageDate[8011][5]}{DateFile.instance.massageDate[8011][2].Split('|')[17]}{DateFile.instance.SetColoer(20003, num17.ToString())}";
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 402:
//                                                                    text += $"{DateFile.instance.massageDate[8011][5]}{DateFile.instance.massageDate[8011][2].Split('|')[18]}{DateFile.instance.SetColoer(20003, num17.ToString())}";
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 403:
//                                                                    text += $"{DateFile.instance.massageDate[8011][5]}{DateFile.instance.massageDate[8011][2].Split('|')[19]}{DateFile.instance.SetColoer(20003, num17.ToString())}";
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 404:
//                                                                    text += $"{DateFile.instance.massageDate[8011][5]}{DateFile.instance.massageDate[8011][2].Split('|')[20]}{DateFile.instance.SetColoer(20003, num17.ToString())}";
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 405:
//                                                                    text += $"{DateFile.instance.massageDate[8011][5]}{DateFile.instance.massageDate[8011][2].Split('|')[21]}{DateFile.instance.SetColoer(20003, num17.ToString())}";
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 406:
//                                                                    text += $"{DateFile.instance.massageDate[8011][5]}{DateFile.instance.massageDate[8011][2].Split('|')[3]}{DateFile.instance.SetColoer(20008, num17.ToString())}";
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 407:
//                                                                    text += $"{DateFile.instance.massageDate[8011][5]}{DateFile.instance.massageDate[8011][2].Split('|')[4]}{DateFile.instance.SetColoer(20007, num17.ToString())}";
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                            }
//                                                            break;
//Main.Logger.Log("flag");                                                        }
//                                                    case "ATTMAX":
//                                                        {
//                                                            int num14 = DateFile.instance.ParseInt(array4[1]);
//Main.Logger.Log("flag");                                                            int num15 = DateFile.instance.ParseInt(array4[2]);
//Main.Logger.Log("flag");                                                            Main.Logger.Log("num14 " + num14);
//Main.Logger.Log("flag");                                                            switch (num14)
//                                                            {
//                                                                case 401:
//                                                                    text = text + DateFile.instance.massageDate[8011][2].Split('|')[17] + DateFile.instance.SetColoer(20003, num15.ToString());
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 402:
//                                                                    text = text + DateFile.instance.massageDate[8011][2].Split('|')[18] + DateFile.instance.SetColoer(20003, num15.ToString());
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 403:
//                                                                    text = text + DateFile.instance.massageDate[8011][2].Split('|')[19] + DateFile.instance.SetColoer(20003, num15.ToString());
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 404:
//                                                                    text = text + DateFile.instance.massageDate[8011][2].Split('|')[20] + DateFile.instance.SetColoer(20003, num15.ToString());
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 405:
//                                                                    text = text + DateFile.instance.massageDate[8011][2].Split('|')[21] + DateFile.instance.SetColoer(20003, num15.ToString());
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 406:
//                                                                    text = text + DateFile.instance.massageDate[8011][2].Split('|')[3] + DateFile.instance.SetColoer(20008, num15.ToString());
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                                case 407:
//                                                                    text = text + DateFile.instance.massageDate[8011][2].Split('|')[4] + DateFile.instance.SetColoer(20007, num15.ToString());
//Main.Logger.Log("flag");                                                                    break;
//Main.Logger.Log("flag");                                                            }
//                                                            break;
//Main.Logger.Log("flag");                                                        }
//                                                    case "NOGANG":
//                                                        text = text + DateFile.instance.massageDate[8010][4] + DateFile.instance.SetColoer(10002, DateFile.instance.GetGangDate(DateFile.instance.ParseInt(array4[1]), 0));
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "EFAME":
//                                                        {
//                                                            string str2 = "";
//Main.Logger.Log("flag");                                                            int num7 = DateFile.instance.ParseInt(array4[1]);
//Main.Logger.Log("flag");                                                            Main.Logger.Log("num7 " + num7);
//Main.Logger.Log("flag");                                                            if (num7 <= -75)
//                                                            {
//                                                                str2 = DateFile.instance.SetColoer(20010, DateFile.instance.massageDate[25][4].Split('|')[1]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num7 <= -50)
//                                                            {
//                                                                str2 = DateFile.instance.SetColoer(20007, DateFile.instance.massageDate[25][4].Split('|')[2]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num7 <= -25)
//                                                            {
//                                                                str2 = DateFile.instance.SetColoer(10005, DateFile.instance.massageDate[25][4].Split('|')[3]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num7 <= 0)
//                                                            {
//                                                                str2 = DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[25][4].Split('|')[0]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num7 <= 25)
//                                                            {
//                                                                str2 = DateFile.instance.SetColoer(20004, DateFile.instance.massageDate[25][4].Split('|')[6]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num7 <= 50)
//                                                            {
//                                                                str2 = DateFile.instance.SetColoer(20005, DateFile.instance.massageDate[25][4].Split('|')[5]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num7 <= 75)
//                                                            {
//                                                                str2 = DateFile.instance.SetColoer(20009, DateFile.instance.massageDate[25][4].Split('|')[4]);
//Main.Logger.Log("flag");                                                            }
//                                                            text = text + DateFile.instance.massageDate[8011][2].Split('|')[14] + str2;
//Main.Logger.Log("flag");                                                            break;
//Main.Logger.Log("flag");                                                        }
//                                                    case "FAME":
//                                                        {
//                                                            string str = "";
//Main.Logger.Log("flag");                                                            int num6 = DateFile.instance.ParseInt(array4[1]);
//Main.Logger.Log("flag");                                                            Main.Logger.Log("num6 " + num6);
//Main.Logger.Log("flag");                                                            if (num6 <= -75)
//                                                            {
//                                                                str = DateFile.instance.SetColoer(20010, DateFile.instance.massageDate[25][4].Split('|')[1]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num6 <= -50)
//                                                            {
//                                                                str = DateFile.instance.SetColoer(20007, DateFile.instance.massageDate[25][4].Split('|')[2]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num6 <= -25)
//                                                            {
//                                                                str = DateFile.instance.SetColoer(10005, DateFile.instance.massageDate[25][4].Split('|')[3]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num6 <= 0)
//                                                            {
//                                                                str = DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[25][4].Split('|')[0]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num6 <= 25)
//                                                            {
//                                                                str = DateFile.instance.SetColoer(20004, DateFile.instance.massageDate[25][4].Split('|')[6]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num6 <= 50)
//                                                            {
//                                                                str = DateFile.instance.SetColoer(20005, DateFile.instance.massageDate[25][4].Split('|')[5]);
//Main.Logger.Log("flag");                                                            }
//                                                            else if (num6 <= 75)
//                                                            {
//                                                                str = DateFile.instance.SetColoer(20009, DateFile.instance.massageDate[25][4].Split('|')[4]);
//Main.Logger.Log("flag");                                                            }
//                                                            text = text + DateFile.instance.massageDate[8011][2].Split('|')[13] + str;
//Main.Logger.Log("flag");                                                            break;
//Main.Logger.Log("flag");                                                        }
//                                                    case "AGV":
//                                                        {
//                                                            int num5 = DateFile.instance.ParseInt(array4[1]);
//Main.Logger.Log("flag");                                                            int id = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(MassageWindow.instance.eventMianActorId, 19, addValue: false));
//Main.Logger.Log("flag");                                                            text = text + DateFile.instance.SetColoer(10002, DateFile.instance.GetGangDate(id, 0)) + DateFile.instance.massageDate[8011][2].Split('|')[6] + DateFile.instance.SetColoer(20005, num5 / 10 + "%");
//Main.Logger.Log("flag");                                                            break;
//Main.Logger.Log("flag");                                                        }
//                                                    case "XXVALUE":
//                                                        text += DateFile.instance.massageDate[8011][2].Split('|')[22];
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "BGOOD":
//                                                        text += DateFile.instance.massageDate[8011][2].Split('|')[23];
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "GGOOD":
//                                                        text += DateFile.instance.massageDate[8011][2].Split('|')[24];
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "FA":
//                                                        text = text + DateFile.instance.massageDate[8011][2].Split('|')[7] + ActorMenu.instance.Color5(0,  true, DateFile.instance.ParseInt(array4[1]));
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "TIME":
//                                                        text = text + DateFile.instance.massageDate[8011][2].Split('|')[8] + DateFile.instance.SetColoer(20003, array4[1]);
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "MFS":
//                                                        text = text + DateFile.instance.massageDate[8011][2].Split('|')[9] + DateFile.instance.SetColoer(20003, DateFile.instance.GetMaxFamilySize().ToString());
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "BHS":
//                                                        text = text + DateFile.instance.massageDate[8011][2].Split('|')[10] + 9;
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "NOF":
//                                                        text += DateFile.instance.massageDate[8011][2].Split('|')[11];
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "SHOPV":
//                                                        text += DateFile.instance.massageDate[8011][2].Split('|')[12];
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "LIFEF":
//                                                        text += string.Format("{0}{1} {2}", DateFile.instance.massageDate[8011][2].Split('|')[15], DateFile.instance.SetColoer(10002, DateFile.instance.identityDate[DateFile.instance.ParseInt(array4[1])][0]), DateFile.instance.SetColoer(20005, "（" + DateFile.instance.ParseInt(array4[2]) + "%）"));
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                    case "OUTW":
//                                                        text += DateFile.instance.massageDate[8011][2].Split('|')[16];
//Main.Logger.Log("flag");                                                        break;
//Main.Logger.Log("flag");                                                }
//                                                num18 = num4;
//Main.Logger.Log("flag");                                            }
//                                            if (text != "")
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text += $"{Dit()}{text}{DateFile.instance.massageDate[157][1]}\n";
//Main.Logger.Log("flag");                                            }
//                                            num18 = num3;
//Main.Logger.Log("flag");                                        }
//                                    }
//                                    Main.Logger.Log("num2 " + num2);
//Main.Logger.Log("flag");                                    switch (num2)
//                                    {
//                                        case 900100001:
//                                            if (DateFile.instance.actorTalkFavor.ContainsKey(MassageWindow.instance.eventMianActorId) && DateFile.instance.actorTalkFavor[MassageWindow.instance.eventMianActorId].Count >= 1 && DateFile.instance.actorTalkFavor[MassageWindow.instance.eventMianActorId][0] != 0)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + "\n" + DateFile.instance.massageDate[8011][0] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        case 900200002:
//                                            if (DateFile.instance.actorTalkFavor.ContainsKey(MassageWindow.instance.eventMianActorId) && DateFile.instance.actorTalkFavor[MassageWindow.instance.eventMianActorId].Count >= 4 && DateFile.instance.actorTalkFavor[MassageWindow.instance.eventMianActorId][3] != 0)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + "\n" + DateFile.instance.massageDate[8011][0] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        case 900200006:
//                                            if (DateFile.instance.actorTalkFavor.ContainsKey(MassageWindow.instance.eventMianActorId) && DateFile.instance.actorTalkFavor[MassageWindow.instance.eventMianActorId].Count >= 3 && DateFile.instance.actorTalkFavor[MassageWindow.instance.eventMianActorId][2] != 0)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + "\n" + DateFile.instance.massageDate[8011][0] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                        case 900400002:
//                                            if (DateFile.instance.actorTalkFavor.ContainsKey(MassageWindow.instance.eventMianActorId) && DateFile.instance.actorTalkFavor[MassageWindow.instance.eventMianActorId].Count >= 3 && DateFile.instance.actorTalkFavor[MassageWindow.instance.eventMianActorId][2] != 0)
//                                            {
//                                                Text text2 = informationMassage;
//Main.Logger.Log("flag");                                                text2.text = text2.text + "\n" + DateFile.instance.massageDate[8011][0] + "\n";
//Main.Logger.Log("flag");                                            }
//                                            break;
//Main.Logger.Log("flag");                                    }
//                                    break;
//Main.Logger.Log("flag");                                }
//                            case "SkillBuildingIcon":
//                                informationName.text = $"{DateFile.instance.massageDate[8011][3]}\n{DateFile.instance.basehomePlaceDate[num2][0]}";
//Main.Logger.Log("flag");                                informationMassage.text = $"{DateFile.instance.massageDate[8011][4]}\n\n{DateFile.instance.SetColoer(20002, DateFile.instance.basehomePlaceDate[num2][99])}\n\n{DateFile.instance.SetColoer(20002, DateFile.instance.basehomePlaceDate[num2][100])}\n";
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                            default:
//                                Main.Logger.Log("default %%%%%%%%%%%");
//Main.Logger.Log("flag");                                flag = false;
//Main.Logger.Log("flag");                                break;
//Main.Logger.Log("flag");                        }
//                    }
//                    Main.Logger.Log("end  %%%%%%%%%%%  "  + flag);
//Main.Logger.Log("flag");                    anTips = flag;
//Main.Logger.Log("flag");                }

//                return false;
//            }
//        }

    }
}