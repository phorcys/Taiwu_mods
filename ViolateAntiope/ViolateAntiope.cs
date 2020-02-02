using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Xml.Serialization;


namespace ViolateAntiope
{
    public class Setting : UnityModManager.ModSettings
    {
        public bool specifiedPossibility = false;
        public int possibility = 10;
        public bool skipCheckFightAbility = false;
        public bool influenceMood = true;
        public bool causeHostility = true;
        public bool toldParents = false;
        public bool moreOptions = false;
        public bool nameFilter = false;
        public string nameStr = "";
        public bool lovedFilter = true;
        public bool sexFilter = false;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    /// <summary>
    ///  逐地块更新时拦截并修改结果
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "UpdateTileCharsBehavior")]
    public static class PeopleLifeAI_UpdateTileCharsBehavior_Patch
    {
        public static bool Prefix(int mapId, int tileId, bool isTaiwuAtThisTile, Dictionary<int, int> righteousInfo, object disasterInfo, int worldId, int mainActorId, Dictionary<int, List<int>> mainActorItems, System.Random random)
        {
            if (!Main.enabled) return true;
            if (!isTaiwuAtThisTile) return true;
            // int mainActorId = DateFile.instance.MianActorID();

            int 立场 = DateFile.instance.GetActorGoodness(mainActorId);
            int 概率 = int.Parse(DateFile.instance.goodnessDate[立场][25]);
            int 战力评价 = int.Parse(DateFile.instance.GetActorDate(mainActorId, 993, applyBonus: false));
            int 性别 = int.Parse(DateFile.instance.GetActorDate(mainActorId, 14, applyBonus: false));

            PeopleLifeAI_Utils.GetTileCharacters(mapId, tileId, out int[] aliveChars);
            List<int> targets = aliveChars.ToList<int>();
            if (Main.settings.lovedFilter) targets = targets.Where(id => DateFile.instance.GetActorSocial(mainActorId, 312).Contains(id)).ToList<int>();
            if (Main.settings.nameFilter) targets = targets.Where(id => (DateFile.instance.GetActorName(id).IndexOf(Main.settings.nameStr) != -1)).ToList<int>();
            if (Main.settings.sexFilter) targets = targets.Where(id => (int.Parse(DateFile.instance.GetActorDate(id, 14, applyBonus: false)) != 性别)).ToList<int>();

            Debug.Log("目标名单：" + (targets.Count == 0 ? "None" : (DateFile.instance.GetActorName(targets[0]) + " etc.")));

            

            if (Main.settings.specifiedPossibility) 概率 = Main.settings.possibility;

            if (targets.Count > 0 && UnityEngine.Random.Range(0, 100) < 概率)
            {
                int target = targets[UnityEngine.Random.Range(0, targets.Count)];
                // 如果打算欺辱打不过的人
                if (!Main.settings.skipCheckFightAbility && 战力评价 < int.Parse(DateFile.instance.GetActorDate(target, 993, applyBonus: false)) + 10000)
                {
                    Debug.Log("欺辱打不过的人");
                    // 被欺辱者与其势不两立
                    if (Main.settings.causeHostility) DateFile.instance.AddSocial(target, mainActorId, 401);
                    // 略微高兴？？
                    if(Main.settings.influenceMood) PeopleLifeAI_Utils.AiMoodChange(mainActorId, int.Parse(DateFile.instance.goodnessDate[DateFile.instance.GetActorGoodness(mainActorId)][102]));
                    // 设置历史记录 主动欺辱未成功
                    PeopleLifeAI_Utils.AISetMassage(99, mainActorId, mapId, tileId, new int[1], target);
                }
                // 欺辱成功
                else
                {
                    // 十分高兴？？！
                    if (Main.settings.influenceMood) PeopleLifeAI_Utils.AiMoodChange(mainActorId, int.Parse(DateFile.instance.goodnessDate[DateFile.instance.GetActorGoodness(mainActorId)][102]) * 10);
                    // 如若互相爱慕，则并无过多怨恨
                    if (DateFile.instance.GetActorSocial(target, 312).Contains(mainActorId))
                    {
                        Debug.Log("欺辱成功，但是并无过多怨恨");
                        // 被欺辱者情绪随机变化
                        if (Main.settings.influenceMood) PeopleLifeAI_Utils.AiMoodChange(target, UnityEngine.Random.Range(-10, 11));
                        if (UnityEngine.Random.Range(0, 100) < 50)
                        {
                            // 50% 产生 血海深仇
                            if (Main.settings.causeHostility) DateFile.instance.AddSocial(target, mainActorId, 402);
                        }
                        // 记录被欺辱但不难过的信息
                        PeopleLifeAI_Utils.AISetMassage(97, target, mapId, tileId, new int[1], mainActorId);
                    }
                    else
                    {
                        Debug.Log("欺辱成功，对方伤心欲绝");
                        // 被欺辱十分伤心
                        if (Main.settings.influenceMood) PeopleLifeAI_Utils.AiMoodChange(target, -50);
                        // 血海深仇
                        if (Main.settings.causeHostility) DateFile.instance.AddSocial(target, mainActorId, 402);
                        // 记录被欺辱十分伤心的信息
                        PeopleLifeAI_Utils.AISetMassage(96, target, mapId, tileId, new int[1], mainActorId);
                    }
                    // 性别不同则尝试怀孕？
                    if (性别 != int.Parse(DateFile.instance.GetActorDate(target, 14, applyBonus: false)))
                    {
                        Debug.Log("试图怀孕");
                        PeopleLifeAI_Utils.AISetChildren((性别 == 1) ? mainActorId : target,
                                                             (性别 == 1) ? target : mainActorId,
                                                             Main.settings.toldParents ? 1 : ((性别 != 1) ? 1 : 0),
                                                             Main.settings.toldParents ? 1 : ((性别 == 1) ? 1 : 0));
                        // 因为拿不到返回值所以放弃
                    }
                    else
                    {
                        Debug.Log("同性不能怀孕");
                        DateFile.instance.ChangeActorFeature(mainActorId, 4001, 4002);
                        DateFile.instance.ChangeActorFeature(target, 4001, 4002);
                        GEvent.OnEvent(eEvents.Copulate, mainActorId, target);
                    }
                }
                // PeopleLifeAI_Utils.AICantMove(target);
            }
            return true;
        }
    }

    public static class PeopleLifeAI_Utils
    {
        public static void AiMoodChange(int actorId, int value)
        {
            CallPrivateMethod<PeopleLifeAI>(PeopleLifeAI.instance, "AiMoodChange",
                new object[] { actorId, value});
        }

        public static void AICantMove(int actorId)
        {
            CallPrivateMethod<PeopleLifeAI>(PeopleLifeAI.instance, "AICantMove",
                new object[] { actorId});
        }

        public static void AISetEvent(int typ, int[] aiEventDate)
        {
            CallPrivateMethod<PeopleLifeAI>(PeopleLifeAI.instance, "AISetEvent",
                new object[] { typ, aiEventDate });
        }

        public static void AISetMassage(int massageId, int actorId, int partId, int placeId, int[] paramValues = null, int otherActorId = -1)
        {
            PeopleLifeAI.instance.AISetMassage(massageId, actorId, partId, placeId, paramValues, otherActorId);
            // Debug.Log((paramValues == null ? "null" : paramValues.ToString()) + (PeopleLifeAI.instance == null ? "null" : PeopleLifeAI.instance.ToString()));
            //CallPrivateMethod<PeopleLifeAI>(PeopleLifeAI.instance, "AISetMassage",
            //    new object[] { massageId, actorId, partId, placeId, paramValues, otherActorId});
        }

        public static void AISetChildren(int fatherId, int motherId, int setFather, int setMother)
        {
            PeopleLifeAI.instance.AISetChildren(fatherId, motherId, setFather, setMother);
            //CallPrivateMethod<PeopleLifeAI>(PeopleLifeAI.instance, "AISetChildren",
            //    new object[] { fatherId, motherId, setFather, setMother });
        }

        public static void GetTileCharacters(int mapId, int tileId, out int[] aliveChars)
        {
            HashSet<int> hashSet = new HashSet<int>();
            if (!DateFile.instance.doMapMoveing)
            {
                hashSet.UnionWith(PeopleLifeAI.instance.allFamily);
            }
            List<int> list = DateFile.instance.HaveActor(mapId, tileId, getNormal: true, getDieActor: true, getEnemy: true);
            foreach (int item in list)
            {
                if (int.Parse(DateFile.instance.GetActorDate(item, 8, applyBonus: false)) == 1)
                {
                    // 如果没死就加入
                    if(int.Parse(DateFile.instance.GetActorDate(item, 26, applyBonus: false)) == 0)
                    {
                        hashSet.Add(item);
                    }
                }
            }
            aliveChars = Enumerable.ToArray(hashSet);
        }

        public static T CallPrivateMethod<T>(this object instance, string name, params object[] param)
        {
            Debug.Log((instance == null ? "null" : instance.ToString()) + " -> " + (name == null ? "null" : name) + " -> " + (param == null ? "null" :  param.ToString()));
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = instance.GetType();
            Debug.Log((type == null ? "null type" : type.ToString()));
            MethodInfo method = type.GetMethod(name, flag);
            Debug.Log((method == null ? "null method" : method.ToString()));
            return (T)method.Invoke(instance, param);
        }
    }

    public static class ExpandUtils
    {
    }

    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Setting settings;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = Setting.Load<Setting>(modEntry);

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal("Box");
            settings.specifiedPossibility = GUILayout.Toggle(settings.specifiedPossibility, "指定概率");
            if (settings.specifiedPossibility)
            {
                var InputBox = GUILayout.TextField(settings.possibility.ToString(), 3);
                if (GUI.changed && !int.TryParse(InputBox, out settings.possibility))
                {
                    settings.possibility = 10;
                }
                GUILayout.Label("%");
            }
            settings.skipCheckFightAbility = GUILayout.Toggle(settings.skipCheckFightAbility, "无视战力直接成功");
            settings.influenceMood = GUILayout.Toggle(settings.influenceMood, "影响双方情绪");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.causeHostility = GUILayout.Toggle(settings.causeHostility, "导致对方仇恨");
            settings.toldParents = !GUILayout.Toggle(!settings.toldParents, "单亲孩子");
            settings.moreOptions = GUILayout.Toggle(settings.moreOptions, "显示定向欺辱选项");
            GUILayout.EndHorizontal();
            if(settings.moreOptions)
            { 
                GUILayout.BeginHorizontal("Box");
                settings.lovedFilter = GUILayout.Toggle(settings.lovedFilter, "只对自己爱慕的人下手");
                settings.sexFilter = GUILayout.Toggle(settings.sexFilter, "只对异性下手");
                settings.nameFilter = GUILayout.Toggle(settings.nameFilter, "只对指定名称的人下手（支持部分匹配）");
                GUILayout.Label("名称（部分）：", new GUILayoutOption[] { GUILayout.Width(120f) });
                settings.nameStr = GUILayout.TextField(settings.nameStr.ToString(), 6, new GUILayoutOption[] { GUILayout.Width(120f) });
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }

    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()

        {

            return null;

        }



        public void ReadXml(System.Xml.XmlReader reader)

        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();



            if (wasEmpty)

                return;



            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)

            {

                reader.ReadStartElement("item");



                reader.ReadStartElement("key");

                TKey key = (TKey)keySerializer.Deserialize(reader);

                reader.ReadEndElement();



                reader.ReadStartElement("value");

                TValue value = (TValue)valueSerializer.Deserialize(reader);

                reader.ReadEndElement();



                this.Add(key, value);



                reader.ReadEndElement();

                reader.MoveToContent();

            }

            reader.ReadEndElement();

        }



        public void WriteXml(System.Xml.XmlWriter writer)

        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            foreach (TKey key in this.Keys)

            {

                writer.WriteStartElement("item");



                writer.WriteStartElement("key");

                keySerializer.Serialize(writer, key);

                writer.WriteEndElement();



                writer.WriteStartElement("value");

                TValue value = this[key];

                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();



                writer.WriteEndElement();

            }

        }

        #endregion

    }
}