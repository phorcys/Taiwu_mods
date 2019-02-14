using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NpcScan
{
    public class Features
    {
        public int Key = 0;
        public int Level = 0;
        public int Group = 0;
        public string Name = "";
        public int Plus = 0;
        public string Color = "";
        public string tarColor = "";

        public Features(int i)
        {
            this.Key = i;
            this.Level = int.Parse(DateFile.instance.actorFeaturesDate[i][4]);
            this.Group = int.Parse(DateFile.instance.actorFeaturesDate[i][5]);
            this.Name = DateFile.instance.actorFeaturesDate[i][0];
            this.Plus = int.Parse(DateFile.instance.actorFeaturesDate[i][8]);
            switch (Plus)
            {
                case 0:
                    this.Color = Main.textColor[20003];
                    break;
                case 3:
                    this.Color = Main.textColor[20006];
                    break;
                case 4:
                    this.Color = Main.textColor[10004];
                    break;
            }
            this.tarColor = Main.textColor[20004];
        }

        public Features()
        {

        }
    }

}
