using SkiaSharp;
using RMSProjectAPI;
using QRCoder;
using static QRCoder.QRCodeGenerator;

public class QRCodeService
{
    public byte[] GenerateQRCode(string url)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrCode = qrGenerator.CreateQrCode(url, ECCLevel.Q);
        int qrSize = 256;

        using (var surface = SKSurface.Create(new SKImageInfo(qrSize, qrSize)))
        {
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            using (var paint = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                float scale = qrSize / (float)qrCode.ModuleMatrix.Count;
                for (int y = 0; y < qrCode.ModuleMatrix.Count; y++)
                {
                    for (int x = 0; x < qrCode.ModuleMatrix[y].Count; x++)
                    {
                        if (qrCode.ModuleMatrix[y][x])
                        {
                            canvas.DrawRect(x * scale, y * scale, scale, scale, paint);
                        }
                    }
                }
            }

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }
    }
}
