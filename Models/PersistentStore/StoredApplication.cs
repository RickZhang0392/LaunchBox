using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchBox.Models.PersistentStore
{
    public class StoredApplication
    {
        public string id { get; set; }
        public string? name { get; set; }
        public string? extension { get; set; }
        public string? path { get; set; }
    }
}
