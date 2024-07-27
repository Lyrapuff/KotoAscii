namespace Kotori.AsciiTelegramBot.Configuration;

public record ConfigData
{
    public string TelegramToken { get; set; }
    public string FontFamilyName { get; set; }
}