using StonkBot.Services.DiscordService.Enums;

namespace StonkBot.Services.DiscordService.Models;

public class DiscordTableMessage
{
    public DiscordChannel Channel { get; set; }
    public string Title { get; set; }
    public List<List<string>> Data { get; set; }

    public DiscordTableMessage(DiscordChannel channel, string title, string headerRow)
    {
        var headers = headerRow.Split(',')
            .ToList();

        Channel = channel;
        Title = title;
        Data = new List<List<string>> { headers };
    }
}