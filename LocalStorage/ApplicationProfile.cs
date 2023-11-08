using LaunchBox.Models.PersistentStore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchBox.LocalStorage
{
    public class ApplicationProfile
    {
        private static string storage_folder = @"data\profile";

        public static StoredProfile? Get(string appid)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + storage_folder+@"\"+appid+".profile"))
            {
                return null;
            }
            using (StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + storage_folder + @"\" + appid + ".profile"))
            {
                string json = sr.ReadToEnd();
                try
                {
                    return JsonConvert.DeserializeObject<StoredProfile>(json);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public static bool Add(StoredProfile application)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + storage_folder))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + storage_folder);
            }
            var content = JsonConvert.SerializeObject(application);
            using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + storage_folder + @"\"+application.appid+".profile", false))
            {
                sw.WriteLine(content);
            }
            return true;
        }

        public static bool Update(StoredProfile profile)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + storage_folder))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + storage_folder);
            }
            var content = JsonConvert.SerializeObject(profile);
            using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + storage_folder + @"\" + profile.appid + ".profile", false))
            {
                sw.WriteLine(content);
            }
            return true;
        }
    }
}
