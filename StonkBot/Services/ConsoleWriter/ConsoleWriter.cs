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
    private readonly IConsole _sbAction;
    private readonly IConsole _sbStatus;

    private readonly string[] _danceMoves;

    public ConsoleWriter()
    {
        var win = new Window();
        Console.SetWindowSize(150, 35);

        // StatusBox settings
        var sbXSize = 45;
        var sbYSize = 10;
        var sbXPos = win.WindowWidth - sbXSize;
        var sbYPos = 0;

        // Log windows settings
        var statusWindow = new Window(sbXPos, sbYPos, sbXSize, sbYSize).Concurrent();

        var streamingLogWindow = new Window(0, 0, (win.WindowWidth - sbXSize), sbYSize).Concurrent();
        _streamingLog = streamingLogWindow.OpenBox("StonkBot StreamingData Log");
        var actionLogWindow = new Window(0, sbYSize, win.WindowWidth, win.WindowHeight - sbYSize).Concurrent();
        _actionLog = actionLogWindow.OpenBox("StonkBot ActionRunner Log");
        var statusBox = statusWindow.OpenBox("Action Status");
        _sbAction = new Window(statusBox, 0, 0, (int)Math.Floor(sbXSize * .7), sbYSize);
        _sbStatus = new Window(statusBox, sbXSize - (int)Math.Ceiling(sbXSize * .3), 0, sbXSize / 2, sbYSize);

        _danceMoves = new[] { "(>'-')~  ~('-'<)   ", "^('-')^  ^('-')^   ", "<('-'<)  (>'-')>   ", "^('-')^  ^('-')^   " };
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
                _actionLog.Write(ConsoleColor.White, "[ ");
                _actionLog.Write(ConsoleColor.DarkGreen, $"{DateTime.Now:hh:mm:ss tt}");
                _actionLog.Write(ConsoleColor.White, " ]  ");
                _actionLog.WriteLine(messageColor, message);
                break;
            case TargetLog.StreamingData:
                _streamingLog.CursorLeft = 0;
                _streamingLog.Write(ConsoleColor.White, "[ ");
                _streamingLog.Write(ConsoleColor.DarkGreen, $"{DateTime.Now:hh:mm:ss tt}");
                _streamingLog.Write(ConsoleColor.White, " ]  ");
                _streamingLog.WriteLine(messageColor, message);
                break;
        }
    }


    public void WriteProgress(decimal current, decimal total)
    {
        _actionLog.CursorLeft = 0;
        _actionLog.Write(ConsoleColor.White, "[ ");
        _actionLog.Write(ConsoleColor.DarkGreen, $"{DateTime.Now:hh:mm:ss tt}");
        _actionLog.Write(ConsoleColor.White, " ]  ");
        _actionLog.Write(ConsoleColor.White, $"{(current / total):P}");
    }
    public void WriteProgress(string message, decimal current, decimal total)
    {
        _actionLog.CursorLeft = 0;
        _actionLog.Write(ConsoleColor.White, "[ ");
        _actionLog.Write(ConsoleColor.DarkGreen, $"{DateTime.Now:hh:mm:ss tt}");
        _actionLog.Write(ConsoleColor.White, " ]  ");
        _actionLog.Write(ConsoleColor.White, $"{message}: {(current / total):P}");
    }
    public void WriteProgressComplete(string message)
    {
        _actionLog.CursorLeft = 0;
        _actionLog.Write(ConsoleColor.White, "[ ");
        _actionLog.Write(ConsoleColor.DarkGreen, $"{DateTime.Now:hh:mm:ss tt}");
        _actionLog.Write(ConsoleColor.White, " ]  ");
        _actionLog.WriteLine(ConsoleColor.White, $"{message}");
    }


    public int DanceParty(int i)
    {
        _streamingLog.CursorLeft = 0;
        _streamingLog.Write(ConsoleColor.White, "[ ");
        _streamingLog.Write(ConsoleColor.DarkGreen, $"{DateTime.Now:hh:mm:ss tt}");
        _streamingLog.Write(ConsoleColor.White, " ]  ");
        _streamingLog.Write(ConsoleColor.DarkCyan, $"{_danceMoves[i]}");
        
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
        console.Write(ConsoleColor.White, "[ ");
        console.Write(ConsoleColor.DarkGreen, $"{DateTime.Now:hh:mm:ss tt}");
        console.Write(ConsoleColor.White, " ]  ");
        console.Write(messageColor, message);
    }
}