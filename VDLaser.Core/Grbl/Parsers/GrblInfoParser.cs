using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;

namespace VDLaser.Core.Grbl.Parsers
{
    public class GrblInfoParser : IGrblSubParser
    {
        private readonly ILogService _log;

        public string Name => "GrblVersionParser";
        public GrblInfoParser(ILogService log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _log.Information("[GrblInfoParser] Initialised");
        }

        public GrblInfoParser()
        {
        }

        public bool CanParse(string line)
        {
            if (string.IsNullOrEmpty(line)) return false;
            line = line.Trim().ToLowerInvariant();
            return
            line.StartsWith("grbl") ||
            line.StartsWith("[") && line.EndsWith("]");
        }

        public void Parse(string line, GrblState state) { }

        public void Parse(string line, GrblInfo state)
        {
            if (string.IsNullOrEmpty(line) || state == null) return;
            var wm = line.Trim().ToLowerInvariant();
            if (wm.StartsWith("grbl", StringComparison.OrdinalIgnoreCase))
            {
                state.WelcomMessage = line;
                state.GrblVersion = wm.Substring(5, 4);
                return;
            }

            var message = line.Trim('[', ']');

            if (message.Length == 0) return;

            if (message.Length >= 1)
            {
                if (message.StartsWith("VER", StringComparison.OrdinalIgnoreCase))
                {
                    HandleVersion(message, state);
                    return;
                }
                if (message.StartsWith("OPT"))
                {
                    HandleOption(message, state);
                    return;
                }
                if (message.StartsWith("MSG"))
                {
                    HandleFeedBackMessage(message, state);
                    return;
                }
                if (message.StartsWith("HLP"))
                {
                    HandleHelpMessage(message, state);
                    return;
                }
                if (message.StartsWith("GC"))
                {
                    HandleGCodeMessage(message, state);
                    return;
                }
                //Ajouter le parameter data  (Command $#): [G54:0.000,0.000,0.000]...
            }
        }
        /// <summary>
        /// Command $I => [VER:1.1d.20161014:]
        /// </summary>
        /// <param name="line"></param>
        /// <param name="state"></param>
        private void HandleVersion(string message, GrblInfo state)
        {
            var version = message.Trim('[', ']');
            state.GrblVersion = version.Substring(4, 4);
            if (version.Length >= 8)
                state.GrblBuild = version.Substring(9);
            _log.Information("[GrblInfoParser] Grbl version {ver}", message);

        }

        /// <summary>
        /// Command $I => [OPT:VL,15,128]
        /// </summary>
        /// <param name="line"></param>
        /// <param name="state"></param>
        private void HandleOption(string message, GrblInfo state)
        {
            var options = message.Trim('[', ']').Split(':', '|', ',');
            if (options.Length == 3)
            { 
                state.CompileOptions = string.Empty;
                state.BlockBufferSize = options[1];
                state.RxBufferSize = options[2];
            }
            else if (options.Length > 3)
            { 
                state.CompileOptions = options[1];
                state.BlockBufferSize = options[2];
                state.RxBufferSize = options[3];
            }
            else
            {
                state.CompileOptions = string.Empty;
                state.BlockBufferSize = string.Empty;
                state.RxBufferSize = string.Empty;
            }
            _log.Information("[GrblInfoParser] Grbl option {opt}", message);

        }

        /// <summary>
        /// Non query feed back message => [MSG:...]
        /// </summary>
        /// <param name="line"></param>
        /// <param name="state"></param>
        private void HandleFeedBackMessage(string message, GrblInfo state)
        {
            state.FeedbackMessage = message.Substring(0,4);
            _log.Information("[GrblInfoParser] Grbl feedback {back}", state.FeedbackMessage);

        }
        /// <summary>
        /// Command $ => [HLP:$$ $# $G $I $N $x=val $Nx=line $J=line $C $X $H ~ ! ? ctrl-x]
        /// </summary>
        /// <param name="message"></param>
        /// <param name="state"></param>
        private void HandleHelpMessage(string message, GrblInfo state)
        {
            state.HelpMessage = message.Substring(0, 4);
            _log.Information("[GrblInfoParser] Grbl help {hlp}", message);

        }
        /// <summary>
        /// Command $G => [GC:G0 G54 G17 G21 G90 G94 M5 M9 T0 F0.0 S0]
        /// </summary>
        /// <param name="message"></param>
        /// <param name="state"></param>
        private void HandleGCodeMessage(string message, GrblInfo state)
        {
            state.GCodeMessage = message.Substring(0, 3);
            _log.Information("[GrblInfoParser] Grbl GCode {gc}", message);
        }
    }
}
