using Newtonsoft.Json;

namespace Kotori.AsciiTelegramBot.Configuration;

public class ConfigFileLoader
{
    public async Task<ConfigData?> LoadFromAsync(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return null;
        }

        var contents = await File.ReadAllTextAsync(fileName);

        var configData = JsonConvert.DeserializeObject<ConfigData>(contents);

        return configData;
    }
}