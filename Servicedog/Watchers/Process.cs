﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Session;
using Servicedog.Messaging;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System.Diagnostics;

namespace Servicedog.Watchers
{
    public class Process : Watcher
    {
        public const string PROCESS_CREATION = "process_creation";
        public const string PROCESS_END = "process_end";

        public Process(IDispatcher sender) : base(sender)
        {
        }

        protected override void Capture(TraceEventSession session)
        {
            session.EnableKernelProvider(Microsoft.Diagnostics.Tracing.Parsers.KernelTraceEventParser.Keywords.Process);
            session.Source.Kernel.ProcessStart += (ProcessTraceData data) => {
                Debug.WriteLine(data.Dump());
                _sender.Send(data.ProcessID,"Process Started:" + data.CommandLine, PROCESS_CREATION);    
            };
            session.Source.Kernel.ProcessStop += (ProcessTraceData data) =>
              {
                  Debug.WriteLine(data.Dump());
                  _sender.Send(data.ProcessID, "Process Ended:" + data.CommandLine, PROCESS_END);

              };
        }

        protected override string SessionName()
        {
            return SESSION_NAME_PREFIX + "process";
        }
    }
}