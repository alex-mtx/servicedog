using System;

namespace Servicedog.OS
{
    public interface IProcessInfo
    {
        string Caption { get; set; }
        string CommandLine { get; set; }
        string CreationClassName { get; set; }
        DateTime? CreationDate { get; set; }
        string CSCreationClassName { get; set; }
        string CSName { get; set; }
        string Description { get; set; }


        string ExecutablePath { get; set; }
        ushort? ExecutionState { get; set; }
        bool Expired { get; }
        string Handle { get; set; }
        uint? HandleCount { get; set; }
        DateTime? InstallDate { get; set; }
        bool IsDefault { get; set; }
        ulong? KernelModeTime { get; set; }
        uint? MaximumWorkingSetSize { get; set; }
        uint? MinimumWorkingSetSize { get; set; }
        string Name { get; set; }
        string OSCreationClassName { get; set; }
        string OSName { get; set; }
        ulong? OtherOperationCount { get; set; }
        ulong? OtherTransferCount { get; set; }
        uint? PageFaults { get; set; }
        uint? PageFileUsage { get; set; }
        uint? ParentProcessId { get; set; }
        uint? PeakPageFileUsage { get; set; }
        ulong? PeakVirtualSize { get; set; }
        uint? PeakWorkingSetSize { get; set; }
        uint? Priority { get; set; }
        ulong? PrivatePageCount { get; set; }
        uint ProcessId { get; set; }
        uint? QuotaNonPagedPoolUsage { get; set; }
        uint? QuotaPagedPoolUsage { get; set; }
        uint? QuotaPeakNonPagedPoolUsage { get; set; }
        uint? QuotaPeakPagedPoolUsage { get; set; }
        ulong? ReadOperationCount { get; set; }
        ulong? ReadTransferCount { get; set; }
        uint? SessionId { get; set; }
        string Status { get; set; }
        DateTime? TerminationDate { get; set; }
        uint? ThreadCount { get; set; }
        ulong? UserModeTime { get; set; }
        ulong? VirtualSize { get; set; }
        string WindowsVersion { get; set; }
        ulong? WorkingSetSize { get; set; }
        ulong? WriteOperationCount { get; set; }
        ulong? WriteTransferCount { get; set; }
        string ToString();
        string ToJson();

    }
}