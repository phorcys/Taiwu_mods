using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace RestEquip
{
    public class Settings : UnityModManager.ModSettings
    {
        public int RecoveryMianQiGongFaSelectedIndex = 0;
        public int RecoveryMianQiGongFaIndex
        {
            get { return RecoveryMianQiGongFaSelectedIndex - 1; }
        }
        public int RecoveryMianQiEquipSelectedIndex = 0;
        public int RecoveryMianQiEquipConfigIndex
        {
            get { return RecoveryMianQiEquipSelectedIndex - 1; }
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
