using System;
using System.Collections.Generic;
using System.Linq;
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

     
    }
}
