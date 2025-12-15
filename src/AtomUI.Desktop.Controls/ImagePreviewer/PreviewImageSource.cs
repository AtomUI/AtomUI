using System.Text;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AtomUI.Desktop.Controls;

internal enum PreviewImageSourceType
{
    Svg, Bitmap
}

internal record PreviewImageSource
{
    public PreviewImageSourceType Type { get; }
    public string? SvgContent { get; }
    public IImage? Bitmap { get; }

    private PreviewImageSource(PreviewImageSourceType type, Stream stream)
    {
        Type = type;
        if (type == PreviewImageSourceType.Svg)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, // 自动检测BOM
                bufferSize: 4096);
            var sb     = new StringBuilder();
            var buffer = new char[4096];
            int charsRead;
        
            while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                sb.Append(buffer, 0, charsRead);
            }
            SvgContent = sb.ToString();
        }
        else
        {
            Bitmap = new Bitmap(stream);
        }
    }
    
    public static PreviewImageSource CreateSvg(Stream stream)
        => new PreviewImageSource(PreviewImageSourceType.Svg, stream);
    
    public static PreviewImageSource CreateBitmap(Stream stream)
        => new PreviewImageSource(PreviewImageSourceType.Bitmap, stream);
    
    public bool IsSvg => Type == PreviewImageSourceType.Svg;
    public bool IsBitmap => Type == PreviewImageSourceType.Bitmap;
}