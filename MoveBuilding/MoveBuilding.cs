using Harmony12;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;

namespace MoveBuilding
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

    [HarmonyPatch(typeof(HomeBuilding), "UpdateBuilding")]
    public static class HomeBuilding_UpdateBack_Patch
    {
        private static void Postfix(GameObject ___buildingButton )
        {
            if (!Main.enabled) return;
            string[] array = ___buildingButton.transform.parent.name.Split(',');
            if (array.Length < 4) return;
            int partId = int.Parse(array[1]);
            int placeId = int.Parse(array[2]);
            int buildingIndex = int.Parse(array[3]);
            int[] buildingType = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            if(buildingType[0] == 0 && ___buildingButton.GetComponent<DropablePlace>() == null) //空地
            {
                ___buildingButton.AddComponent<DropablePlace>();
            }
            else if (buildingType[0] >= 1002 && ___buildingButton.GetComponent<DraggingObject>() == null) // 人工建築
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
        private GameObject buildingDummy;
        Vector3 originalScale;
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Input.GetMouseButton(1) && !DraggingBuilding.Instance.isDragging)
            {

                DraggingBuilding.Instance.draggingObject = gameObject;
                DraggingBuilding.Instance.isDragging = true;
                startPosition = transform.position;

                Image placeBack = transform.parent.Find("PlaceImage").GetComponent<Image>();
                Transform HomeViewPort = transform.parent.parent.parent.parent;

                // 這裏無法直接移動GameObject,每一格已被安排了前後次序.拉動建築時會有一半無反應. 現只能以複制圖片的方法進行
                buildingDummy = Instantiate(placeBack.gameObject, Vector3.zero, Quaternion.identity);
                buildingDummy.layer = LayerMask.NameToLayer("Ignore Raycast");
                buildingDummy.transform.localScale = new Vector3(.1f, .1f, 1); //這處未找到 zoom in / zoom out 的變數, 圖片大小或會有問題. 
                buildingDummy.transform.SetParent(HomeViewPort);
                buildingDummy.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                buildingDummy.transform.SetAsFirstSibling();


                originalScale = placeBack.transform.localScale; // 本想做成半透明或者全灰色, 但試許多方法都不成功. 望指點
                placeBack.transform.localScale = new Vector3(.5f, .5f, 1);

            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!DraggingBuilding.Instance.isDragging) return;
            buildingDummy.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!DraggingBuilding.Instance.isDragging) return;
            Image placeBack = transform.parent.Find("PlaceImage").GetComponent<Image>();
            Destroy(buildingDummy);
            transform.position = startPosition;

            placeBack.transform.localScale = originalScale;

            if (DraggingBuilding.Instance.dropablePlace != null && DraggingBuilding.Instance.dropablePlace != DraggingBuilding.Instance.draggingObject)
            {

                // todo : 以後改版, 或會加入工作窗口, 要求搬家時像 升級建築般 花費人力或時間. 

                Main.Logger.Log("Move building from " + DraggingBuilding.Instance.draggingObject.transform.parent.name);
                Main.Logger.Log("To place " + DraggingBuilding.Instance.dropablePlace.transform.parent.name);

                Destroy(DraggingBuilding.Instance.draggingObject.GetComponent<DraggingObject>());
                Destroy(DraggingBuilding.Instance.dropablePlace.GetComponent<DropablePlace>());

                // interchange the content of two places
                string[] array = DraggingBuilding.Instance.draggingObject.transform.parent.name.Split(',');
                int partId = int.Parse(array[1]);
                int placeId = int.Parse(array[2]);
                int buildingIndex = int.Parse(array[3]);

                int buildingIndex2 = int.Parse(DraggingBuilding.Instance.dropablePlace.transform.parent.name.Split(',')[3]);

                int[] temp = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
                DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex] = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex2];
                DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex2] = temp;

                if (DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(buildingIndex))
                {
                    DateFile.instance.actorsWorkingDate[partId][placeId][buildingIndex2] = DateFile.instance.actorsWorkingDate[partId][placeId][buildingIndex];
                    DateFile.instance.actorsWorkingDate[partId][placeId].Remove(buildingIndex);
                }

                HomeSystem.instance.MakeHomeMap(partId, placeId);
            }
            else
            {
                Main.Logger.Log("Cancel");
            }

            DraggingBuilding.Instance.draggingObject = null;
            DraggingBuilding.Instance.isDragging = false;

        }

    }

    public class DropablePlace : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        bool isEntered = false;
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (DraggingBuilding.Instance.isDragging)
            {
                Main.Logger.Log("dropablePlace: " + gameObject.transform.parent.gameObject.name);
                DraggingBuilding.Instance.dropablePlace = gameObject;
                isEntered = true;
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (isEntered && DraggingBuilding.Instance.dropablePlace == gameObject) {
                DraggingBuilding.Instance.dropablePlace = null;
                Main.Logger.Log("cancel dropablePlace : " + gameObject.transform.parent.gameObject.name);
                isEntered = false;
            }
        }
    }

}
