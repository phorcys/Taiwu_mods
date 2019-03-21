
using UnityEngine;

namespace GuiScroll
{
    public class IEnumeratorActorMenuOpend : MonoBehaviour
    {
        public float fun_times = 0;
        public float down_time = 0;
        private void Update()
        {
            if (fun_times == 1)
            {
                if (down_time > 0)
                {
                    down_time -= Time.deltaTime;
                }
                else
                {
                    fun_times++;
                    ActorMenuOpend1();
                    down_time = 0.02f;
                }
            }
            //else if(fun_times == 2)
            //{
            //    if (down_time > 0)
            //    {
            //        down_time -= Time.deltaTime;
            //    }
            //    else
            //    {
            //        fun_times++;
            //        if (ActorMenuActorListPatch.showActors.Count > 0)
            //        {
            //            int actorId = ActorMenuActorListPatch.showActors[0];
            //            ActorMenu.instance.SetActorAttr(actorId);
            //        }
            //    }
            //}
        }

        public void ActorMenuOpend1()
        {
            // Main.Logger.Log("角色菜单打开 111");

            TipsWindow.instance.showTipsTime = -100;
            YesOrNoWindow.instance.ShwoWindowMask(ActorMenuActorListPatch.actorMenu.transform, on: true);
            ActorMenuActorListPatch.acotrId = 0;
            ActorMenuActorListPatch.giveActorId = 0;
            ActorMenuActorListPatch.actorMenu.SetActive(true);
            ActorMenu.instance.SetActorTeam();
            ActorMenu.instance.SetActorList();
            //if (ActorMenuActorListPatch.listActorsHolder.childCount > 0)
            //{
            //    ActorMenuActorListPatch.listActorsHolder.GetChild(0).GetComponent<Toggle>().isOn = true;
            //}
            ActorMenuActorListPatch.m_listActorsHolder.data = ActorMenuActorListPatch.showActors.ToArray();
            // Main.Logger.Log("打开界面判断队伍人数是否大于0 " + ActorMenuActorListPatch.showActors.Count);
            //if (ActorMenuActorListPatch.showActors.Count > 1)
            //{
            //    ActorMenuActorListPatch.giveActorId = ActorMenuActorListPatch.showActors[1];
            //    ActorMenu.instance.SetActorAttr(ActorMenuActorListPatch.showActors[1]);
            //    ActorMenuActorListPatch.giveActorId = ActorMenuActorListPatch.showActors[0];
            //    ActorMenu.instance.SetActorAttr(ActorMenuActorListPatch.showActors[0]);
            //}
            //else 
            if (ActorMenuActorListPatch.showActors.Count > 0)
            {
                // Main.Logger.Log("默认选择第一个人 " + ActorMenuActorListPatch.showActors[0]);
                int actorId = ActorMenuActorListPatch.showActors[0];
                ActorMenuActorListPatch.giveActorId = actorId;
                ActorMenu.instance.SetActorAttr(actorId);

                //ActorMenu.instance.UpdateBaseAttr(actorId);
                //System.Type type = ActorMenu.instance.GetType();
                //System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                //var m = type.GetMethod("UpdateBaseAttr", flags);
                //object[] parameters = new object[] { actorId};
                //m.Invoke(ActorMenu.instance, parameters);
            }


            if (ActorMenuActorListPatch.isEnemy)
            {
                ActorMenuActorListPatch.actorAttrToggle.isOn = true;
            }
            ActorMenuActorListPatch.isShowActorMenu = false;
            if (DateFile.instance.teachingOpening > 0)
            {
                switch (DateFile.instance.teachingOpening)
                {
                    case 101:
                        if (!ActorMenuActorListPatch.actorEquipToggle.isOn)
                        {
                            Teaching.instance.SetTeachingWindow(2);
                        }
                        Teaching.instance.SetTeachingWindow(4);
                        break;
                    case 500:
                        if (!ActorMenuActorListPatch.actorGongFaToggle.isOn)
                        {
                            Teaching.instance.SetTeachingWindow(1);
                        }
                        Teaching.instance.SetTeachingWindow(2);
                        break;
                }
            }
            ActorMenuActorListPatch.actorPeopleToggle.interactable = (!ActorMenuActorListPatch.isEnemy || DateFile.instance.ParseInt(DateFile.instance.GetActorDate(ActorMenuActorListPatch.acotrId, 8, addValue: false)) == 1);
            ActorMenuActorListPatch.actorMassageToggle.interactable = (!ActorMenuActorListPatch.isEnemy || DateFile.instance.ParseInt(DateFile.instance.GetActorDate(ActorMenuActorListPatch.acotrId, 8, addValue: false)) == 1);
            if (!DateFile.instance.battleStart && !HomeSystem.instance.homeSystem.activeSelf && !StorySystem.instance.storySystem.activeSelf)
            {
                WorldMapSystem.instance.worldMapSystemHolder.gameObject.SetActive(false);
            }
            // Main.Logger.Log("角色菜单打开 444 " + ActorMenuActorListPatch.acotrId);
            ActorMenuActorListPatch.m_listActorsHolder.scrollRect.verticalNormalizedPosition = 1;
            ActorMenuActorListPatch.m_listActorsHolder.data = ActorMenuActorListPatch.showActors.ToArray();

            ActorMenuItemPackagePatch.ActorMenu_UpdateItems_Patch.Prefix(ActorMenuActorListPatch.showActors[0],ActorMenu.instance.itemTyp);


            if (ActorMenuActorListPatch.showActors.Count > 0)
            {
                int actorId = ActorMenuActorListPatch.showActors[0];
                ActorMenu.instance.SetActorAttr(actorId);
            }
        }
    }
}