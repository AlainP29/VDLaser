using System.Globalization;
using System.Text.RegularExpressions;
using VDLaser.Core.Codes;
using VDLaser.Core.Grbl.Interfaces;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Interfaces;

namespace VDLaser.Core.Grbl.Parsers
{
    /// <summary>
    /// Parseur des paramètres GRBL ($x=value).
    /// Compatible GRBL 0.9 / 1.1.
    /// Produit un GrblSetting complet directement utilisable.
    /// </summary>
    public class GrblSettingsParser : IGrblSubParser
    {
        private readonly ILogService _log;

        private readonly GrblSettingCodes _codeDb = new();

        /// <summary>
        /// Regex officielle pour capturer :  $100=80.000 (step/mm)
        /// </summary>
        private static readonly Regex SettingRegex = new(
            @"^\s*\$(?<id>\d+)\s*=\s*(?<value>[-+]?\d*\.?\d+)\s*(\((?<comment>.*)\))?\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public string Name => "GrblSettingParser";
        public GrblSettingsParser(ILogService log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public GrblSettingsParser()
        {
        }

        public bool CanParse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            return SettingRegex.IsMatch(line);
        }

        public void Parse(string line, GrblState state)
        {
            _log.Debug("[GrblSettingParser] Settings {set}",line);

            var setting = ParseInternal(line, state.IsGrbl11);
            if (setting == null)
                return;

            // stocker dans la map des settings
            state.Settings[setting.Id] = setting;

            // marquer le dernier setting modifié
            state.LastParsedSetting = setting;

            // indiquer un message simple de confirmation
            state.LastMessage = $"Setting parsed: {setting.Name} = {setting.Value}";
        }

        // ---------------------------------------------------------
        // Parse interne → retourne un GrblSetting complet
        // ---------------------------------------------------------
        public GrblSetting? ParseInternal(string line, bool isGrbl11)
        {
            var match = SettingRegex.Match(line);
            if (!match.Success)
                return null;

            int id = int.Parse(match.Groups["id"].Value, CultureInfo.InvariantCulture);
            double value = double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);

            string? comment = match.Groups["comment"].Success
                ? match.Groups["comment"].Value.Trim()
                : null;

            // récupère l'info statique
            var baseInfo = _codeDb.GetSetting(id, isGrbl11);

            // si on a l'info complète → on la clone et on y injecte la valeur
            if (baseInfo != null)
            {
                baseInfo.Value = value;
                baseInfo.Comment = comment;
                return baseInfo;
            }

            // setting inconnu → fabriquer un objet minimal
            return new GrblSetting
            {
                Id = id,
                Name = $"${id}",
                Description = "Unknown setting",
                Unit = "",
                Value = value,
                Comment = comment
            };
        }

        public void Parse(string line, GrblInfo state)
        {
        }
    }
}
