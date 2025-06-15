using System.Runtime.CompilerServices;
using Konsole;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.ConsoleWriter.Models;
using StonkBot.Services.SbActions._Models;

namespace StonkBot.Services.ConsoleWriter;

public interface IConsoleWriter
{
    void UpdateStatus(ActionSchedule actionSchedule);

    void WriteLog(LogEntry logEntry);
    void WriteLog(MessageSeverity severity, string message);
    void WriteLog(MessageSeverity severity, TargetLog targetLog, string message);

    void WriteProgress(decimal current, decimal total);
    void WriteProgress(string message, decimal current, decimal total);
    void WriteProgressComplete(string message);

    int DanceParty(int i);

    void ReplaceLastLog(MessageSeverity severity, TargetLog targetLog, string message);
}

public class ConsoleWriter : IConsoleWriter
{
    private readonly IConsole _actionLog;
    private readonly IConsole _streamingLog;
    private readonly Window _sbAction;
    private readonly Window _sbStatus;
    
    public ConsoleWriter()
    {
        Console.SetWindowSize(150, 40);

        // StatusBox settings
        const int statusWindowWidth = 45;
        const int bannerHeight = 13;
        
        var win = new Window();
        var statusWindowPosition = win.WindowWidth - statusWindowWidth;

        // Log windows settings
        var streamingLogWindow = new Window(x:0, y:0, width:(win.WindowWidth - statusWindowWidth), height:bannerHeight).Concurrent();
        _streamingLog = streamingLogWindow.OpenBox("StonkBot StreamingData Log");

        var actionLogWindow = new Window(x: 0, y: bannerHeight, width: win.WindowWidth, height: win.WindowHeight - bannerHeight).Concurrent();
        _actionLog = actionLogWindow.OpenBox("StonkBot ActionRunner Log");

        var statusWindow = new Window(x: statusWindowPosition, y: 0, width: statusWindowWidth, height: bannerHeight).Concurrent();
        var statusBox = statusWindow.OpenBox("Action Status");
        _sbAction = new Window(statusBox, x:0, y:0, width:(int)Math.Floor(statusWindowWidth * .7), height:bannerHeight);
        _sbStatus = new Window(statusBox, x:statusWindowWidth - (int)Math.Ceiling(statusWindowWidth * .3), y:0, width:statusWindowWidth / 2, height:bannerHeight);
    }

    public void UpdateStatus(ActionSchedule actionSchedule)
    {
        var initialTimes = new ActionSchedule();
        _sbAction.Clear();
        _sbStatus.Clear();
        _sbAction.WriteLine("Action:");
        _sbStatus.WriteLine("NextRun:");

        foreach (var action in actionSchedule.ActionInfos)
        {
            var now = DateTime.Now;
            var startTime = Convert.ToDateTime(action.StartTime);

            var matchAction = initialTimes.ActionInfos
                .First(x => x.Method == action.Method);
            var matchStartTime = Convert.ToDateTime(matchAction.StartTime);
            var matchEndTime = Convert.ToDateTime(matchAction.EndTime);

            var conColor = now > matchStartTime && now < matchEndTime
                ? ConsoleColor.Gray
                : ConsoleColor.DarkGray;
            _sbAction.WriteLine(conColor, action.Method);
            _sbStatus.WriteLine(conColor, startTime.ToString("hh:mm:ss tt"));
        }
    }
    
    public void WriteLog(LogEntry logEntry)
    {
        WriteLog(logEntry.Severity, logEntry.TargetedLog, logEntry.Message);
    }
    public void WriteLog(MessageSeverity severity, string message)
    {
        WriteLog(severity, TargetLog.ActionRunner, message);
    }
    public void WriteLog(MessageSeverity severity, TargetLog targetLog, string message)
    {
        var messageColor = severity switch
        {
            MessageSeverity.Section => ConsoleColor.DarkCyan,
            MessageSeverity.Debug => ConsoleColor.DarkGray,
            MessageSeverity.Info => ConsoleColor.DarkGray,
            MessageSeverity.Warning => ConsoleColor.DarkYellow,
            MessageSeverity.Error => ConsoleColor.DarkRed,
            MessageSeverity.Stats => ConsoleColor.White,
            _ => ConsoleColor.DarkGray
        };

        switch (targetLog)
        {
            case TargetLog.ActionRunner:
                _actionLog.CursorLeft = 0;
                TimeStamp(_actionLog);
                _actionLog.WriteLine(messageColor, message);
                break;
            case TargetLog.StreamingData:
                _streamingLog.CursorLeft = 0;
                TimeStamp(_streamingLog);
                _streamingLog.WriteLine(messageColor, message);
                break;
        }

        var filePath = "E:\\projects\\stonkBot\\Data\\sbLog.txt";
        using var logger = new StreamWriter(filePath, true);
        logger.WriteLine($"[ {DateTime.Now:hh:mm:ss tt} ]  {message}");
    }
    
    public void WriteProgress(decimal current, decimal total)
    {
        _actionLog.CursorLeft = 0;
        TimeStamp(_actionLog);
        _actionLog.Write(ConsoleColor.White, $"{(current / total):P}");
    }
    public void WriteProgress(string message, decimal current, decimal total)
    {
        _actionLog.CursorLeft = 0;
        TimeStamp(_actionLog);
        _actionLog.Write(ConsoleColor.White, $"{message}: {(current / total):P}");
    }
    public void WriteProgressComplete(string message)
    {
        _actionLog.CursorLeft = 0;
        TimeStamp(_actionLog);
        _actionLog.WriteLine(ConsoleColor.White, $"{message}");
    }
    
    public int DanceParty(int i)
    {
        _streamingLog.CursorLeft = 0;
        TimeStamp(_streamingLog);
        _streamingLog.Write(ConsoleColor.DarkCyan, $"{Dance.Moves[i]}");
        
        i++;
        return i > 3 ? 0 : i;
    }

    public void ReplaceLastLog(MessageSeverity severity, TargetLog targetLog, string message)
    {
        var messageColor = severity switch
        {
            MessageSeverity.Section => ConsoleColor.DarkCyan,
            MessageSeverity.Debug => ConsoleColor.DarkGray,
            MessageSeverity.Info => ConsoleColor.DarkGray,
            MessageSeverity.Warning => ConsoleColor.DarkYellow,
            MessageSeverity.Error => ConsoleColor.DarkRed,
            MessageSeverity.Stats => ConsoleColor.White,
            _ => ConsoleColor.DarkGray
        };

        var console = targetLog switch
        {
            TargetLog.StreamingData => _streamingLog,
            TargetLog.ActionRunner => _actionLog,
            _ => _actionLog
        };

        console.CursorLeft = 0;
        TimeStamp(console);
        console.Write(messageColor, message);
    }

    public void TimeStamp(IConsole x)
    {
        x.Write(ConsoleColor.White, "[ ");
        x.Write(ConsoleColor.DarkGreen, $"{DateTime.Now:hh:mm:ss tt}");
        x.Write(ConsoleColor.White, " ]  ");
    }
}