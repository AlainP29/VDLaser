using System.Globalization;
using System.Text.RegularExpressions;
using VDLaser.Core.Gcode.Interfaces;

namespace VDLaser.Core.Gcode.Parsers
{
    //Responsable uniquement de lire une ligne et renvoyer une structure
    

    public sealed class GcodeParser : IGcodeParser
    {
        private static readonly Regex TokenRegex =
            new(@"([A-Z])([-+]?[0-9]*\.?[0-9]+)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public GcodeCommand Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return new GcodeCommand();

            line = StripComments(line).Trim();

            if (line.Length == 0)
                return new GcodeCommand();

            var cmd = new GcodeCommand();

            foreach (Match match in TokenRegex.Matches(line))
            {
                char letter = char.ToUpperInvariant(match.Groups[1].Value[0]);
                string valueText = match.Groups[2].Value;

                if (!double.TryParse(
                        valueText,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double value))
                    continue;

                cmd = letter switch
                {
                    'G' => cmd with { G = (int)value },
                    'M' => cmd with { M = (int)value },
                    'X' => cmd with { X = value },
                    'Y' => cmd with { Y = value },
                    'Z' => cmd with { Z = value },
                    'I' => cmd with { I = value },
                    'J' => cmd with { J = value },
                    'F' => cmd with { F = value },
                    'S' => cmd with { S = value },
                    _ => cmd
                };
            }

            return cmd;
        }

        private static string StripComments(string line)
        {
            line = Regex.Replace(line, @"\(.*?\)", string.Empty);

            int index = line.IndexOf(';');
            if (index >= 0)
                line = line[..index];

            return line;
        }
    }


}
