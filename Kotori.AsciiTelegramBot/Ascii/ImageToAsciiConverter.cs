using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kotori.AsciiTelegramBot.Ascii;

public class ImageToAsciiConverter
{
    private readonly Dictionary<char, float> _characterBrightness;

    public ImageToAsciiConverter(Dictionary<char, float> characterBrightness)
    {
        _characterBrightness = characterBrightness;
    }

    public async Task<Image<Rgba32>?> ConvertAsync(Image<Rgba32> image, string fontFamilyName, bool grayscale = false)
    {
        var largestDimension = Math.Max(image.Width, image.Height);
        
        var groupSizeX = Math.Clamp(largestDimension / 100, 8, 32);
        var groupSizeY = Math.Clamp(largestDimension / 100, 8, 32);
        var maxGroupBrightness = groupSizeX * groupSizeY * 256 * 3;

        if (!SystemFonts.TryGet(fontFamilyName, out var fontFamily))
        {
            return null;
        }

        var font = fontFamily.CreateFont((int)(groupSizeX * 1.2));
        
        var clearColor = new Color(Rgba32.ParseHex("#000"));

        var outImage = new Image<Rgba32>(image.Width, image.Height);
        
        outImage.Mutate(x => x.Clear(clearColor));
        
        for (var startY = 0; startY < image.Height; startY += groupSizeY)
        {
            for (var startX = 0; startX < image.Width; startX += groupSizeX)
            {
                var groupR = 0;
                var groupG = 0;
                var groupB = 0;

                var size = 0;

                for (var x = startX; x < startX + groupSizeX; x++)
                {
                    for (var y = startY; y < startY + groupSizeY; y++)
                    {
                        if (x >= image.Width || y >= image.Height)
                        {
                            continue;
                        }

                        var pixel = image[x, y];

                        groupR += pixel.R;
                        groupG += pixel.G;
                        groupB += pixel.B;

                        size++;
                    }
                }

                var brightness = (float)(groupR + groupG + groupB) / maxGroupBrightness;
                
                var maxChannel = size;
                
                var averageR = groupR / maxChannel / 255f;
                var averageG = groupG / maxChannel / 255f;
                var averageB = groupB / maxChannel / 255f;
                
                foreach (var (character, characterBrightness) in _characterBrightness)
                {
                    if (brightness < characterBrightness)
                    {
                        continue;
                    }
                    
                    var fontColor = !grayscale 
                        ? new Color(new Rgba32(averageR, averageG, averageB))
                        : new Color(Rgba32.ParseHex("#ffffff"));
                    
                    var point = new PointF(startX, startY);
                    
                    outImage.Mutate(x => x.DrawText(
                        character.ToString(),
                        font,
                        fontColor,
                        point));

                    break;
                }
            }
        }

        return outImage;
    }
}