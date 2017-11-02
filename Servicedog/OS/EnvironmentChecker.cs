using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Servicedog.OS
{
    public static class EnvironmentChecker
    {
        public static void EnsureIsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator) == false)
                throw new ApplicationException("Servicedog requires Admin privileges to run in order to start ETW Kernel Sessions");
        }
    }
}
