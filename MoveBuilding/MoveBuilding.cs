using Harmony12;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace MoveBuilding
{
    public class Settings : UnityModManager.ModSettings
    {
        public int button = 1;
        public int language_index = 0;
        
        public readonly Dictionary<string, string>[] language_map_list = new Dictionary<string, string>[]
        {
            new Dictionary<string, string>()
            {
                {"right", "右键"},
                {"middle", "中键"},
                {"button_select_setting_text", "选择拖动使用的鼠标按键:"},
                {"english", "英语 (EN)"},
                {"chinese", "中文 (CH)"},
                {"language_select_setting_text", "选择语言:"}
            },
            new Dictionary<string, string>()
            {
                {"right", "Right"},
                {"middle", "Middle"},
                {"button_select_setting_text", "Building Drag Selection Button:"},
                {"english", "EN (英语)"},
                {"chinese", "CH (中文)"},
                {"language_select_setting_text", "Choose Language:"}
            }
        };

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
    public static class Main
    {
        public static bool enabled;

        public static Settings Settings { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger;
        public const string MOD_ID = "MoveBuildings";
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.Logger = modEntry.Logger;

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Main.Settings = Settings.Load<Settings>(modEntry);

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            var language_dicitonary = Settings.language_map_list[Settings.language_index];
            var button_select_setting_text = language_dicitonary["button_select_setting_text"];
            var button_select_list = new string[] { language_dicitonary["right"], language_dicitonary["middle"] };
            var language_select_setting_text = language_dicitonary["language_select_setting_text"];
            var language_select_list = new string[] { language_dicitonary["chinese"], language_dicitonary["english"] };
                
            GUILayout.BeginHorizontal("box");
            GUILayout.Label(button_select_setting_text);
            Settings.button = GUILayout.SelectionGrid(Settings.button - 1, button_select_list, 2) + 1;
            GUILayout.EndHorizontal();
                
            GUILayout.BeginHorizontal("box");
            GUILayout.Label(language_select_setting_text);
            Settings.language_index = GUILayout.SelectionGrid(Settings.language_index, language_select_list, 2);
            GUILayout.EndHorizontal();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }
    }

    [HarmonyPatch(typeof(HomeBuilding), "UpdateBuilding")]
    public static class HomeBuilding_UpdateBack_Patch
    {
        private static void Postfix(GameObject ___buildingButton)
        {
            if (!Main.enabled) return;
            string[] array = ___buildingButton.transform.parent.name.Split(',');
            if (array.Length < 4) return;
            int partId = int.Parse(array[1]);
            int placeId = int.Parse(array[2]);
            int buildingIndex = int.Parse(array[3]);
            int[] buildingType = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            if (buildingType[0] == 0 && ___buildingButton.GetComponent<DropablePlace>() == null) //空地
            {
                ___buildingButton.AddComponent<DropablePlace>();
            }
            //Modify By InpayH Start
            //else if (buildingType[0] >= 1002 && ___buildingButton.GetComponent<DraggingObject>() == null) // 人工建築
            // 判定：当前建筑不为太污村 且 该对象元素不为空
            else if (buildingType[0] != 1001 && ___buildingButton.GetComponent<DraggingObject>() == null)
            //Modify By InpayH End
            {
                ___buildingButton.AddComponent<DraggingObject>();
            }

        }
    }

    public class DraggingBuilding   // It is a singleton class
    {
        private DraggingBuilding() { }
        public static DraggingBuilding Instance
        {
            get { return Nested.instance; }
        }
        private class Nested
        {
            static Nested() { }
            internal static readonly DraggingBuilding instance = new DraggingBuilding();
        }
        public GameObject draggingObject = null;
        public GameObject dropablePlace = null;
        public Sprite placeBackSprite = null;
        public Sprite droppablePlaceSprite = null;
        public Image droppablePlaceImage = null;
        public bool isDragging = false;
    }

    public class DraggingObject : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Vector3 startPosition;
        private Transform originalParent;
        private Vector2 originalPosition;
        private Image placeBack;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Input.GetMouseButton(Main.Settings.button) && !DraggingBuilding.Instance.isDragging)
            {
                startPosition = transform.position;
                originalPosition = gameObject.GetComponent<RectTransform>().anchoredPosition;
                originalParent = transform.parent;

                placeBack = transform.parent.Find("PlaceImage").GetComponent<Image>();
                placeBack.color = new Color(1.000f, 1.000f, 1.000f, 0.500f);

                DraggingBuilding.Instance.draggingObject = gameObject;
                DraggingBuilding.Instance.placeBackSprite = placeBack.sprite;
                DraggingBuilding.Instance.isDragging = true;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!DraggingBuilding.Instance.isDragging) return;
            //Delete By InpayH Start
            //transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Delete By InpayH End
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!DraggingBuilding.Instance.isDragging) return;
            placeBack.color = new Color(1.000f, 1.000f, 1.000f, 1.000f);
            transform.SetParent(originalParent);
            transform.position = startPosition;
            gameObject.GetComponent<RectTransform>().anchoredPosition = originalPosition;

            if (DraggingBuilding.Instance.dropablePlace != null && DraggingBuilding.Instance.dropablePlace != DraggingBuilding.Instance.draggingObject)
            {
                // todo : 以後改版, 或會加入工作窗口, 要求搬家時像 升級建築般 花費人力或時間. 

                Main.Logger.Log("Move building from " + DraggingBuilding.Instance.draggingObject.transform.parent.name);
                Main.Logger.Log("To place " + DraggingBuilding.Instance.dropablePlace.transform.parent.name);
                //Helper.Functions.LogProperties(DraggingBuilding.Instance.draggingObject);
                //Helper.Functions.LogProperties(DraggingBuilding.Instance.dropablePlace);

                // interchange the content of two places
                string[] array = DraggingBuilding.Instance.draggingObject.transform.parent.name.Split(',');
                int partId = int.Parse(array[1]);
                int placeId = int.Parse(array[2]);
                int buildingIndex = int.Parse(array[3]);

                int buildingIndex2 = int.Parse(DraggingBuilding.Instance.dropablePlace.transform.parent.name.Split(',')[3]);

                int[] temp = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
                DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex] = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex2];
                DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex2] = temp;

                var workingDate = DateFile.instance.actorsWorkingDate;
                if (workingDate.ContainsKey(partId)
                    && workingDate[partId].ContainsKey(placeId)
                    && workingDate[partId][placeId].ContainsKey(buildingIndex))
                {
                    workingDate[partId][placeId][buildingIndex2] = workingDate[partId][placeId][buildingIndex];
                    workingDate[partId][placeId].Remove(buildingIndex);
                }

                Main.Logger.Log("Update Buildings");
                DraggingBuilding.Instance.droppablePlaceImage.color = new Color(1.000f, 1.000f, 1.000f, 1.000f);
                DestroyImmediate(DraggingBuilding.Instance.draggingObject.GetComponent<DraggingObject>());
                DestroyImmediate(DraggingBuilding.Instance.dropablePlace.GetComponent<DropablePlace>());
                HomeSystemWindow.Instance.allHomeBulding[buildingIndex].name = string.Format("HomeMapPlace,{0},{1},{2}", partId, placeId, buildingIndex);
                //HomeSystem.instance.allHomeBulding[buildingIndex].UpdateBack();
                HomeSystemWindow.Instance.UpdateHomePlace(partId, placeId, buildingIndex);
                HomeSystemWindow.Instance.allHomeBulding[buildingIndex2].name = string.Format("HomeMapPlace,{0},{1},{2}", partId, placeId, buildingIndex2);
                //HomeSystem.instance.allHomeBulding[buildingIndex2].UpdateBack();
                HomeSystemWindow.Instance.UpdateHomePlace(partId, placeId, buildingIndex2);

                //Helper.Functions.LogAllChild(HomeSystem.instance.homeMapHolder.gameObject);
                //Helper.Functions.LogProperties(DraggingBuilding.Instance.draggingObject);
                //Helper.Functions.LogProperties(DraggingBuilding.Instance.dropablePlace);
            }
            else
            {
                Main.Logger.Log("Cancel");
            }

            DraggingBuilding.Instance.draggingObject = null;
            DraggingBuilding.Instance.dropablePlace = null;
            DraggingBuilding.Instance.isDragging = false;

        }

    }

    public class DropablePlace : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        bool isEntered = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Helper.Functions.LogProperties(gameObject);
            if (DraggingBuilding.Instance.isDragging)
            {
                Main.Logger.Log("Enter dropablePlace: " + gameObject.transform.parent.gameObject.name);
                DraggingBuilding.Instance.dropablePlace = gameObject;
                DraggingBuilding.Instance.droppablePlaceImage = transform.parent.Find("PlaceImage").GetComponent<Image>();
                DraggingBuilding.Instance.droppablePlaceSprite = DraggingBuilding.Instance.droppablePlaceImage.sprite;
                DraggingBuilding.Instance.droppablePlaceImage.sprite = DraggingBuilding.Instance.placeBackSprite;
                DraggingBuilding.Instance.droppablePlaceImage.color = new Color(1.000f, 1.000f, 1.000f, 0.500f);
                isEntered = true;
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (isEntered && DraggingBuilding.Instance.dropablePlace == gameObject)
            {
                DraggingBuilding.Instance.dropablePlace = null;
                Main.Logger.Log("Leave dropablePlace : " + gameObject.transform.parent.gameObject.name);
                transform.parent.Find("PlaceImage").GetComponent<Image>().sprite = DraggingBuilding.Instance.droppablePlaceSprite;
                transform.parent.Find("PlaceImage").GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 1.000f);
                isEntered = false;
            }
        }
    }

}
