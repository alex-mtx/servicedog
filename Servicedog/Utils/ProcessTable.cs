using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Servicedog.Utils
{
    /// <summary>
    /// Provides access to a <see cref="ProcessInfo"/> even when the original process has been terminated.
    /// <para>Each time a process is created or terminated by the underlying operating system this table is updated.
    /// </para>
    /// </summary>
    public class ProcessTable: ManagementEventWatcher
    {
        private readonly ConcurrentDictionary<uint, ProcessInfo> _table = new ConcurrentDictionary<uint, ProcessInfo>();
        private const  string CREATION = "__InstanceCreationEvent";
        private const string DELETION = "__InstanceDeletionEvent";
        private const string MODIFICATION = "__InstanceModificationEvent";
        private static readonly string WMI_OPER_EVENT_QUERY = @"SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";

        public ProcessTable()
        {
            this.Query = new WqlEventQuery(WMI_OPER_EVENT_QUERY);
        }

        public void Init()
        {
            InitializeTableFromWMI();
            this.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            this.Start();
        }

        public ProcessInfo Get(int id)
        {
            var process = new ProcessInfo();
            process.IsDefault = true;//if something goes wrong the client code won't explode with null exceptions

            if (_table.Count == 0)
                return process;

            if(_table.TryGetValue((uint)id, out process))
                process.IsDefault = false;

            return process;
        }


        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            string eventType = e.NewEvent.ClassPath.ClassName;
            ProcessInfo process;

            if (!TryConvert(e.NewEvent["TargetInstance"] as ManagementBaseObject, out process))
                return;

            switch (eventType)
            {
                case CREATION:
                    ProcessCreated(process);
                    break;
                case DELETION:
                    ProcessDeleted(process);
                    break;
                case MODIFICATION:
                    //ProcessModified(process); //TODO: understand what makes a process come this way
                    break;
            }
        }

        private void ProcessDeleted(ProcessInfo process)
        {
            if (_table.ContainsKey(process.ProcessId))
                _table[process.ProcessId].TerminationDate = DateTime.Now;
        }


        private void ProcessCreated(ProcessInfo process)
        {
            _table[process.ProcessId] = process;
            ExcludeOldDeleted();
        }

        private void ExcludeOldDeleted()
        {
            var expiredItems = _table.Where(x => x.Value.Expired == true);
            var tableSize = _table.Count;
            decimal proportion = (decimal) expiredItems.Count() / tableSize;
            decimal maxPercent = 0.01M;

            if (proportion > maxPercent)//no more than 10%
            {
                Debug.WriteLine("more than 10% expired");
                ProcessInfo process;
                foreach (var item in expiredItems)
                {
                    _table.TryRemove(item.Value.ProcessId, out process);
                    Debug.WriteLine("Expired and Removed from table " + process.ProcessId);
                }
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
                    ProcessInfo process;
                    if (!TryConvert(obj, out process))
                        continue;

                    _table[process.ProcessId] = process; 
                }

            }
            catch (Exception e)
            {
            }
        }

        private bool TryConvert(ManagementBaseObject obj, out ProcessInfo process)
        {
            process = new ProcessInfo();
            process.IsDefault = true;

            if (obj == null || (uint?)obj["ProcessId"] == 0)
                return false;

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

            process.IsDefault = false;
            return true;
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
        public bool IsDefault;

        public override string ToString()
        {
            if (IsDefault)
                return "Empty process";

            var path = string.IsNullOrEmpty(ExecutablePath) ? string.Empty : ExecutablePath.Replace("\\", "/");

            return string.Format("{0}, {1}, {2}", ProcessId, Name,path);
        }

        internal string ToJson()
        {
            if (IsDefault)
                return "{ }";
            var path = string.IsNullOrEmpty(ExecutablePath) ? string.Empty : ExecutablePath;
            return "{ \"id\":" + ProcessId + ", \"name\":\"" + Name +"\",\"path\":\"" + path + "\"}";
        }
    }
}
