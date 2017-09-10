using Microsoft.Diagnostics.Tracing;
using Newtonsoft.Json;
using Servicedog.Manifests.Afd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Servicedog.OS
{
    public static class StringExtensions
    {
        public static bool ContainsAny(this string str, List<string> strings)
        {
            foreach (var item in strings)
            {
                if (str.Contains(item))
                    return true;
            }
            return false;
        }

        public static string GetInfoFromWMI(this Process current)
        {
            try
            {
                string query = "Select * From Win32_Process Where ProcessID = " + current.Id;
                StringBuilder sb = new StringBuilder();

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection processList = searcher.Get();

                foreach (ManagementObject obj in processList)
                {
                    sb.Append(obj.GetPropertyValue("Name"));

                    string[] argList = new string[] { string.Empty, string.Empty };
                    int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                    {
                        // return DOMAIN\user
                        sb.AppendFormat(" user: {0} ", argList[1] + "\\" + argList[0]);
                    }

                    sb.AppendFormat(" cmd: {0} ", obj.GetPropertyValue("CommandLine"));

                }

                return sb.ToString();
            }
            catch (Exception e)
            {
                return "dead proccess";
            }
        }

        public static string ToJSON( TraceEvent data,Type type)
        {
            //TODO: implement the faster way
            //faster way
            //http://www.newtonsoft.com/json/help/html/ReadingWritingJSON.htm
            //StringWriter sw = new StringWriter();
            //using (JsonTextWriter writer = new JsonTextWriter(sw))
            //{
            //    //{
            //    writer.WriteStartObject();
            //    writer.WritePropertyName("processId");
            //    writer.WriteValue(data.ProcessID);

            //    //...
            //    writer.WriteEndObject();
            //    return sw.ToString();
            //}

            //faster implementation

            return JsonConvert.SerializeObject(data,type,new JsonSerializerSettings());


        }
    }
}
