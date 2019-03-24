
using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using static GuiScroll.NewActorListScroll;

namespace GuiScroll
{
    public static class ActorMenuActorListPatch
    {
        public static IEnumeratorActorMenuOpend updateActorMenu;
        public static void Init(UnityModManager.ModEntry modEntry)
        {
            GameObject updateActorMenu = new GameObject();
            GameObject.DontDestroyOnLoad(updateActorMenu);
            ActorMenuActorListPatch.updateActorMenu = updateActorMenu.AddComponent<IEnumeratorActorMenuOpend>();

            //增加点击事件
            GameObject health = ActorMenu.instance.healthText.gameObject;
            Button btn = health.GetComponent<Button>();
            if (!btn)
            {
                btn = health.AddComponent<Button>();
            }
            var onclick = btn.onClick;
            onclick.RemoveAllListeners();
            onclick.AddListener(delegate {
                ActorMenuInjuryPatch.AddHealth();
            });
        }

        public static bool isShowActorMenu
        {
            get
            {
                var f_isShowActorMenu = typeof(ActorMenu).GetField("isShowActorMenu", BindingFlags.NonPublic | BindingFlags.Instance);
                return (bool)f_isShowActorMenu.GetValue(ActorMenu.instance);
            }
            set
            {
                var f_isShowActorMenu = typeof(ActorMenu).GetField("isShowActorMenu", BindingFlags.NonPublic | BindingFlags.Instance);
                f_isShowActorMenu.SetValue(ActorMenu.instance, value);
            }
        }
        public static List<int> showActors = new List<int>();

        public static GameObject closeButton
        {
            get { return ActorMenu.instance.closeButton; }
        }

        public static bool isEnemy
        {
            get { return ActorMenu.instance.isEnemy; }
            set { ActorMenu.instance.isEnemy = value; }
        }

        public static Toggle actorTeamToggle
        {
            get { return ActorMenu.instance.actorTeamToggle; }
        }

        public static GameObject actorMenu
        {
            get { return ActorMenu.instance.actorMenu; }
        }

        public static int acotrId
        {
            get { return ActorMenu.instance.acotrId; }
            set { ActorMenu.instance.acotrId = value; }
        }

        public static RectTransform listActorsHolder
        {
            get { return ActorMenu.instance.listActorsHolder; }
        }

        public static Toggle actorAttrToggle
        {
            get { return ActorMenu.instance.actorAttrToggle; }
        }

        public static Toggle actorEquipToggle
        {
            get { return ActorMenu.instance.actorEquipToggle; }
        }

        public static Toggle actorGongFaToggle
        {
            get { return ActorMenu.instance.actorGongFaToggle; }
        }

        public static Toggle actorPeopleToggle
        {
            get { return ActorMenu.instance.actorPeopleToggle; }
        }

        public static Toggle actorMassageToggle
        {
            get { return ActorMenu.instance.actorMassageToggle; }
        }

        public static Text familySizeText
        {
            get { return ActorMenu.instance.familySizeText; }
        }
        public static int giveActorId = 0;

        private static NewActorListScroll mm;
        public static NewActorListScroll m_listActorsHolder
        {
            get
            {
                // Main.Logger.Log("获取 NewActor");
                if (mm == null)
                {
                    InitGuiUI();
                }
                // Main.Logger.Log("获取 NewActor = " + mm.ToString());
                return mm;
            }
            set
            {
                // Main.Logger.Log("设置 NewActor mm");
                mm = value;
            }
        }
        private static void InitGuiUI()
        {
            // Main.Logger.Log("初始化 NewActor mm begin");
            ActorMenu.instance.listActorsHolder.gameObject.SetActive(false);
            mm = ActorMenu.instance.listActorsHolder.parent.parent.gameObject.AddComponent<NewActorListScroll>();
            mm.Init();
            // Main.Logger.Log("初始化 NewActor mm end");
        }



        // 显示角色菜单
        [HarmonyPatch(typeof(ActorMenu), "ShowActorMenu")]
        public static class ActorMenu_ShowActorMenu_Patch
        {

