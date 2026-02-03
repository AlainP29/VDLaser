using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Grbl.Commands
{
    public sealed class GrblCommand
    {
        public string Command { get; }
        public string? Source { get; }   // ex: "Settings", "Console", "Jogging"
        public bool WaitForOk { get; } = true;
        public string? LastError { get; set; }

        public TaskCompletionSource<GrblCommandResult> Completion { get; }
            = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public GrblCommand(string command, string? source = null, bool waitForOk=true)
        {
            Command = command;
            Source = source;
            WaitForOk = waitForOk;
        }
    }


}
