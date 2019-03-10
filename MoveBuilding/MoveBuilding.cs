using Harmony12;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace MoveBuilding
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);

        public int button = 1;
    }
    public static class Main
    {
        public static bool enabled;

        public static readonly string[] buttons = new string[] { "右键", "中键" };
        public static Settings Settings { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            Settings = Settings.Load<Settings>(modEntry);
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("选择拖动使用的鼠标按键:");
            Settings.button = GUILayout.SelectionGrid(Settings.button - 1, buttons, 2) + 1;
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
        public bool isDragging = false;
    }

    public class DraggingObject : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Vector3 startPosition;
        private Transform originalParent;
        private Vector2 originalPosition;
        private GameObject buildingDummy;
        private Image placeBack;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Input.GetMouseButton(Main.Settings.button) && !DraggingBuilding.Instance.isDragging)
            {

                DraggingBuilding.Instance.draggingObject = gameObject;
                DraggingBuilding.Instance.isDragging = true;
                startPosition = transform.position;
                originalPosition = gameObject.GetComponent<RectTransform>().anchoredPosition;
                originalParent = transform.parent;

                placeBack = transform.parent.Find("PlaceImage").GetComponent<Image>();
                placeBack.color = new Color(1.000f, 1.000f, 1.000f, 0.500f);
                Transform HomeViewport = HomeSystem.instance.homeMapHolder;

                buildingDummy = Instantiate(placeBack.gameObject, Vector3.zero, Quaternion.identity);
                buildingDummy.name = "buildingDummy";
                buildingDummy.GetComponent<Image>().color = new Color(1.000f, 1.000f, 1.000f, 0.500f);
                transform.SetParent(HomeViewport);
                buildingDummy.transform.SetParent(HomeViewport);
                buildingDummy.transform.localScale = transform.parent.parent.parent.localScale;
                buildingDummy.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                buildingDummy.transform.SetAsLastSibling();

            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!DraggingBuilding.Instance.isDragging) return;
            buildingDummy.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
            DestroyImmediate(buildingDummy);

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
                DestroyImmediate(DraggingBuilding.Instance.draggingObject.GetComponent<DraggingObject>());
                DestroyImmediate(DraggingBuilding.Instance.dropablePlace.GetComponent<DropablePlace>());
                HomeSystem.instance.allHomeBulding[buildingIndex].name = string.Format("HomeMapPlace,{0},{1},{2}", partId, placeId, buildingIndex);
                //HomeSystem.instance.allHomeBulding[buildingIndex].UpdateBack();
                HomeSystem.instance.UpdateHomePlace(partId, placeId, buildingIndex);
                HomeSystem.instance.allHomeBulding[buildingIndex2].name = string.Format("HomeMapPlace,{0},{1},{2}", partId, placeId, buildingIndex2);
                //HomeSystem.instance.allHomeBulding[buildingIndex2].UpdateBack();
                HomeSystem.instance.UpdateHomePlace(partId, placeId, buildingIndex2);

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
                isEntered = true;
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (isEntered && DraggingBuilding.Instance.dropablePlace == gameObject)
            {
                DraggingBuilding.Instance.dropablePlace = null;
                Main.Logger.Log("Leave dropablePlace : " + gameObject.transform.parent.gameObject.name);
                isEntered = false;
            }
        }
    }

}
