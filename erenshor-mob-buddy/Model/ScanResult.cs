using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace erenshor_mob_buddy.Model
{
    internal class ScanResult
    {
        public string MobName { get; private set; }
        public bool Found { get; private set; }

        public ScanResult(string MobName, bool Found)
        {
            this.MobName = MobName;
            this.Found = Found;
        }
    }
}
