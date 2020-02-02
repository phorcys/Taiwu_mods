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


namespace PrecisionShooting
{
    public class Setting : UnityModManager.ModSettings
    {
        public bool 指定生育可能性 = false;
        public int 生育可能性 = 7500;
        public bool 指定怀孕概率 = false;
        public int 怀孕概率 = 50;
        public bool 指定蛐蛐概率 = false;
        public int 蛐蛐概率 = 10;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    /// <summary>
    ///  逐地块更新时拦截并修改结果
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
    public static class PeopleLifeAI_AISetChildren_Patch
    {
        public static bool Prefix(int fatherId, int motherId, int setFather, int setMother, ref bool __result)
        {
            if (!Main.enabled) return true;

            int mainActorId = DateFile.instance.MianActorID();
            if (fatherId != mainActorId && motherId != mainActorId) return true;
            // int mainActorId = DateFile.instance.MianActorID();
            Debug.Log("PrecisionShooting...");
            int 父本生育参数 = int.Parse(DateFile.instance.GetActorDate(fatherId, 24));
            int 母本生育参数 = int.Parse(DateFile.instance.GetActorDate(motherId, 24));
            // 很容错的函数，不用动
            DateFile.instance.ChangeActorFeature(fatherId, 4001, 4002);
            DateFile.instance.ChangeActorFeature(motherId, 4001, 4002);
            GEvent.OnEvent(eEvents.Copulate, fatherId, motherId);
            if (Main.settings.指定生育可能性 ? (Main.settings.生育可能性 <= 0) : (父本生育参数 <= 0 || 母本生育参数 <= 0))
            {
                __result = false;
                return false;
            }

            // 不适合乱动的块
            if (int.Parse(DateFile.instance.GetActorDate(motherId, 14, applyBonus: false)) != 2)
            {
                __result = false;
                return false;
            }
            
            // 母亲未怀胎
            if (!DateFile.instance.HaveLifeDate(motherId, 901)
                && (UnityEngine.Random.Range(0, 15000) <
                     (Main.settings.指定生育可能性 ?
                        Main.settings.生育可能性 : 父本生育参数 * 母本生育参数)))
            {
                Debug.Log("生育可能性判定成功");
                bool isMainActor = fatherId == mainActorId || motherId == mainActorId;
                int 实际怀孕概率 = 100;
                int 子嗣加权 = isMainActor ? 20 : 50;
                // 主角能生五个，其他人两个

                // 减去孩子数量
                实际怀孕概率 -= DateFile.instance.GetActorSocial(fatherId, 310).Count * 子嗣加权;
                实际怀孕概率 -= DateFile.instance.GetActorSocial(motherId, 310).Count * 子嗣加权;
                
                if (UnityEngine.Random.Range(0, 100) < 
                    (Main.settings.指定怀孕概率 ? Main.settings.怀孕概率 : 实际怀孕概率))
                {
                    Debug.Log("怀孕判定成功");
                    DateFile.instance.ChangeActorFeature(motherId, 4002, 4003);
                    // 怀蛐蛐
                    if (UnityEngine.Random.Range(0, 100) <
                        (Main.settings.指定蛐蛐概率 ? Main.settings.蛐蛐概率 : ((DateFile.instance.getQuquTrun - 100) / 10)))
                    {
                        Debug.Log("异胎判定成功");
                        DateFile.instance.getQuquTrun = 0;
                        DateFile.instance.actorLife[motherId].Add(901, new List<int> { 1042,
                            fatherId, motherId, setFather, setMother });
                    }
                    // 正经怀孕
                    else
                    {
                        Debug.Log("常胎判定成功");
                        DateFile.instance.actorLife[motherId].Add(901, new List<int> {
                            UnityEngine.Random.Range(7, 10),
                            fatherId, motherId, setFather, setMother });
                        DateFile.instance.pregnantFeature.Add(motherId, new string[2] {
                            GameData.Characters.GetCharProperty(fatherId, 101),
                            GameData.Characters.GetCharProperty(motherId, 101)
                        });
                    }
                    __result = true;
                }
            }
            return false;
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
            settings.指定生育可能性 = GUILayout.Toggle(settings.指定生育可能性, "指定生育可能性");
            if (settings.指定生育可能性)
            {
                var InputBox = GUILayout.TextField(settings.生育可能性.ToString(), 5);
                if (GUI.changed && !int.TryParse(InputBox, out settings.生育可能性))
                {
                    settings.生育可能性 = 7500;
                }
                GUILayout.Label("/15000");
            }
            settings.指定怀孕概率 = GUILayout.Toggle(settings.指定怀孕概率, "指定怀孕概率");
            if (settings.指定怀孕概率)
            {
                var InputBox = GUILayout.TextField(settings.怀孕概率.ToString(), 3);
                if (GUI.changed && !int.TryParse(InputBox, out settings.怀孕概率))
                {
                    settings.怀孕概率 = 50;
                }
                GUILayout.Label("%");
            }
            settings.指定蛐蛐概率 = GUILayout.Toggle(settings.指定蛐蛐概率, "指定蛐蛐概率");
            if (settings.指定蛐蛐概率)
            {
                var InputBox = GUILayout.TextField(settings.蛐蛐概率.ToString(), 3);
                if (GUI.changed && !int.TryParse(InputBox, out settings.蛐蛐概率))
                {
                    settings.蛐蛐概率 = 10;
                }
                GUILayout.Label("%");
            }
            GUILayout.EndHorizontal();
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