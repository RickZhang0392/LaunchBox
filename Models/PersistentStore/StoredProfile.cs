using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchBox.Models.PersistentStore
{
    public class StoredProfile
    {
        public string id { get; set; }
        public string appid { get; set; }
        public string displayname { get; set; }
        public string parameters { get; set; }
        public bool nowindow { get; set; }
        public UseageAlarm cpu { get; set; }
        public UseageAlarm memory { get; set; }
        public UseageAlarm time { get; set; }
        public bool alarmnotification { get; set; }

        public class UseageAlarm
        {
            public bool need { get; set; }
            public double value { get; set; }
        }
    }
}
