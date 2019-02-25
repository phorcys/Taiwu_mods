using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GuiMartialArts
{

    public class MartialArtsData
    {

        public MartialArtsData()
        {

        }


        /// <summary>
        /// 功法熟练度
        /// </summary>
        private Dictionary<int, int> gongfaProficiency;
        public Dictionary<int, int> GongfaProficiency
        {
            get
            {
                if (gongfaProficiency == null)
                    gongfaProficiency = new Dictionary<int, int>();
                return gongfaProficiency;
            }
            set
            {
                gongfaProficiency = value;
            }
        }

        private List<int> battleEnemyIds;
        public List<int> BattleEnemyIds
        {
            get
            {
                if (battleEnemyIds == null)
                    battleEnemyIds = new List<int>();
                return battleEnemyIds;
            }
            set
            {
                battleEnemyIds = value;
            }
        }
        private Dictionary<int, int> battleGongfaWeitht;
        public Dictionary<int, int> BattleGongfaWeitht
        {
            get
            {
                if (battleGongfaWeitht == null)
                    battleGongfaWeitht = new Dictionary<int, int>();
                return battleGongfaWeitht;
            }
            set
            {
                battleGongfaWeitht = value;
            }
        }


        public void AddUseGongfa(bool isActor, int gongFaId)
        {
            if (isActor)
            {
                if (GongfaProficiency.ContainsKey(gongFaId))
                    GongfaProficiency[gongFaId]++;
                else
                    GongfaProficiency.Add(gongFaId, 1);
            }
            else
            {
                if (BattleGongfaWeitht.ContainsKey(gongFaId))
                    BattleGongfaWeitht[gongFaId] += 1;
                else
                    BattleGongfaWeitht.Add(gongFaId, 2);
            }
        }


        string winTitle = "获得战斗领悟";
        string winMessage = "你在战斗中偷窥到了对方的心法，是否静心参悟？";
        public void SaveWindows()
        {
            Main.Logger.Log("弹出领悟功法窗口:");
            YesOrNoWindow.instance.SetYesOrNoWindow(1992062500, winTitle, winMessage, true, true);
            Button okbtn = YesOrNoWindow.instance.yesOrNoWindow.Find("YesButton").GetComponent<Button>();
            okbtn.onClick.AddListener(SaveData);
            Button nobtn = YesOrNoWindow.instance.yesOrNoWindow.Find("NoButton").GetComponent<Button>();
            nobtn.onClick.AddListener(DiscardData);
        }

        private void SaveData()
        {
            int id = OnClick.instance.ID;
            Main.Logger.Log("点击了确认按钮:" + id);
            if (id == 1992062500)
            {
                Main.Logger.Log("保存数据");
            }
            RemoveBind();
        }

        private void DiscardData()
        {
            int id = OnClick.instance.ID;
            Main.Logger.Log("点击了取消按钮:" + id);
            if (id == 1992062500)
            {
                Main.Logger.Log("丢弃数据");
            }
            RemoveBind();
        }

        private void RemoveBind()
        {
            Button okbtn = YesOrNoWindow.instance.yesOrNoWindow.Find("YesButton").GetComponent<Button>();
            okbtn.onClick.RemoveAllListeners();
            Button nobtn = YesOrNoWindow.instance.yesOrNoWindow.Find("NoButton").GetComponent<Button>();
            nobtn.onClick.RemoveAllListeners();
        }
    }


    public class Test : UnityEngine.MonoBehaviour
    {
        void Awake()
        {
            Main.Logger.Log("Awake" + this.ToString());

        }
        void Start()
        {
            Main.Logger.Log("Start" + this.ToString());

        }

        void OnGUI()
        {
            if (UnityEngine.GUILayout.Button("xxxx"))
            {
                //LogAllChild(YesOrNoWindow.instance.yesOrNoWindow, true);
                Main.artsData.SaveWindows();
            }
        }
        public void LogAllChild(Transform tf, bool logSize = false, int idx = 0)
        {
            string s = "";
            for (int i = 0; i < idx; i++)
            {
                s += "-- ";
            }
            s += tf.name + " " + tf.gameObject.activeSelf;
            if (logSize)
            {
                RectTransform rect = tf as RectTransform;
                if (rect == null)
                {
                    s += " scale=" + tf.localScale.ToString();
                }
                else
                {
                    s += " sizeDelta=" + rect.sizeDelta.ToString();
                }
            }
            Main.Logger.Log(s);

            idx++;
            for (int i = 0; i < tf.childCount; i++)
            {
                Transform child = tf.GetChild(i);
                LogAllChild(child, logSize, idx);
            }
        }
    }
}

//[GuiMartialArts] YesOrNoWindow True sizeDelta=(720.0, 280.0)
//[GuiMartialArts] -- YesOrNoTitle True sizeDelta=(280.0, 40.0)
//[GuiMartialArts] -- -- YesOrNoTitleText True sizeDelta=(0.0, 0.0)
//[GuiMartialArts] -- -- YesOrNoMassageText True sizeDelta=(580.0, 140.0)
//[GuiMartialArts] -- NoButton True sizeDelta=(60.0, 60.0)
//[GuiMartialArts] -- -- Image True sizeDelta=(0.0, 0.0)
//[GuiMartialArts] -- YesButton True sizeDelta=(90.0, 90.0)
//[GuiMartialArts] -- -- Image True sizeDelta=(0.0, 0.0)
