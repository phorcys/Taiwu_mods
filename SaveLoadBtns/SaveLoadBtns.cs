using Harmony12;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace SaveLoadBtns
{
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            return true;
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
