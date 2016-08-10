using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Servicedog.Utils
{
    public class ProcessTable: ManagementEventWatcher
    {
        private readonly ConcurrentDictionary<uint, ProcessInfo> _table = new ConcurrentDictionary<uint, ProcessInfo>();
        private const  string CREATION = "__InstanceCreationEvent";
        private const string DELETION = "__InstanceDeletionEvent";
        private const string MODIFICATION = "__InstanceModificationEvent";
        private static readonly string WMI_OPER_EVENT_QUERY = @"SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";

        public ProcessTable()
        {
            this.Query.QueryString = WMI_OPER_EVENT_QUERY;
            this.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            Maintenance();
        }

        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            string eventType = e.NewEvent.ClassPath.ClassName;
            ProcessInfo proc = new
                Win32_Process(e.NewEvent["TargetInstance"] as ManagementBaseObject);

            switch (eventType)
            {
                case CREATION:
                    if (ProcessCreated != null) ProcessCreated(proc); break;
                case DELETION:
                    if (ProcessDeleted != null) ProcessDeleted(proc); break;
                case MODIFICATION:
                    if (ProcessModified != null) ProcessModified(proc); break;
            }
        }
        private void Maintenance()
        {
            foreach(var process in Listen(CREATION))
            {
                if (obj.GetPropertyValue("Name").ToString() == "System Idle Process")
                    continue;

                if (obj["ProcessId"] == null)//can ever happen? 
                    continue;

                _table[process.ProcessId] = process;
            }

            foreach (var process in Listen(DELETION))
            {
                _table[process.ProcessId].TerminationDate = DateTime.Now;//hazardows, I know. TODO: check for null
            }
        }

        private void InitializeTableFromWMI()
        {
            try
            {
                //https://msdn.microsoft.com/en-us/library/aa394372(v=vs.85).aspx
                string query = "Select * From Win32_Process";

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection processList = searcher.Get();

                foreach (ManagementObject obj in processList)
                {

                    

                    _table[process.ProcessId] = process;
                }

            }
            catch (Exception e)
            {
            }
        }

        private ProcessInfo Convert(ManagementObject obj)
        {
            
            ProcessInfo process = new ProcessInfo();
            process.Caption = (string)obj["Caption"];
            process.CommandLine = (string)obj["CommandLine"];
            process.CreationClassName = (string)obj["CreationClassName"];

            if (obj["CreationDate"] != null)
                process.CreationDate = ManagementDateTimeConverter.ToDateTime((string)obj["CreationDate"]);

            process.CSCreationClassName = (string)obj["CSCreationClassName"];
            process.CSName = (string)obj["CSName"];
            process.Description = (string)obj["Description"];
            process.ExecutablePath = (string)obj["ExecutablePath"];
            process.ExecutionState = (ushort?)obj["ExecutionState"];
            process.Handle = (string)obj["Handle"];
            process.HandleCount = (uint?)obj["HandleCount"];

            if (obj["InstallDate"] != null)
                process.InstallDate = ManagementDateTimeConverter.ToDateTime((string)obj["InstallDate"]);

            process.KernelModeTime = (ulong?)obj["KernelModeTime"];
            process.MaximumWorkingSetSize = (uint?)obj["MaximumWorkingSetSize"];
            process.MinimumWorkingSetSize = (uint?)obj["MinimumWorkingSetSize"];
            process.Name = (string)obj["Name"];
            process.OSCreationClassName = (string)obj["OSCreationClassName"];
            process.OSName = (string)obj["OSName"];
            process.OtherOperationCount = (ulong?)obj["OtherOperationCount"];
            process.OtherTransferCount = (ulong?)obj["OtherTransferCount"];
            process.PageFaults = (uint?)obj["PageFaults"];
            process.PageFileUsage = (uint?)obj["PageFileUsage"];
            process.ParentProcessId = (uint?)obj["ParentProcessId"];
            process.PeakPageFileUsage = (uint?)obj["PeakPageFileUsage"];
            process.PeakVirtualSize = (ulong?)obj["PeakVirtualSize"];
            process.PeakWorkingSetSize = (uint?)obj["PeakWorkingSetSize"];
            process.Priority = (uint?)obj["Priority"];
            process.PrivatePageCount = (ulong?)obj["PrivatePageCount"];
            process.ProcessId = (uint)obj["ProcessId"];
            process.QuotaNonPagedPoolUsage = (uint?)obj["QuotaNonPagedPoolUsage"];
            process.QuotaPagedPoolUsage = (uint?)obj["QuotaPagedPoolUsage"];
            process.QuotaPeakNonPagedPoolUsage = (uint?)obj["QuotaPeakNonPagedPoolUsage"];
            process.QuotaPeakPagedPoolUsage = (uint?)obj["QuotaPeakPagedPoolUsage"];
            process.ReadOperationCount = (ulong?)obj["ReadOperationCount"];
            process.ReadTransferCount = (ulong?)obj["ReadTransferCount"];
            process.SessionId = (uint?)obj["SessionId"];
            process.Status = (string)obj["Status"];

            if (obj["TerminationDate"] != null)
                process.TerminationDate = ManagementDateTimeConverter.ToDateTime((string)obj["TerminationDate"]);
            process.ThreadCount = (uint?)obj["ThreadCount"];
            process.UserModeTime = (ulong?)obj["UserModeTime"];
            process.VirtualSize = (ulong?)obj["VirtualSize"];
            process.WindowsVersion = (string)obj["WindowsVersion"];
            process.WorkingSetSize = (ulong?)obj["WorkingSetSize"];
            process.WriteOperationCount = (ulong?)obj["WriteOperationCount"];
            process.WriteTransferCount = (ulong?)obj["WriteTransferCount"];

            return process;
        }

        //__InstanceCreationEvent
        //__InstanceDeletionEvent
        //__InstanceModificationEvent
        private IEnumerable<ProcessInfo> Listen(string eventName)
        {
            //Task.Run(() =>
            //{
                ProcessInfo process = new ProcessInfo();

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
                process.Name = ((ManagementBaseObject)e
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

    public class ProcessInfo
    {
        public string Caption;
        public string CommandLine;
        public string CreationClassName;
        public DateTime? CreationDate;
        public string CSCreationClassName;
        public string CSName;
        public string Description;
        public string ExecutablePath;
        public ushort? ExecutionState;
        public string Handle;
        public uint? HandleCount;
        public DateTime? InstallDate;
        public ulong? KernelModeTime;
        public uint? MaximumWorkingSetSize;
        public uint? MinimumWorkingSetSize;
        public string Name;
        public string OSCreationClassName;
        public string OSName;
        public ulong? OtherOperationCount;
        public ulong? OtherTransferCount;
        public uint? PageFaults;
        public uint? PageFileUsage;
        public uint? ParentProcessId;
        public uint? PeakPageFileUsage;
        public ulong? PeakVirtualSize;
        public uint? PeakWorkingSetSize;
        public uint? Priority;
        public ulong? PrivatePageCount;
        public uint ProcessId;
        public uint? QuotaNonPagedPoolUsage;
        public uint? QuotaPagedPoolUsage;
        public uint? QuotaPeakNonPagedPoolUsage;
        public uint? QuotaPeakPagedPoolUsage;
        public ulong? ReadOperationCount;
        public ulong? ReadTransferCount;
        public uint? SessionId;
        public string Status;
        public DateTime? TerminationDate;
        public uint? ThreadCount;
        public ulong? UserModeTime;
        public ulong? VirtualSize;
        public string WindowsVersion;
        public ulong? WorkingSetSize;
        public ulong? WriteOperationCount;
        public ulong? WriteTransferCount;
        public bool Expired
        {
            get
            {
                if (!TerminationDate.HasValue)
                    return false;
                return DateTime.Now.Subtract(TerminationDate.Value).TotalSeconds > 30;
            }
        }

    }
}
