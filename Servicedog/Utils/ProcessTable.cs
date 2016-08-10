using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Servicedog.Utils
{
    public class ProcessTable
    {
        private readonly ConcurrentDictionary<int, TinyProcessInfo> _table = new ConcurrentDictionary<int, TinyProcessInfo>();
        private static string CREATION = "__InstanceCreationEvent";
        private static string DELETION = "__InstanceDeletionEvent";

        public ProcessTable()
        {
            InitializeTableFromWMI();
        }
        private void Maintenance()
        {
            foreach(var process in Listen(CREATION))
            {
                _table[process.Id] = process;
            }

            foreach (var process in Listen(DELETION))
            {
                _table[process.Id].Stop = DateTime.Now;//hazardows, I know. TODO: check for null
            }
        }

        private void InitializeTableFromWMI()
        {
            try
            {
                //https://msdn.microsoft.com/en-us/library/aa394372(v=vs.85).aspx
                string query = "Select * From Win32_Process";
                StringBuilder sb = new StringBuilder();

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection processList = searcher.Get();

                foreach (ManagementObject obj in processList)
                {

                    if(obj.GetPropertyValue("Name").ToString() == "System Idle Process")
                        continue;

                    TinyProcessInfo process = new TinyProcessInfo();
                    process.Id = int.Parse(obj.GetPropertyValue("ProcessId").ToString());
                    process.Creation = ((DateTime)obj.GetPropertyValue("CreationDate"));

                    if(obj.GetPropertyValue("CommandLine") != null)
                        process.CommandLine = obj.GetPropertyValue("CommandLine").ToString();
                    process.KernelImageFileName = obj.GetPropertyValue("Name").ToString();

                    _table[process.Id] = process;
                }

            }
            catch (Exception e)
            {
            }
        }


        //__InstanceCreationEvent
        //__InstanceDeletionEvent
        //__InstanceModificationEvent
        private IEnumerable<TinyProcessInfo> Listen(string eventName)
        {
            //Task.Run(() =>
            //{
                TinyProcessInfo process = new TinyProcessInfo();

                // Create event query to be notified within 1 second of 
                // a change in a service
                WqlEventQuery query =
                    new WqlEventQuery(eventName,
                    "TargetInstance isa \"Win32_Process\"");

                // Initialize an event watcher and subscribe to events 
                // that match this query
                ManagementEventWatcher watcher =
                    new ManagementEventWatcher();
                watcher.Query = query;
                // times out watcher.WaitForNextEvent in 5 seconds
                // watcher.Options.Timeout = new TimeSpan(0, 0, 1);

                // Block until the next event occurs 
                // Note: this can be done in a loop if waiting for 
                //        more than one occurrence
                while (true) {
                    ManagementBaseObject e = watcher.WaitForNextEvent();
                process.ImageFileName = ((ManagementBaseObject)e
                        ["TargetInstance"])["Name"].ToString();

                     yield return process;
                    //Display information from the event
                    Console.WriteLine(
                        "Process {0} has been created, path is: {1}",
                        ((ManagementBaseObject)e
                        ["TargetInstance"])["Name"],
                        ((ManagementBaseObject)e
                        ["TargetInstance"])["ExecutablePath"]);

                    //Cancel the subscription
                    // watcher.Stop();

                }
            //});
        }
    }

    public class TinyProcessInfo
    {
        public int Id { get; set; }
        public string ApplicationID { get; set; }
        public string CommandLine { get; set; }
        public int ExitStatus { get; set; }
        public string ImageFileName { get; set; }
        public string KernelImageFileName { get; set; }
        public DateTime Creation { get; set; }
        public DateTime Stop { get; set; }
        public bool Expired
        {
            get
            {
                return DateTime.Now.Subtract(Stop).TotalSeconds > 30;
            }
        }

    }
}
