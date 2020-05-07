using UnityEngine;
using UnityEngine.UI;

namespace QuQuXRay
{
    public class UI : MonoBehaviour
    {
        public static UI Instance { get; private set; }
        public bool display = false;
        private GameObject mCanvas;
        int window_width = 420;
        int window_height = 420;
        int window_padding = 20;
        

        private void BlockGameUI(bool value)
        {
            if (value)
            {
                
                mCanvas = new GameObject("", typeof(Canvas), typeof(GraphicRaycaster));
                mCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                mCanvas.GetComponent<Canvas>().sortingOrder = short.MaxValue;
                DontDestroyOnLoad(mCanvas);
                var panel = new GameObject("", typeof(Image));
                panel.transform.SetParent(mCanvas.transform);
                panel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                panel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            }
            else
            {
                if(mCanvas != null)
                    Destroy(mCanvas);
            }
        }

        public static void ToggleShow()
        {
            Main.logger.Log("new");
            Instance.display = !Instance.display;
            //Instance.BlockGameUI(Instance.display);
        }
        private void Awake()
        {
            Main.logger.Log("awake");
            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void OnGUI()
        {
            if (display && Main.enabled)
            {
                DateFile tbl = DateFile.instance;
                if (tbl == null || !GameData.Characters.HasChar(tbl.MianActorID()) || GetQuquWindow.instance == null || GetQuquWindow.instance.cricketDate.Count == 0)
                {
                    return;
                }
                Rect rect = new Rect((Screen.width - window_width) / 2, (Screen.height - window_height) / 2, window_width, window_height);
                GUI.Window(952, rect, WindowFunction, "XRay");
                
            }
        }

        private void WindowFunction(int windowId)
        {

            int label_w = (window_width - window_padding) / 5;
            int label_h = (window_height - window_padding) / 5;
            //var style = new GUIStyle(GUI.skin.label);
            //style.fontSize = label_w;
            for (int i = 0; i < GetQuquWindow.instance.cricketDate.Count; i++)
            //for (int i = 0; i < 21; i++)
            {
                
                int x = 0, y = 0;
                if (i <= 2)
                {
                    x = (i + 1) * label_w + window_padding;
                    y = window_padding;
                }
                else if (i >= 18)
                {
                    x = (i - 18 + 1) * label_w + window_padding;
                    y = label_h * 4 + window_padding;
                    
                }
                else {
                    x = ((i - 3) % 5) * label_w + window_padding;
                    y = (((i - 3) / 5) + 1) * label_h + window_padding;
                }
                //Main.logger.Log(i + " : " + x + "," + y);
                
                string ququ_prefix = DateFile.instance.cricketDate[GetQuquWindow.instance.cricketDate[i][1]][0].Split('|')[0];
                string ququ_endfix = DateFile.instance.cricketDate[GetQuquWindow.instance.cricketDate[i][2]][0];
                ququ_prefix = DateFile.instance.SetColoer(int.Parse(DateFile.instance.cricketDate[GetQuquWindow.instance.cricketDate[i][1]][1]) + 20001, ququ_prefix);
                ququ_endfix = DateFile.instance.SetColoer(int.Parse(DateFile.instance.cricketDate[GetQuquWindow.instance.cricketDate[i][2]][1]) + 20001, ququ_endfix);
                GUI.Label(new Rect(x,y, label_w, label_h),ququ_prefix + "\n" + ququ_endfix);
            }
            
            //style.fontSize = lable_w;
            //for (int i = 0; i < 5; i++)
            //{
            //    for (int j = 0; j < 5; j++)
            //        GUI.Label(new Rect(j * lable_w, i * lable_h, lable_w, lable_h), j.ToString(), style);
            //}
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.F2) && Main.enabled)
            {
                ToggleShow();
            }
        }
    }
}