            public static bool Prefix(bool enemy)
            {
                if (!Main.enabled)
                    return true;

                if (m_listActorsHolder == null)
                {
                    InitGuiUI();
                }

                // Main.Logger.Log("显示角色菜单 ShowActorMenu " + enemy);

                if (!isShowActorMenu && !DateFile.instance.playerMoveing)
                {
                    closeButton.SetActive(!DateFile.instance.setNewMianActor);
                    if (DateFile.instance.battleStart && StartBattle.instance.enemyTeamId == 4)
                    {
                        Teaching.instance.RemoveTeachingWindow(0);
                        closeButton.SetActive(value: false);
                    }
                    isShowActorMenu = true;
                    showActors.Clear();
                    if (enemy)
                    {
                        if (DateFile.instance.battleStart)
                        {
                            for (int i = 0; i < DateFile.instance.enemyBattlerIdDate.Count; i++)
                            {
                                showActors.Add(DateFile.instance.enemyBattlerIdDate[i]);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < MassageWindow.instance.massageActors.Count; j++)
                            {
                                showActors.Add(MassageWindow.instance.massageActors[j]);
                            }
                        }
                    }
                    else if (DateFile.instance.battleStart)
                    {
                        for (int k = 0; k < DateFile.instance.actorBattlerIdDate.Count; k++)
                        {
                            int num = DateFile.instance.actorBattlerIdDate[k];
                            if (num > 0)
                            {
                                showActors.Add(DateFile.instance.actorBattlerIdDate[k]);
                            }
                        }
                        if (showActors[0] != DateFile.instance.MianActorID())
                        {
                            enemy = true;
                        }
                    }
                    else
                    {
                        List<int> list = new List<int>(DateFile.instance.GetFamily(getPrisoner: true));
                        for (int l = 0; l < list.Count; l++)
                        {
                            int num2 = list[l];
                            if (num2 > 0 && !DateFile.instance.enemyBattlerIdDate.Contains(num2))
                            {
                                showActors.Add(list[l]);
                            }
                        }
                    }
                    isEnemy = enemy;
                    actorTeamToggle.interactable = !enemy;
                    TipsWindow.instance.showTipsTime = -100;
                    if (StorySystem.instance.itemWindowIsShow)
                    {
                        StorySystem.instance.CloseItemWindow();
                    }
                    if (StorySystem.instance.toStoryIsShow)
                    {
                        StorySystem.instance.ClossToStoryMenu();
                    }
                    if (!DateFile.instance.battleStart)
                    {
                        UIMove.instance.CloseGUI();
                        //StartCoroutine(ActorMenuOpend(0.2f));
                        updateActorMenu.fun_times = 1;
                        updateActorMenu.down_time = 0.2f;
                    }
                    else
                    {
                        //StartCoroutine(ActorMenuOpend(0f));
                        updateActorMenu.fun_times = 1;
                        updateActorMenu.down_time = 0;
                    }
                }


                return false;
            }
        }


        // 显示角色
        [HarmonyPatch(typeof(ActorMenu), "SetActorList")]
        public static class ActorMenu_SetActorList_Patch
        {
            public static bool Prefix()
            {
                if (!Main.enabled && m_listActorsHolder != null)
                    return true;

                // Main.Logger.Log("显示角色 SetActorList");

                ActorMenu_RemoveActorList_Patch.Prefix();
                //for (int i = 0; i < showActors.Count; i++)
                //{
                //    int num = showActors[i];
                //    GameObject gameObject = UnityEngine.Object.Instantiate(listActor, Vector3.zero, Quaternion.identity);
                //    gameObject.name = "Actor," + num;
                //    gameObject.transform.SetParent(listActorsHolder, worldPositionStays: false);
                //    gameObject.GetComponent<Toggle>().group = listActorsHolder.GetComponent<ToggleGroup>();
                //    gameObject.GetComponent<SetListActor>().SetActor(num);
                //}
                //m_listActorsHolder.data = showActors.ToArray();

                m_listActorsHolder.scrollRect.verticalNormalizedPosition = 1;

                ActorMenu.instance.SortActorList();

                return false;
            }
        }

        // 排序角色列表
        [HarmonyPatch(typeof(ActorMenu), "SortActorList")]
        public static class ActorMenu_SortActorList_Patch
        {
            public static bool Prefix()
            {
                if (!Main.enabled)
                    return true;


                // Main.Logger.Log("排序角色列表 SortActorList");

                int idx = 0;
                List<int> list = showActors;
                int[] sort_result = new int[list.Count];
                if (!isEnemy)
                {
                    List<int> list2 = new List<int>();
                    List<int> list3 = new List<int>();
                    List<int> list4 = new List<int>();
                    List<int> list5 = new List<int>();
                    // Main.Logger.Log("开始分类 SortActorList");
                    for (int j = 0; j < list.Count; j++)
                    {
                        int actorId = list[j];
                        if (actorId == DateFile.instance.MianActorID())
                        {
                            sort_result[idx] = actorId;
                            idx++;
                        }
                        else if (DateFile.instance.acotrTeamDate.Contains(actorId))
                        {
                            list2.Add(actorId);
                        }
                        else if (DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 27, addValue: false)) == 1)
                        {
                            list4.Add(actorId);
                        }
                        else if (DateFile.instance.ActorIsWorking(actorId) != null)
                        {
                            list3.Add(actorId);
                        }
                        else
                        {
                            list5.Add(actorId);
                        }
                    }
                    // Main.Logger.Log("分类好了 SortActorList");
                    for (int k = 0; k < list2.Count; k++)
                    {
                        int num3 = list2[k];
                        sort_result[idx] = num3;
                        idx++;
                    }
                    for (int l = 0; l < list5.Count; l++)
                    {
                        int num4 = list5[l];
                        sort_result[idx] = num4;
                        idx++;
                    }
                    for (int m = 0; m < list4.Count; m++)
                    {
                        int num5 = list4[m];
                        sort_result[idx] = num5;
                        idx++;
                    }
                    for (int n = 0; n < list3.Count; n++)
                    {
                        int num6 = list3[n];
                        sort_result[idx] = num6;
                        idx++;
                    }
                    // Main.Logger.Log("排序好了 SortActorList");
                }
                else
                {
                    sort_result = list.ToArray();
                }
                // Main.Logger.Log("开始设置敌人 SortActorList");
                showActors = new List<int>(sort_result);

