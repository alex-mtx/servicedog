using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace EventTraceExperiments
{
   public static class StringExtensions
    {
       public  static bool ContainsAny(this string str, List<string> strings)
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

     
    }
}
