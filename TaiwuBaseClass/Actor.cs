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
        public int Id { get; private set; }
        public Tuple<string, string> Name { get; private set; }
        protected Actor()
        {
        }

        public static Actor fromId(int id)
        {
            var actor = new Actor();
            actor.Id = id;
            // actor.Name = ...
            return actor;
        }
    }
}