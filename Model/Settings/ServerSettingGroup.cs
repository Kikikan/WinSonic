using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSonic.Model.Settings
{
    public class ServerSettingGroup : ISettingGroup
    {
        public string Key => "servers";

        public void Load(Dictionary<string, string> settings)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> ToDictionary()
        {
            throw new NotImplementedException();
        }
    }
}
