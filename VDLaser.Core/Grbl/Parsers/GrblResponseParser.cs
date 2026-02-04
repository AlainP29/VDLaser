using System;
using VDLaser.Core.Codes;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;

namespace VDLaser.Core.Grbl.Parsers
{
    /// <summary>
    /// Parses GRBL responses such as:
    /// - ok
    /// - error:XX
    /// - ALARM:XX
    /// - busy:...
    /// - [MSG:...]
    /// Updates GrblState with structured GrblError and GrblAlarm objects.
    /// </summary>
    public class GrblResponseParser : IGrblSubParser
    {
        private readonly ILogService? _log;

        public string Name => "GrblResponseParser";

        private readonly ErrorCodes _errors = new();
        private readonly AlarmCodes _alarms = new();

        #region Constructors

        public GrblResponseParser() { }

        public GrblResponseParser(ILogService log)
        {
            _log = log;
            _log?.Information("[GrblResponseParser] Initialised");
        }

        #endregion

        #region CanParse

        public bool CanParse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            line = line.Trim().ToLowerInvariant();

            return line == "ok"
                || line.StartsWith("error:")
                || line.StartsWith("alarm:")
                || line.StartsWith("busy:")
                || (line.StartsWith("[") && line.EndsWith("]"));
        }

        #endregion

        #region Parse Entry Points

        public void Parse(string line, GrblInfo state)
        {
            // Not used for GrblInfo
        }

        public void Parse(string line, GrblState state)
        {
            if (string.IsNullOrWhiteSpace(line) || state == null)
                return;

            var msg = line.Trim();
            state.LastMessage = msg;

            // OK
            if (msg.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                state.IsBusy = false;
                state.LastBusyMessage = string.Empty;
                _log?.Debug("[GrblResponseParser] OK");
                return;
            }

            // BUSY
            if (msg.StartsWith("busy:", StringComparison.OrdinalIgnoreCase))
            {
                state.IsBusy = true;
                state.LastBusyMessage = msg;
                _log?.Debug("[GrblResponseParser] Busy: {Msg}", msg);
                return;
            }

            // ERROR
            if (msg.StartsWith("error:", StringComparison.OrdinalIgnoreCase))
            {
                HandleError(msg, state);
                return;
            }

            // ALARM
            if (msg.StartsWith("alarm:", StringComparison.OrdinalIgnoreCase))
            {
                HandleAlarm(msg, state);
                return;
            }

            // [MSG:...]
            if (msg.StartsWith("[") && msg.EndsWith("]"))
            {
                HandleMessage(msg, state);
                return;
            }
        }

        #endregion

        #region Handlers

        private void HandleError(string msg, GrblState state)
        {
            var codeStr = msg.Substring(6).Trim();

            if (int.TryParse(codeStr, out int code))
            {
                var err = _errors.GetError(code, state.IsGrbl11);

                if (err != null)
                {
                    state.ErrorMessage = err.ToString();
                    _log?.Error("[GrblResponseParser] Error {Code}: {Msg}", code, err.Message);
                }
                else
                {
                    state.ErrorMessage = $"Unknown error {code}";
                    _log?.Error("[GrblResponseParser] Unknown error code: {Code}", code);
                }
            }
            else
            {
                state.ErrorMessage = "Invalid error format";
                _log?.Error("[GrblResponseParser] Invalid error format: {Msg}", msg);
            }
        }

        private void HandleAlarm(string msg, GrblState state)
        {
            var codeStr = msg.Substring(6).Trim();

            if (int.TryParse(codeStr, out int code))
            {
                var alarm = _alarms.GetAlarm(code, state.IsGrbl11);

                if (alarm != null)
                {
                    state.Alarm = alarm;
                    state.AlarmHistory.Add(alarm);

                    _log?.Error("[GrblResponseParser] Alarm {Code}: {Msg} ({Severity})",
                        code, alarm.Message, alarm.Severity);
                }
                else
                {
                    state.ErrorMessage = $"Unknown alarm {code}";
                    _log?.Error("[GrblResponseParser] Unknown alarm code: {Code}", code);
                }
            }
            else
            {
                state.ErrorMessage = "Invalid alarm format";
                _log?.Error("[GrblResponseParser] Invalid alarm format: {Msg}", msg);
            }
        }

        private void HandleMessage(string msg, GrblState state)
        {
            var content = msg.Trim('[', ']');

            if (content.StartsWith("MSG:", StringComparison.OrdinalIgnoreCase))
            {
                state.FeedbackMessage = content.Substring(4);
                _log?.Information("[GrblResponseParser] Feedback: {Msg}", state.FeedbackMessage);
            }
        }

        #endregion
    }
}
