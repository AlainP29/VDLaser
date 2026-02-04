using System;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;

namespace VDLaser.Core.Grbl.Parsers
{
    /// <summary>
    /// Parses GRBL informational messages such as:
    /// - "Grbl 1.1h ['$' for help]"
    /// - [VER:1.1d.20161014:]
    /// - [OPT:VL,15,128]
    /// - [MSG:...]
    /// - [HLP:...]
    /// - [GC:...]
    /// Updates the GrblInfo object accordingly.
    /// </summary>
    public class GrblInfoParser : IGrblSubParser
    {
        private readonly ILogService? _log;

        public string Name => "GrblInfoParser";

        #region Constructors

        public GrblInfoParser() { }

        public GrblInfoParser(ILogService log)
        {
            _log = log;
            _log?.Information("[GrblInfoParser] Initialised");
        }

        #endregion

        #region CanParse

        /// <summary>
        /// Determines whether the line is a GRBL info message.
        /// Matches:
        /// - "Grbl ..."
        /// - "[...]"
        /// </summary>
        public bool CanParse(string line)
        {
            if (string.IsNullOrEmpty(line))
                return false;

            line = line.Trim().ToLowerInvariant();

            return line.StartsWith("grbl") ||
                   (line.StartsWith("[") && line.EndsWith("]"));
        }

        #endregion

        #region Parse Entry Points

        public void Parse(string line, GrblState state)
        {
            // This parser does not update GrblState
        }

        /// <summary>
        /// Parses GRBL info messages and updates GrblInfo.
        /// </summary>
        public void Parse(string line, GrblInfo state)
        {
            if (string.IsNullOrEmpty(line) || state == null)
                return;

            var wm = line.Trim().ToLowerInvariant();

            // Welcome message: "Grbl 1.1h ['$' for help]"
            if (wm.StartsWith("grbl", StringComparison.OrdinalIgnoreCase))
            {
                state.WelcomMessage = line;
                state.GrblVersion = wm.Substring(5, 4);
                _log?.Information("[GrblInfoParser] Welcome message detected: {Msg}", line);
                return;
            }

            // Strip brackets
            var message = line.Trim('[', ']');
            if (message.Length == 0)
                return;

            // Route message type
            if (message.StartsWith("VER", StringComparison.OrdinalIgnoreCase))
            {
                HandleVersion(message, state);
                return;
            }

            if (message.StartsWith("OPT", StringComparison.OrdinalIgnoreCase))
            {
                HandleOption(message, state);
                return;
            }

            if (message.StartsWith("MSG", StringComparison.OrdinalIgnoreCase))
            {
                HandleFeedBackMessage(message, state);
                return;
            }

            if (message.StartsWith("HLP", StringComparison.OrdinalIgnoreCase))
            {
                HandleHelpMessage(message, state);
                return;
            }

            if (message.StartsWith("GC", StringComparison.OrdinalIgnoreCase))
            {
                HandleGCodeMessage(message, state);
                return;
            }

            // Future: handle parameter data ($#)
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Handles GRBL version info: [VER:1.1d.20161014:]
        /// </summary>
        private void HandleVersion(string message, GrblInfo state)
        {
            var version = message.Trim('[', ']');

            state.GrblVersion = version.Substring(4, 4);

            if (version.Length >= 8)
                state.GrblBuild = version.Substring(9);

            _log?.Information("[GrblInfoParser] GRBL version: {Ver}", message);
        }

        /// <summary>
        /// Handles GRBL compile options: [OPT:VL,15,128]
        /// </summary>
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

            _log?.Information("[GrblInfoParser] GRBL options: {Opt}", message);
        }

        /// <summary>
        /// Handles GRBL feedback messages: [MSG:...]
        /// </summary>
        private void HandleFeedBackMessage(string message, GrblInfo state)
        {
            state.FeedbackMessage = message.Substring(0, 4);
            _log?.Information("[GrblInfoParser] GRBL feedback: {Msg}", state.FeedbackMessage);
        }

        /// <summary>
        /// Handles GRBL help messages: [HLP:...]
        /// </summary>
        private void HandleHelpMessage(string message, GrblInfo state)
        {
            state.HelpMessage = message.Substring(0, 4);
            _log?.Information("[GrblInfoParser] GRBL help: {Msg}", message);
        }

        /// <summary>
        /// Handles GRBL GCode state: [GC:G0 G54 G17 ...]
        /// </summary>
        private void HandleGCodeMessage(string message, GrblInfo state)
        {
            state.GCodeMessage = message.Substring(0, 3);
            _log?.Information("[GrblInfoParser] GRBL GCode state: {Msg}", message);
        }

        #endregion
    }
}
