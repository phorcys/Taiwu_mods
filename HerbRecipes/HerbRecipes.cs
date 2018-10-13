using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using System.Reflection.Emit;

namespace HerbRecipes
{
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            //注册回调
            string resdir = System.IO.Path.Combine(modEntry.Path, "Data");
            Logger.Log(" resdir :" + resdir);
            BaseResourceMod.Main.registModResDir(modEntry, resdir);
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }
        
    }
}