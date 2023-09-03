using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.Services.ConsoleWriter.Models;

public class LogEntry
{
    public MessageSeverity Severity { get; set; }
    public TargetLog TargetedLog { get; set; }
    public string Message { get; set; }

    public LogEntry(MessageSeverity severity, TargetLog? targetLog, string message)
    {
        Severity = severity;
        TargetedLog = targetLog ?? TargetLog.ActionRunner;
        Message = message;
    }
}