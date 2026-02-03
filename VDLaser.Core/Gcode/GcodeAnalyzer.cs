using Serilog;
using System.Globalization;
using VDLaser.Core.Gcode;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Interfaces;

public sealed class GcodeAnalyzer : IGcodeAnalyzer
{
    private readonly IGcodeParser _parser;
    private readonly double _rapidFeedRate; // mm/min
    private readonly double _accXMmPerSec2; // mm/s²
    private readonly double _accYMmPerSec2; // mm/s²
    private readonly ILogService _log;

    public GcodeAnalyzer(
        IGcodeParser parser, ILogService log,
        double rapidFeedRateMmPerMin = 3000,
        double accXMmPerSec2 = 300,  // Valeur par défaut GRBL typique
        double accYMmPerSec2 = 300)
    {
        _parser = parser;
        _rapidFeedRate = rapidFeedRateMmPerMin;
        _accXMmPerSec2 = accXMmPerSec2;
        _accYMmPerSec2 = accYMmPerSec2;
        _log = log ?? throw new ArgumentNullException(nameof(log));

    }

    public GcodeStats Analyze(IEnumerable<string> lines)
    {
        bool absoluteMode = true;
        bool metric = true;

        double currentX = 0;
        double currentY = 0;
        double feedRate = 0;

        double minX = 0, maxX = 0;
        double minY = 0, maxY = 0;

        bool hasBounds = false;
        bool usesLaser = false;

        double totalSeconds = 0;
        int lineCount = 0;

        foreach (var line in lines)
        {
            lineCount++;

            var cmd = _parser.Parse(line);
            if (cmd.IsEmpty)
                continue;

            if (cmd.G == 90) absoluteMode = true;
            if (cmd.G == 91) absoluteMode = false;
            if (cmd.G == 21) metric = true;
            if (cmd.G == 20) metric = false;

            if (cmd.M == 3 || cmd.M == 4)
                usesLaser = true;

            if (cmd.F.HasValue)
                feedRate = cmd.F.Value;

            if (cmd.G is 0 or 1 or 2 or 3)
            {
                double targetX = currentX;
                double targetY = currentY;

                if (cmd.X.HasValue)
                    targetX = absoluteMode ? cmd.X.Value : currentX + cmd.X.Value;

                if (cmd.Y.HasValue)
                    targetY = absoluteMode ? cmd.Y.Value : currentY + cmd.Y.Value;

                // Calcul de la distance réelle (linéaire ou arc)
                double distance = CalculateDistance(cmd, currentX, currentY, targetX, targetY);

                if (distance > 0)
                {
                    double speedMmPerMin = cmd.G == 0 ? _rapidFeedRate : (feedRate > 0 ? feedRate : _rapidFeedRate);

                    // Accélération effective (min des axes impliqués, en mm/min²)
                    double dx = targetX - currentX;
                    double dy = targetY - currentY;
                    double accMmPerSec2 = (Math.Abs(dx) > 0 && Math.Abs(dy) > 0) ? Math.Min(_accXMmPerSec2, _accYMmPerSec2) :
                                        (Math.Abs(dx) > 0 ? _accXMmPerSec2 : _accYMmPerSec2);
                    double accMmPerMin2 = accMmPerSec2 * 3600;  // Conversion en mm/min²

                    // Modèle trapézoïdal
                    double d_acc = (speedMmPerMin * speedMmPerMin) / (2 * accMmPerMin2);
                    double timeMin;
                    if (distance >= 2 * d_acc)
                    {
                        // Phase acc + constante + dec
                        double t_acc_dec = (speedMmPerMin / accMmPerMin2) * 2;
                        double d_const = distance - 2 * d_acc;
                        double t_const = d_const / speedMmPerMin;
                        timeMin = t_acc_dec + t_const;
                    }
                    else
                    {
                        // Pas de phase constante (triangle)
                        timeMin = 2 * Math.Sqrt(distance / accMmPerMin2);
                    }

                    totalSeconds += timeMin * 60;  // Conversion en secondes
                    //_log.Information("Move time estimated: {Time}s for distance {Dist}mm at {Speed}mm/min with acc {Acc}mm/s²", timeMin * 60, distance, speedMmPerMin, accMmPerSec2);
                }

                // Mise à jour des bounds (inchangé)
                if (!hasBounds)
                {
                    minX = maxX = targetX;
                    minY = maxY = targetY;
                    hasBounds = true;
                }
                else
                {
                    minX = Math.Min(minX, targetX);
                    maxX = Math.Max(maxX, targetX);
                    minY = Math.Min(minY, targetY);
                    maxY = Math.Max(maxY, targetY);
                }

                currentX = targetX;
                currentY = targetY;
                //_log.Information("Bounds updated: MinX={0}, MaxX={1}", minX, maxX);
            }
        }

        return new GcodeStats
        {
            LineCount = lineCount,
            UsesLaser = usesLaser,
            IsMetric = metric,
            MinX = minX,
            MaxX = maxX,
            MinY = minY,
            MaxY = maxY,
            EstimatedTime = TimeSpan.FromSeconds(totalSeconds)
        };
    }
    // Nouvelle méthode : Calcul de distance réelle (linéaire ou arc)
    private double CalculateDistance(GcodeCommand cmd, double currentX, double currentY, double targetX, double targetY)
    {
        if (cmd.G is 2 or 3 && (cmd.I.HasValue || cmd.J.HasValue))  // Arc (assume GcodeCommand a I et J)
        {
            double centerX = currentX + (cmd.I ?? 0);
            double centerY = currentY + (cmd.J ?? 0);

            double radius = Math.Sqrt(Math.Pow(currentX - centerX, 2) + Math.Pow(currentY - centerY, 2));

            double startAngle = Math.Atan2(currentY - centerY, currentX - centerX);
            double endAngle = Math.Atan2(targetY - centerY, targetX - centerX);

            double angle = endAngle - startAngle;
            if (cmd.G == 3) angle = -angle;  // CW vs CCW (ajuster si nécessaire)

            if (angle < 0) angle += 2 * Math.PI;
            if (angle > 2 * Math.PI) angle -= 2 * Math.PI;

            return Math.Abs(radius * angle);  // Longueur d'arc
        }
        else
        {
            double dx = targetX - currentX;
            double dy = targetY - currentY;
            return Math.Sqrt(dx * dx + dy * dy);  // Linéaire
        }
    }
}
