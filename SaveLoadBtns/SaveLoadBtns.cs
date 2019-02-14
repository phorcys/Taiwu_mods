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
        public override void Save(UnityModManager.ModEntry modEntry) { Save(this, modEntry); }
        public bool blockAutoSave = false;
    }
    public static class Main
    {
        public static bool enabled;
        public static bool forceSave = false;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
			GUILayout.BeginHorizontal("Box");
			GUILayout.Label("阻止游戏自动储存", GUILayout.Width(200));
            settings.blockAutoSave = GUILayout.SelectionGrid(settings.blockAutoSave?1:0, new string[] { "关闭", "启用" }, 2, GUILayout.Width(150)) ==1;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
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
                loadBtn.AddComponent<MyPointerClick>();

                GameObject saveBtn = Object.Instantiate(GameObject.Find("EncyclopediaButton,609"), new Vector3(1570f, -30f, 0), Quaternion.identity);
                saveBtn.name = "SaveButton";
                saveBtn.tag = "SystemIcon";
                saveBtn.transform.SetParent(parent, false);
                saveBtn.transform.localPosition = new Vector3(1570f, -30f, 0);
                Selectable saveButton = saveBtn.GetComponent<Selectable>();
                ((Image)saveButton.targetGraphic).sprite = Resources.Load<Sprite>("Graphics/Buttons/StartGameButton");
                saveBtn.AddComponent<MyPointerClick>();
            }
        }
    }


    public class MyPointerClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Main.enabled) return;
            if (gameObject.name == "LoadButton")
            {
                YesOrNoWindow.instance.SetYesOrNoWindow(4646, "快速载入", DateFile.instance.massageDate[701][2].Replace("返回主菜单", "载入旧进度").Replace("返回到游戏的主菜单…\n", ""), false, true);
            }
            else if (gameObject.name == "SaveButton")
            {
                Main.forceSave = true;
                SaveDateFile.instance.SaveGameDate();
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
                DateFile.instance.SetEvent(new int[] { 0, -1, 1001 }, true, true);
                DateFile.instance.Initialize(SaveDateFile.instance.dateId);
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

    [HarmonyPatch(typeof(SaveDateFile), "LateUpdate")]
    public class SaveDateFile_LateUpdate_Patch
    {
        [HarmonyBefore(new string[] { "SaveBackup" })]
        static void Prefix()
        {
            if (!Main.enabled || !Main.settings.blockAutoSave || UIDate.instance == null) return;
            SaveDateFile.instance.saveSaveDate = Main.forceSave;
			UIDate.instance.trunSaveText.text = "";
            Main.forceSave = false;
        }
    }

}
