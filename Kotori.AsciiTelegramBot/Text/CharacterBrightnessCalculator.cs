using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kotori.AsciiTelegramBot.Text;

public class CharacterBrightnessCalculator
{
    public Dictionary<char, float> CalculateMany(string characters, string fontFamilyName)
    {
        const int width = 64;
        const int height = 64;
        const int totalPixelCont = width * height;
        
        using var tmpImage = new Image<Rgba32>(width, height);
        
        if (!SystemFonts.TryGet(fontFamilyName, out var fontFamily))
        {
            Console.WriteLine("Font not found");
            return new Dictionary<char, float>();
        }

        var font = fontFamily.CreateFont(width);

        var clearColor = new Color(Rgba32.ParseHex("#000"));

        var fontRgba = Rgba32.ParseHex("#ffffff");
        var fontColor = new Color(fontRgba);
        var point = new PointF(0, 0);

        var brightnessLevels = new Dictionary<char, float>();

        foreach (var character in characters)
        {
            tmpImage.Mutate(x => x.Clear(clearColor));

            tmpImage.Mutate(x => x.DrawText(
                character.ToString(),
                font,
                fontColor,
                point));
            
            var characterBrightness = CalculateImageBrightness(tmpImage, totalPixelCont, fontRgba);

            brightnessLevels[character] = characterBrightness;
        }

        var maxBrightness = brightnessLevels.MaxBy(x => x.Value).Value;
        
        foreach (var (character, brightness) in brightnessLevels)
        {
            brightnessLevels[character] = brightness / maxBrightness;
        }

        return brightnessLevels
            .OrderByDescending(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private float CalculateImageBrightness(Image<Rgba32> image, int totalPixelCont, Rgba32 testColor)
    {
        var onPixelCount = 0;

        for (var x = 0; x < image.Width; x++)
        {
            for (var y = 0; y < image.Height; y++)
            {
                var pixel = image[x, y];

                if (pixel == testColor)
                {
                    onPixelCount++;
                }
            }
        }

        var totalBrightness = (float)onPixelCount / totalPixelCont;

        totalBrightness += Random.Shared.NextSingle() * 0.05f;
        
        return totalBrightness;
    }
}