                // Main.Logger.Log("设置敌人好了 SortActorList");
                int num9 = isEnemy ? showActors.Count : DateFile.instance.GetFamily(getPrisoner: true).Count;
                // Main.Logger.Log("num9设置人数 SortActorList " + num9);
                familySizeText.text = DateFile.instance.SetColoer((num9 > DateFile.instance.GetMaxFamilySize()) ? 20010 : 20003, num9 + " / " + DateFile.instance.GetMaxFamilySize());


                return false;
            }
        }


        [HarmonyPatch(typeof(ActorMenu), "UpdateActorListFavor")]
        public static class ActorMenu_UpdateActorListFavort_Patch
        {
            public static bool Prefix()
            {
                if (!Main.enabled)
                    return true;

                // Main.Logger.Log("更新角色列表喜爱 UpdateActorListFavor");
                m_listActorsHolder.data = showActors.ToArray();


                return false;
            }
        }


        [HarmonyPatch(typeof(ActorMenu), "UpdateActorListFace")]
        public static class ActorMenu_UpdateActorListFace_Patch
        {
            public static bool Prefix()
            {
                if (!Main.enabled)
                    return true;

                // Main.Logger.Log("显示角色列表面容 UpdateActorListFace");
                m_listActorsHolder.data = showActors.ToArray();


                return false;
            }
        }


        [HarmonyPatch(typeof(ActorMenu), "RemoveActorList")]
        public static class ActorMenu_RemoveActorList_Patch
        {
            public static bool Prefix()
            {
                if (!Main.enabled)
                    return true;

                m_listActorsHolder.data = new int[0];


                // Main.Logger.Log("删除角色列表 RemoveActorList");


                return false;
            }
        }


        [HarmonyPatch(typeof(ActorMenu), "SetActorAttr")]
        public static class ActorMenu_SetActorAttr_Patch
        {
            public static bool Prefix(int key)
            {
                if (!Main.enabled)
                    return true;

                int key1 = acotrId;
                int key2 = giveActorId;
                bool selGive = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

                // Main.Logger.Log(selGive + " 设置角色属性 " + key);

                if (selGive)
                {
                    if (giveActorId == key)
                    {
                        // Main.Logger.Log(key + "=========相同赠送：" + giveActorId);
                        return false;
                    }
                    else
                        giveActorId = key;
                }
                if (!selGive)
                {
                    if (acotrId == key)
                    {
                        // Main.Logger.Log(key + "=========相同选中：" + acotrId);
                        return true;
                    }
                    else
                        acotrId = key;

                }
                bool result = false;
                var tf = m_listActorsHolder.rectContent;
                if (tf.childCount > 0)
                {
                    // 设置选中
                    string name = "Actor," + key;
                    var child = tf.Find(name);
                    if (child != null)
                    {
                        ActorItem item = child.GetComponentInChildren<ActorItem>();
                        ChildData childData = item.childDatas[0];
                        SelectState selectState;
                        if (selGive)
                        {
                            if (key == acotrId)
                            {
                                selectState = SelectState.All;
                            }
                            else
                            {
                                selectState = SelectState.Give;
                            }
                        }
                        else
                        {
                            if (key == giveActorId)
                            {
                                selectState = SelectState.All;
                            }
                            else
                            {
                                selectState = SelectState.Select;
                            }
                            result = true;
                        }
                        // Main.Logger.Log("设置选中：" + selectState);
                        childData.Select(selectState);
                    }
                    // 取消原来的赠送
                    if (selGive)
                    {
                        name = "Actor," + key2;
                        child = tf.Find(name);
                        if (child != null)
                        {
                            ActorItem item = child.GetComponentInChildren<ActorItem>();
                            ChildData childData = item.childDatas[0];
                            SelectState selectState;
                            if (key2 == acotrId)
                                selectState = SelectState.Select;
                            else
                                selectState = SelectState.Node;
                            // Main.Logger.Log("取消原来的赠送：" + selectState);
                            childData.Select(selectState);
                        }
                    }
                    else // 取消原来的选择
                    {
                        name = "Actor," + key1;
                        child = tf.Find(name);
                        if (child != null)
                        {
                            ActorItem item = child.GetComponentInChildren<ActorItem>();
                            ChildData childData = item.childDatas[0];
                            SelectState selectState;
                            if (key1 == giveActorId)
                                selectState = SelectState.Give;
                            else
                                selectState = SelectState.Node;
                            childData.Select(selectState);
                        }
                    }
                }
                // Main.Logger.Log("是否更新选中：" + result);
                // Main.Logger.Log("=========选中：" + acotrId);
                // Main.Logger.Log("=========赠送：" + giveActorId);
                return result;
            }
        }
    }
}