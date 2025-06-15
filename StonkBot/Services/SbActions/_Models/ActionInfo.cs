using StonkBot.Services.SbActions._Enums;

namespace StonkBot.Services.SbActions._Models;

public class ActionInfo
{
    public string Method { get; set; }
    public dynamic StartTime { get; set; }
    public dynamic EndTime { get; set; }
    public dynamic Interval { get; set; }

    public ActionInfo(string method, string startTime, string endTime, ActionInterval interval)
    {
        var todayDate = DateTime.Today.ToString("MM/dd/yyyy");
        Method = method;
        StartTime = DateTime.Parse($"{todayDate} {startTime}");
        EndTime = DateTime.Parse($"{todayDate} {endTime}");
        Interval = TimeSpan.FromSeconds((int)interval);
    }

    public void SetNextRuntime()
    {
        while (this.StartTime <= DateTime.Now)
            this.StartTime += this.Interval;
    }
}