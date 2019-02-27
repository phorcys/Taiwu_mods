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
                actorUseGongfaTimes++;
            }
            else
            {
                if (BattleGongfaWeitht.ContainsKey(gongFaId))
                    BattleGongfaWeitht[gongFaId] += 1;
                else
                    BattleGongfaWeitht.Add(gongFaId, 2);
                enemyUseGongfaTimes++;
            }
        }


        public void AddBattleEnemy(int _newEnemyId, int _oldEnemyId)
        {
            if (!BattleEnemyIds.Contains(_newEnemyId))
                BattleEnemyIds.Add(_newEnemyId);

            if (!BattleEnemyIds.Contains(_oldEnemyId))
                BattleEnemyIds.Add(_oldEnemyId);
        }

        int enemyUseGongfaTimes = 0;
        int actorUseGongfaTimes = 0;

        string winTitle = "获得战斗领悟";
        string winMessage = "你在战斗中偷窥到了对方的心法，是否静心参悟？";
        public void SaveWindows()
        {
            Main.Logger.Log("弹出领悟功法窗口:");
            YesOrNoWindow.instance.SetYesOrNoWindow(1992062500, winTitle, winMessage, true, true);
            Button okbtn = YesOrNoWindow.instance.yesOrNoWindow.Find("YesButton").GetComponent<Button>();
            okbtn.onClick.AddListener(ClickYes);
            Button nobtn = YesOrNoWindow.instance.yesOrNoWindow.Find("NoButton").GetComponent<Button>();
            nobtn.onClick.AddListener(ClickNo);
        }

        private void ClickYes()
        {
            int id = OnClick.instance.ID;
            Main.Logger.Log("点击了确认按钮:" + id);
            if (id == 1992062500)
            {
                CalculationData();
                RemoveBind();
            }
        }

        private void ClickNo()
        {
            int id = OnClick.instance.ID;
            Main.Logger.Log("点击了取消按钮:" + id);
            if (id == 1992062500)
            {
                RemoveBind();
            }
        }

        private void RemoveBind()
        {
            Button okbtn = YesOrNoWindow.instance.yesOrNoWindow.Find("YesButton").GetComponent<Button>();
            okbtn.onClick.RemoveAllListeners();
            Button nobtn = YesOrNoWindow.instance.yesOrNoWindow.Find("NoButton").GetComponent<Button>();
            nobtn.onClick.RemoveAllListeners();

            BattleEnemyIds = null;
            BattleGongfaWeitht = null;
            enemyUseGongfaTimes = 0;
            actorUseGongfaTimes = 0;
            Main.Logger.Log("清空了数据");
        }

        private void CalculationData()
        {
            // 计算每个功法使用次数平均数
            int enemyAverage = enemyUseGongfaTimes / BattleGongfaWeitht.Count;

            // 此处加上装备的内功，身法，绝技
            foreach (int enemyId in BattleEnemyIds)
            {
                Dictionary<int, int[]> equipGongfas = new Dictionary<int, int[]>(DateFile.instance.GetActorEquipGongFa(enemyId));
                foreach (var GongFaIds in equipGongfas)
                {
                    // 0是内功 1是攻击类 2是身法类 3是护体类 4是绝技类
                    if (GongFaIds.Key != 1 && GongFaIds.Key != 2 && GongFaIds.Key != 3)
                        foreach (var gongFaId in GongFaIds.Value)
                        {
                            int times = (enemyAverage / 2 + 1);
                            for (int i = 0; i < times; i++)
                            {
                                AddUseGongfa(false, gongFaId);
                            }
                        }
                }
            }

            List<int> GongfaIds = new List<int>();
            foreach (var item in BattleGongfaWeitht)
            {
                // 此处可能应该排除掉已经大成的功法
                if (!GongFaIsMaxLevel(item.Key))
                {
                    for (int i = 0; i < item.Value; i++)
                    {
                        GongfaIds.Add(item.Key);
                    }
                }
            }
            // 随机得到的功法
            int getGongfaId = GongfaIds[Random.Range(0, GongfaIds.Count)];

            int addLvel = GongfaUpLevel(getGongfaId, 1);


            Main.Logger.Log("功法等级增加 " + addLvel);

        }

        private bool GongFaIsMaxLevel(int gongFaId)
        {
            //SortedDictionary<int, int[]> sortedDictionary = DateFile.instance.actorGongFas[gongFaId];
            //int gongFaLevel = sortedDictionary[gongFaId][0];
            return DateFile.instance.GetGongFaLevel(DateFile.instance.mianActorId, gongFaId) >= 10;
        }
        

        private int GongfaUpLevel(int gongFaId, int value)
        {
            int actorId = DateFile.instance.MianActorID();  // 角色id
            bool flag2 = !DateFile.instance.gongFaBookPages.ContainsKey(gongFaId); // 是否学会了技能
            if (flag2)
            {
                DateFile.instance.gongFaBookPages.Add(gongFaId, new int[10]);
            }
            int[] pages = new int[value];
            int idx = 0;
            for (int i = 0; i < 10; i++)
            {
                if (DateFile.instance.gongFaBookPages[gongFaId][i] == 0 && idx < value)
                {
                    pages[idx++] = i;
                }
            }

            foreach (var pageIndex in pages)
            {
                int num5 = DateFile.instance.gongFaBookPages[gongFaId][pageIndex];
                bool flag3 = num5 != 1 && num5 > -100;
                Main.Logger.Log("flag3功法" + gongFaId + " 书页" + pageIndex + "还没读吗" + flag3);
                if (flag3)// 
                {
                    int num6 = int.Parse(DateFile.instance.gongFaDate[gongFaId][2]);

                    bool flag4 = !DateFile.instance.actorGongFas[actorId].ContainsKey(gongFaId);
                    Main.Logger.Log("flag4 " + flag4);
                    if (flag4)
                    {
                        string name = DateFile.instance.massageDate[7010][3] + WindowManage.instance.Mut() + DateFile.instance.gongFaDate[gongFaId][0];
                        Main.Logger.Log("功法名称 " + name);
                    }
                    DateFile.instance.gongFaBookPages[gongFaId][pageIndex] = 1;
                    DateFile.instance.AddActorScore(303, num6 * 100);
                }
            }
            return idx;
        }
    }


    public class Test : UnityEngine.MonoBehaviour
    {

        void OnGUI()
        {
            if (UnityEngine.GUILayout.Button("xxxx"))
            {
                var gongfas = DateFile.instance.actorGongFas[DateFile.instance.MianActorID()];


                foreach (var item in gongfas)
                {
                    int gongFaId = item.Key; // 功法id
                    string s = "";
                    foreach (int v in item.Value) // [0]修习程度  [1]npc独有心法等级  [2]逆练页数？
                    {
                        s += " " + v.ToString();
                    }
                    Main.Logger.Log("玩家功法"+ gongFaId + s);

                    var gongfasdata = DateFile.instance.gongFaDate[gongFaId];
                    foreach (var data in gongfasdata)
                    {
                        Main.Logger.Log("功法数据"+data.Key + " " + data.Value);
                    }
                }
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


//逆练
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 10701
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 True
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 1
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 True
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 10701
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 1
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 True
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 10701
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 1
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 True
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 10701
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 1
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 True
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 True


//正练
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 True
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 False
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 0
//[GuiMartialArts]
//flag3 True
//[GuiMartialArts] num6 1
//[GuiMartialArts] num7 0
//[GuiMartialArts]
//flag4 False
//[GuiMartialArts] flag5 False
//[GuiMartialArts] flag6 False
//[GuiMartialArts] flag7 True
//[GuiMartialArts] num 10001
//[GuiMartialArts] 功法idnum3 150001
//[GuiMartialArts] 分数num4 60
//[GuiMartialArts]
//flag True
//[GuiMartialArts] 读的是功法书
//[GuiMartialArts] flag2 False
//[GuiMartialArts] num5 1
//[GuiMartialArts]
//flag3 False
