using System.Collections.Generic;
using ConchShipGame.Event;
using UnityEngine;

namespace Ju.EventExtension
{
    public class EventExtentionHandle : IEventBase
    {   
        public void Init(EventMethodManager baseManager)
        {
            Debug.Log("初始化完成!");
            baseManager.RegisterMethod<EventHandler>("500001",2,KillPeopleAndTakeResource);
            baseManager.RegisterMethod<EventHandler>("500002",2,TakeResourceAndMakeChildren);
            baseManager.RegisterMethod<ChoiceFilter>("50000",2,AddFamaleChoice);
            baseManager.RegisterMethod<EventHandler>("500011",2, (d, e) =>
            {
                var actorId = d[3];
                DateFile.instance.ChangeFavor(actorId,-4000);
            });
            baseManager.RegisterMethod<EventHandler>("500012",2, (d, e) =>
            {
                var actorId = d[3];
                var mainActorId = DateFile.instance.mianActorId;
                DateFile.instance.ChangeFavor(actorId,-4000);
                //扩散敌对关系
                MessageEventManager.Instance.SetBadSocial(actorId, mainActorId, 402);
            });
            baseManager.RegisterMethod<ChoiceFilter>("9176",2,AddChoice);
        }

        [EventMethod("9001", 2, EventMethodType.ChoiceFilter)]
        public void AddChoice(int eventId,ref List<string> choice)
        {
            Main.Logger.Log($"{string.Join(",",choice)}");
            choice.Insert(choice.Count - 1,"900100004");
        }

        public void AddFamaleChoice(int eventId,ref List<string> choice)
        {
            Main.Logger.Log($"{string.Join(",",choice)}");
            var actorId = MessageEventManager.Instance.MainEventData[3];
            choice.Insert(choice.Count - 1,"500000002");
        }

        public void KillPeopleAndTakeResource(int[] eventData,int[] eventValue)
        {
            //取对话者的id
            var actorId = eventData[3];
            var mainActorId = DateFile.instance.mianActorId;
            for (int i = 0; i < 6; i++)
            {
                //资源交换
                UIDate.instance.ChangeTwoActorResource(actorId,mainActorId,i,DateFile.instance.GetActorDate(actorId,401+i,false).ParseInt());
            }
            DateFile.instance.RemoveActor(new List<int>(){actorId},true );
            //扩散敌对关系
            MessageEventManager.Instance.SetBadSocial(actorId, mainActorId, 402);
        }

        public void TakeResourceAndMakeChildren(int[] eventData,int[] eventValue)
        {
            //取对话者的id
            var actorId = eventData[3];
            var mainActorId = DateFile.instance.mianActorId;
            int actorGender = int.Parse(DateFile.instance.GetActorDate(actorId, 14, false)); //性别
            for (int i = 0; i < 6; i++)
            {
                //资源交换
                UIDate.instance.ChangeTwoActorResource(actorId,mainActorId,i,DateFile.instance.GetActorDate(actorId,401+i,false).ParseInt());
            }

            PeopleLifeAI.instance.AISetChildren(actorGender == 1 ? actorId : mainActorId,
                actorGender == 1 ? mainActorId : actorId, 1, 1);
        }
    }
}