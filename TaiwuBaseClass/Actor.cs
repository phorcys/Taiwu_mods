using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace TaiwuBaseClass
{
    public class Actor
    {
        private int _id;
        public int Id
        {
            get
            {
                return _id;
            }
            protected set
            {
                _id = value;
                SetId(_id);
            }
        }

        protected void SetId(int id)
        {
            _name.id = id;
            _kungfu.id = id;
            _skill.id = id;
            _status.id = id;
        }

        public struct PersonName
        {
            internal int id;
            public string First
            {
                get
                {
                    return DateFile.instance.actorsDate[id][29];
                }
                set
                {
                    DateFile.instance.actorsDate[id][29] = value;
                }
            }

            public string Last
            {
                get
                {
                    return DateFile.instance.actorsDate[id][2];
                }
                set
                {
                    DateFile.instance.actorsDate[id][2] = value;
                }
            }

            public PersonName(int id)
            {
                this.id = id;
            }
        }
        protected PersonName _name;

        public PersonName Name
        {
            get
            {
                return _name;
            }
        }

        public void SetName(string first, string last)
        {
            _name.First = first;
            _name.Last = last;
        }
        public int Age//年龄
        {
            get
            {
                return int.Parse(DateFile.instance.actorsDate[Id][11]);
            }
        }
        public int Health//健康
        {
            get
            {
                return ActorMenu.instance.Health(Id);
            }

        }
        public int Strength//膂力
        {
            get
            {
                return DateFile.instance.BaseAttr(Id, 0, 0);
            }
        }
        public int Constitution//体质
        {
            get
            {
                return DateFile.instance.BaseAttr(Id, 1, 0);
            }
        }
        public int Agility//灵敏
        {
            get
            {
                return DateFile.instance.BaseAttr(Id, 2, 0);
            }
        }
        public int Bone//根骨
        {
            get
            {
                return DateFile.instance.BaseAttr(Id, 3, 0);
            }
        }
        public int Intelligence//悟性
        {
            get
            {
                return DateFile.instance.BaseAttr(Id, 4, 0);
            }
        }
        public int Patience//定力
        {
            get
            {
                return DateFile.instance.BaseAttr(Id, 5, 0);
            }
        }

        public int Gender//性别
        {
            get
            {
                return int.Parse(DateFile.instance.GetActorDate(Id, 14, false));
            }
        }
        public int Charm//魅力
        {
            get
            {
                return int.Parse(DateFile.instance.GetActorDate(Id, 15, false));
            }
        }
        public struct Samsara
        {
            public int Count;
            public List<int> SamsaraList;
            public List<string> samNameList;

            public Samsara(int id)
            {
                SamsaraList = DateFile.instance.GetLifeDateList(id, 801, false);
                Count = SamsaraList.Count;
                samNameList = new List<string>();
                foreach (int i in SamsaraList)
                {
                    samNameList.Add(DateFile.instance.GetActorName(i));
                }
            }
        }

        public struct PersonKungfu//武学资质
        {
            internal int id;

            public int Force//内功
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 601, true));
                }
            }
            public int Dodge//身法
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 602, true));
                }
            }
            public int Stunt//绝技
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 603, true));
                }
            }
            public int Strike//掌法
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 604, true));
                }
            }
            public int Finger//指法
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 605, true));
                }
            }
            public int Kick//腿法
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 606, true));
                }
            }
            public int HidWeapon//暗器
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 607, true));
                }
            }
            public int Sword//剑法
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 608, true));
                }
            }
            public int Blade//刀法
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 609, true));
                }
            }
            public int Longstick//长兵
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 610, true));
                }
            }
            public int Qimen//奇门
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 611, true));
                }
            }
            public int Whip//软兵
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 612, true));
                }
            }
            public int Shoot//御射
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 613, true));
                }
            }
            public int Instrument//乐器
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 614, true));
                }
            }

            public PersonKungfu(int id)
            {
                this.id = id;
            }
        }

        private PersonKungfu _kungfu;

        public PersonKungfu Kungfu//武学资质
        {
            get
            {
                return _kungfu;
            }
        }

        public struct PersonSkill//技艺资质
        {
            internal int id;

            public int Music//音律
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 501, true));
                }
            }
            public int ChessArt//棋艺
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 502, true));
                }
            }
            public int Poem//诗书
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 503, true));
                }
            }
            public int Paint//绘画
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 504, true));
                }
            }
            public int Math//术数
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 505, true));
                }
            }
            public int Tasting//品鉴
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 506, true));
                }
            }
            public int Smith//锻造
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 507, true));
                }
            }
            public int Wood//制木
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 508, true));
                }
            }
            public int Medical//医术
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 509, true));
                }
            }
            public int Poison//毒术
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 510, true));
                }
            }
            public int Cloth//织锦
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 511, true));
                }
            }
            public int Jwelry//巧匠
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 512, true));
                }
            }
            public int Taoism//道法
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 513, true));
                }
            }
            public int Buddhism//佛学
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 514, true));
                }
            }
            public int Cooking//厨艺
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 515, true));
                }
            }
            public int MisArt//杂学
            {
                get
                {
                    return int.Parse(DateFile.instance.GetActorDate(id, 516, true));
                }
            }


            public PersonSkill(int id)
            {
                this.id = id;
            }
        }

        private PersonSkill _skill;

        public PersonSkill Skill
        {
            get
            {
                return _skill;
            }
        }

        public struct PersonStatus//人物状态
        {
            internal int id;
            public int MaxHp//外伤上限
            {
                get
                {
                    return ActorMenu.instance.MaxHp(id, 100);
                }
            }
            public int Hp//内伤
            {
                get
                {
                    return ActorMenu.instance.Hp(id, false);
                }
            }
            public int MaxSp//内伤上限
            {
                get
                {
                    return ActorMenu.instance.MaxSp(id, 100);
                }
            }
            public int Sp//外伤
            {
                get
                {
                    return ActorMenu.instance.Sp(id, false);
                }
            }
            public int[] ResPoisons//毒素抗性
            {
                get
                {
                    int[] arr = new int[6];
                    for (int i = 0; i < 6; i++)
                    {
                        arr[i] = int.Parse(DateFile.instance.actorsDate[id][42 + i]);
                    }
                    return arr;
                }
            }
            public List<int> Poisons//中毒
            {
                get
                {
                    List<int> list = new List<int> { };
                    for (int i = 0; i < 6; i++)
                    {
                        int num = int.Parse(DateFile.instance.GetActorDate(id, 51 + i, false));
                        list.Add(num);
                    }
                    return list;
                }
            }

            public PersonStatus(int id)
            {
                this.id = id;
            }
        }

        private PersonStatus _status;
        public PersonStatus Stauts
        {
            get
            {
                return _status;
            }
        }

        protected Actor()
        {
        }

        public static Actor fromId(int id)
        {
            if (!DateFile.instance.actorsDate.ContainsKey(id))
            {
                return null;
            }
            var actor = new Actor();
            actor.Id = id;
            return actor;
        }
    }

    public class GangMember : Actor
    {
        public int GangId
        {
            get
            {
                return int.Parse(DateFile.instance.GetActorDate(Id, 19, false));
            }
        }
        public string GangName
        {
            get
            {
                return DateFile.instance.GetGangDate(GangId, 0);
            }
        }
        public int GangLvl
        {
            get
            {
                int num1 = int.Parse(DateFile.instance.GetActorDate(Id, 19, false));
                int num2 = int.Parse(DateFile.instance.GetActorDate(Id, 20, false));
                int gangValueId = DateFile.instance.GetGangValueId(num1, num2);
                return gangValueId;
            }
        }
        public string GangLvlName
        {
            get
            {
                int num = int.Parse(DateFile.instance.GetActorDate(Id, 20, false));
                int key = (num >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(Id, 14, false)));
                string gang = DateFile.instance.presetGangGroupDateValue[GangLvl][key];
                return gang;
            }
        }

        protected GangMember()
        {
        }

        public static new GangMember fromId(int id)
        {
            GangMember member = (GangMember)Actor.fromId(id);
            return member;
        }
    }
    class MyClass
    {
        public void func()
        {
            GangMember member = GangMember.fromId(111);
        }
    }
}