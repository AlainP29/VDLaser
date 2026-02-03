using System.IO;
using VDLaser.Core.Gcode.Interfaces;
using VDLaser.Core.Models;

namespace VDLaser.Core.Gcode.Services;

public sealed class GcodeFileService : IGcodeFileService
{
    private readonly IGcodeParser _parser;
    private readonly IGcodeAnalyzer _analyzer;

    public GcodeFileService(
        IGcodeParser parser,
        IGcodeAnalyzer analyzer)
    {
        _parser = parser;
        _analyzer = analyzer;
    }

    public async Task<GcodeFileResult> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        var rawLines = await File.ReadAllLinesAsync(filePath);

        var parsed = rawLines
            .Select(l => _parser.Parse(l))
            .ToList();

        var stats = _analyzer.Analyze(rawLines);

        return new GcodeFileResult
        {
            RawLines = rawLines,
            ParsedCommands = parsed,
            Stats = stats
        };
    }
}
