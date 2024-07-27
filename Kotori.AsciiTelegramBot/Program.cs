using Kotori.AsciiTelegramBot.Ascii;
using Kotori.AsciiTelegramBot.Configuration;
using Kotori.AsciiTelegramBot.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

var configFileLoader = new ConfigFileLoader();

var configData = await configFileLoader.LoadFromAsync("Config.json");

if (configData is null)
{
    Console.WriteLine("Failed to load config data");
    return;
}

var characterSet = await File.ReadAllTextAsync("CharacterSet.txt");

var characterBrightnessCalculator = new CharacterBrightnessCalculator();

var brightnessLevels = characterBrightnessCalculator.CalculateMany(
    characterSet, 
    configData.FontFamilyName);

foreach (var (character, brightness) in brightnessLevels)
{
    Console.WriteLine($"{character} - {brightness}");
}

var imageToAsciiConverter = new ImageToAsciiConverter(brightnessLevels);

var telegramClient = new TelegramBotClient(configData.TelegramToken);

telegramClient.OnMessage += async (message, _) =>
{
    if (message.Photo is null)
    {
        return;
    }

    var isGrayscale = message.Caption is not null && message.Caption.StartsWith("g");

    try
    {
        var photo = message.Photo.MaxBy(p => p.FileSize);

        using var inStream = new MemoryStream();
        await telegramClient.GetInfoAndDownloadFileAsync(photo!.FileId, inStream);
        
        var image = await Image.LoadAsync<Rgba32>(new MemoryStream(inStream.ToArray()));

        var asciiImage = await imageToAsciiConverter.ConvertAsync(image, configData.FontFamilyName, isGrayscale);

        using var outStream = new MemoryStream();
        await asciiImage.SaveAsPngAsync(outStream);

        var inputPhoto = InputFile.FromStream(new MemoryStream(outStream.ToArray()), "Result.png");
        await telegramClient.SendDocumentAsync(message.Chat.Id, inputPhoto);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
};

await Task.Delay(-1);