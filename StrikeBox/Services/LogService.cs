using System.Diagnostics;
using System.IO;

namespace StrikeBox.Services;

public sealed class LogService
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "StrikeBox", "Logs");

    private readonly string _logFile;
    private bool _dirEnsured;

    public LogService()
    {
        _logFile = Path.Combine(LogDir, $"strikebox_{DateTime.Now:yyyyMMdd}.log");
    }

    public void Info(string message) => Log("INFO", message);
    public void Warning(string message) => Log("WARN", message);
    public void Error(string message) => Log("ERROR", message);

    private void Log(string level, string message)
    {
        try
        {
            if (!_dirEnsured)
            {
                Directory.CreateDirectory(LogDir);
                _dirEnsured = true;
            }

            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(_logFile, entry + Environment.NewLine);
            Debug.WriteLine(entry);
        }
        catch
        {
            Debug.WriteLine($"[{level}] {message}");
        }
    }
}
