using Harmony12;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace SaveLoadBtns
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool saveOnTurn = true;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
    public static class Main
    {
        public static bool enabled;
        public static bool manualSave;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            manualSave = false;
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            settings.saveOnTurn = GUILayout.Toggle(settings.saveOnTurn, "是否在时节切换时储存");
            GUILayout.EndHorizontal();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
    }

    //[HarmonyPatch(typeof(WorldMapSystem), "ShowChoosePlaceMenu")]
    //public static class WorldMapSystem_ShowChoosePlaceMenu_Patch
    //{
    //    public static void Postfix(int worldId, int partId, int placeId, Transform placeImage)
    //    { // todo: 只有大地图上,才可以SAVE.  这好像有点难,求助中
    //        if (Main.enabled && !DateFile.instance.playerMoveing)
    //            GameObject.Find("SaveButton").GetComponent<Selectable>().interactable = WorldMapSystem.instance.inToPlaceHomeButton.interactable;
    //        GameObject.Find("SaveButton").SetActive(false);
    //    }
    //}

    public class myPointerClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Main.enabled) return;
            if (gameObject.name == "LoadButton")
            {
                Main.Logger.Log("Loading data: " + SaveDateFile.instance.dateId.ToString());
                YesOrNoWindow.instance.SetYesOrNoWindow(4646, "快速载入", DateFile.instance.massageDate[701][2].Replace("返回主菜单", "载入旧进度").Replace("返回到游戏的主菜单…\n", ""), false, true);
            }
            else if (gameObject.name == "SaveButton")
            {
                Main.manualSave = true;
                SaveDateFile.instance.SaveGameDate();
            }
        }
    }

    [HarmonyPatch(typeof(WorldMapSystem), "Start")]
    public static class WorldMapSystem_Start_Patch
    {
        public static void Postfix()
        {
            if (Main.enabled)
            {
                Transform parent = GameObject.Find("ResourceBack").transform;

                GameObject loadBtn = Object.Instantiate(GameObject.Find("EncyclopediaButton,609"), new Vector3(1620f, -30f, 0), Quaternion.identity);
                loadBtn.name = "LoadButton";
                loadBtn.tag = "SystemIcon";
                loadBtn.transform.SetParent(parent, false);
                loadBtn.transform.localPosition = new Vector3(1620f, -30f, 0);
                Selectable loadButton = loadBtn.GetComponent<Selectable>();
                ((Image)loadButton.targetGraphic).sprite = Resources.Load<Sprite>("Graphics/Buttons/StartGameButton_NoColor");
                loadBtn.AddComponent<myPointerClick>();

                GameObject saveBtn = Object.Instantiate(GameObject.Find("EncyclopediaButton,609"), new Vector3(1570f, -30f, 0), Quaternion.identity);
                saveBtn.name = "SaveButton";
                saveBtn.tag = "SystemIcon";
                saveBtn.transform.SetParent(parent, false);
                saveBtn.transform.localPosition = new Vector3(1570f, -30f, 0);
                Selectable saveButton = saveBtn.GetComponent<Selectable>();
                ((Image)saveButton.targetGraphic).sprite = Resources.Load<Sprite>("Graphics/Buttons/StartGameButton");
                saveBtn.AddComponent<myPointerClick>();
            }
        }
    }

    [HarmonyPatch(typeof(SaveDateFile), "LateUpdate")]
    public class SaveDateFile_LateUpdate_Patch
    {
        static void Prefix(SaveDateFile __instance)
        {
            if (__instance.saveSaveDate)
            {
                Main.Logger.Log($"enabled: {Main.enabled}; manualSave: {Main.manualSave}");
                if (Main.enabled)
                {
                    if (Main.manualSave)
                    {
                        Main.manualSave = false;
                    }
                    else if (!Main.settings.saveOnTurn)
                    {
                        __instance.saveSaveDate = false;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(OnClick), "Index")]
    public static class OnClick_Index_Patch
    {
        public static void Postfix()
        {
            if (!Main.enabled) return;
            if (OnClick.instance.ID == 4646)
            {
                int saveID = SaveDateFile.instance.dateId;
                DateFile.instance.NewDate();
                DateFile.instance.LoadHomeBuildingDate(SaveDateFile.instance.LoadHomeBuildingDate(saveID));  // 此句乃游戏现版本的漏洞 才补上的. 
                Loading.instance.LoadingScene(3, saveID);
                YesOrNoWindow.instance.CloseYesOrNoWindow();
                YesOrNoWindow.instance.yesOrNoWindow.sizeDelta = new Vector2(720f, 280f);
                OnClick.instance.Over = true;
            }
        }
    }

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        [HarmonyBefore(new string[] { "CharacterFloatInfo" })]
        public static void Postfix(bool on, GameObject tips, ref Text ___informationMassage, ref Text ___informationName, ref int ___tipsW, ref bool ___anTips)
        {
            if (tips == null) return;
            if (!Main.enabled) return;
            if (tips.name == "SaveButton")
            {
                ___informationName.text = "储存";
                ___informationMassage.text = "立即储存现行进度\n";
                ___tipsW = 230;
                ___anTips = true;
            }
            else if (tips.name == "LoadButton")
            {
                ___informationName.text = "载入";
                ___informationMassage.text = "放弃现行进度, 由上一次记录重新开始\n";
                ___tipsW = 260;
                ___anTips = true;
            }
        }
    }
}
