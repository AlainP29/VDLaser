using VDLaser.Core.Codes;
using VDLaser.Core.Gcode.Models;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;

namespace VDLaser.Core.Grbl.Parsers
{
    /// <summary>
    /// Generic GRBL response messages.
    /// Handles: ok, error.
    /// </summary>
    public class GrblResponseParser : IGrblSubParser
    {
        private readonly ILogService _log;

        private readonly ErrorCodes _errors = new();
        public string Name => "GrblResponseParser";
        public GrblResponseParser()
        {
            
        }
        public GrblResponseParser(ILogService log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _log.Information("[GCodeResponseParser] Initialised");
        }
        public bool CanParse(string line)
        {
            line = line.Trim().ToLowerInvariant();

            return
                line.StartsWith("ok") ||
                line.StartsWith("error:");
        }
        public void Parse(string line, GcodeState state)
        {
            line = line.Trim();

            if (line.StartsWith("ok"))
            {
                HandleOk(state);
                return;
            }

            if (line.StartsWith("error:"))
            {
                HandleError(line, state);
                return;
            }
        }

        private void HandleOk(GcodeState state)
        {
            state.Error = null;
            state.ErrorMessage = string.Empty;
            _log.Information("[GCodeResponseParser] Ok");
        }

        private void HandleError(string line, GcodeState state)
        {
            var codePart = line.Split(':')[1].Trim();
            _log.Information("[GCodeResponseParser] error format {err}", line);

            if (int.TryParse(codePart, out int code))
            {
                var err = GetError(code, state.IsGrbl11);
                state.Error = err;
                state.ErrorMessage = err?.Description ?? $"Unknown error {code}";
                state.ErrorHistory.Add(err);
            }
            else
            {
                state.ErrorMessage = $"Invalid GRBL error format: {line}";
            }
        }

        // -----------------------------------------------------------
        // Error translation (no factory)
        // -----------------------------------------------------------
        private GrblError GetError(int code, bool is11)
        {
            var def = is11
                ? _errors.ErrorDict11.GetValueOrDefault(code)
                : _errors.ErrorDict09.GetValueOrDefault(code);

            if (def == null)
                return new GrblError(code, code.ToString(), "Unknown GRBL error");

            return new GrblError(code, def.Message, def.Description);
            _log.Information("[GCodeResponseParser] Get error");

        }

        public void Parse(string line, GrblState state)
        {
        }

        public void Parse(string line, GrblInfo info)
        {
        }
    }
}
