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
    public class ApplicationStorage
    {
        private static string storage_file = @"data\application.storage";
        public static List<StoredApplication> Get()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + storage_file))
            {
                return new List<StoredApplication>();
            }
            using (StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + storage_file))
            {
                string json = sr.ReadToEnd();
                try
                {
                    return JsonConvert.DeserializeObject<List<StoredApplication>>(json);
                }catch(Exception ex)
                {
                    return new List<StoredApplication>();
                }
            }
        }

        public static StoredApplication? Get(string appid)
        {
            List<StoredApplication> applications = Get();
            if (!applications.Any())
            {
                return null;
            }
            var application = applications.Where(t => t.id == appid).ToList();
            if (!application.Any())
            {
                return null;
            }
            return application.First();
        }

        public static bool Add(StoredApplication application)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"data"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"data");
            }
            application.id = Guid.NewGuid().ToString();
            List<StoredApplication> applications = Get();
            applications.Add(application);
            var content = JsonConvert.SerializeObject(applications);
            using(StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + storage_file, false))
            {
                sw.WriteLine(content);
            }
            return true;
        }

        public static bool Remove(string applicationid)
        {
            List<StoredApplication> applications = Get();
            applications.RemoveAll(t=>t.id==applicationid);
            var content = JsonConvert.SerializeObject(applications);
            using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + storage_file, false))
            {
                sw.WriteLine(content);
            }

            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory+ @"data\profile\" + applicationid + ".profile"))
            {
                try
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"data\profile\" + applicationid + ".profile");
                }
                catch(Exception ex)
                {

                }
            }

            return true;
        }
    }
}
