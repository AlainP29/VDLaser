using System.Text.RegularExpressions;
using VDLaser.Core.Codes;
using VDLaser.Core.Console;
using VDLaser.Core.Interfaces;

namespace VDLaser.Core.Services
{
    /// <summary>
    /// Console parser service to interpret GRBL console lines.
    /// </summary>
    public class ConsoleParserService : IConsoleParserService
    {
        #region Fields
        private readonly ErrorCodes _errorCodes = new();
        private readonly AlarmCodes _alarmCodes = new();

        private ConsoleItem? _pendingCommand;
        public ConsoleItem? CurrentPendingCommand => _pendingCommand;

        private ConsoleSource _source;
        private bool _isGrbl11 = true;

        private static readonly Regex ErrorRegex = new(@"^error:(\d+)", RegexOptions.IgnoreCase);
        private static readonly Regex AlarmRegex = new(@"^ALARM:(\d+)", RegexOptions.IgnoreCase);
        private static readonly Regex StatusRegex = new(@"^<(.+)>$", RegexOptions.IgnoreCase);
        private static readonly Regex MsgRegex = new(@"\[MSG:(.+)\]", RegexOptions.IgnoreCase);
        #endregion

        #region Constructor

        public ConsoleItem ParseRaw(string rawLine)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Parse a structured console line into a ConsoleItem.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public ConsoleItem? ParseStructured(string line)
        {
            line = line.Trim();

            if (line.StartsWith(">>")) return null;

            // 1. Ignore the commands sent back by GRBL
            if (IsCommand(line)) return null;

            // 2. OK → complete the current order
            if (line.Equals("ok", StringComparison.OrdinalIgnoreCase))
            {
                if (_pendingCommand != null)
                {
                    _pendingCommand.Response = "ok";
                    _pendingCommand.Type = ConsoleMessageType.Response;

                    var completed = _pendingCommand;
                    _pendingCommand = null;
                    return completed;
                }

                return null;
            }

            // 3. error:X
            var err = ErrorRegex.Match(line);
            if (err.Success)
            {
                int code = int.Parse(err.Groups[1].Value);
                var error = _errorCodes.GetError(code, _isGrbl11);

                if (_pendingCommand != null)
                { 
                    _pendingCommand.Response = line; 
                    _pendingCommand.Code = code; 
                    _pendingCommand.Description = error?.Description; 
                    _pendingCommand.Type = ConsoleMessageType.Error; 
                    var completed = _pendingCommand; 
                    _pendingCommand = null; 
                    return completed; 
                }

                // orphan error
                return new ConsoleItem 
                  { 
                      Response = line, 
                      Code = code, 
                      Description = error?.Description, 
                      Type = ConsoleMessageType.Error,
                  };
            }

            // 4. ALARM:X
            var alarm = AlarmRegex.Match(line);
            if (alarm.Success)
            {
                int code = int.Parse(alarm.Groups[1].Value);
                var alarmInfo = _alarmCodes.GetAlarm(code, _isGrbl11);

                if (_pendingCommand != null) 
                { 
                    _pendingCommand.Response = line; 
                    _pendingCommand.Code = code; 
                    //_pendingCommand.Description = alarmInfo?.Description; 
                    _pendingCommand.Severity = alarmInfo?.Severity; 
                    _pendingCommand.Type = ConsoleMessageType.Alarm; 
                    
                    var completed = _pendingCommand; 
                    _pendingCommand = null; 
                    return completed; 
                }

                return new ConsoleItem
                {
                    Response = line,
                    Code = code,
                    //Description = alarmInfo?.Description,
                    Severity = alarmInfo?.Severity,
                    Type = ConsoleMessageType.Alarm,
                };
            }

            // 5. Status <Idle|...>
            if (StatusRegex.IsMatch(line))
            {
                return new ConsoleItem
                {
                    Response = line,
                    Type = ConsoleMessageType.Status,
                };
            }

            // 6. [MSG:...]
            var msg = MsgRegex.Match(line);
            if (msg.Success)
            {
                return new ConsoleItem
                {
                    Response = msg.Groups[1].Value,
                    Type = ConsoleMessageType.Info,
                };
            }

            // 7. Raw line
            return new ConsoleItem
            {
                Response = line,
                Type = ConsoleMessageType.Raw,
            };
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Check if the line is a command sent to the console.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private bool IsCommand(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            return line.StartsWith("$") ||
                   line.StartsWith("G") ||
                   line.StartsWith("M") ||
                   line.StartsWith("H") ||
                   line.StartsWith("S");
        }

        /// <summary>
        /// API to begin tracking a command sent to the console.
        /// </summary>
        /// <param name="command"></param>
        public void BeginCommand(string command)
        {
            string cmd = command;
            string? comment = null;

            // 1) Extract the comments in parentheses ( )
            /*var parenStart = cmd.IndexOf('(');
            if (parenStart >= 0)
            {
                var parenEnd = cmd.IndexOf(')', parenStart + 1);
                if (parenEnd > parenStart)
                {
                    string inside = cmd.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();
                    comment = inside;

                    cmd = cmd.Remove(parenStart, parenEnd - parenStart + 1).Trim();
                }
            }*/

            // 2) Extract comments after ';'
            var semicolonIndex = cmd.IndexOf(';');
            if (semicolonIndex >= 0)
            {
                string after = cmd[(semicolonIndex + 1)..].Trim();
                comment = comment == null ? after : $"{comment} ; {after}";

                cmd = cmd[..semicolonIndex].Trim();
            }
            // 3) If the order is empty but there is a comment → it's a pure comment
            if (string.IsNullOrWhiteSpace(cmd) && comment != null)
            {
                _pendingCommand = new ConsoleItem
                {
                    Command = string.Empty,
                    Description = comment,
                    Type = ConsoleMessageType.Command
                };
                return;
            }

            // 4) Regular order
            _pendingCommand = new ConsoleItem
            {
                Command = cmd,
                Description = comment,
                Type = ConsoleMessageType.Command,
                Source=ConsoleSource.Manual
            };
        }

        #endregion
    }
}
