using Microsoft.Xna.Framework.Graphics;

namespace Mythril.GameLogic.AI;

public class ScreenshotUtility(GraphicsDevice graphicsDevice)
{
    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;

    public string TakeScreenshot(string filename, bool inlineBase64 = false)
    {
        var width = _graphicsDevice.PresentationParameters.BackBufferWidth;
        var height = _graphicsDevice.PresentationParameters.BackBufferHeight;

        using (var renderTarget = new RenderTarget2D(_graphicsDevice, width, height))
        {
            _graphicsDevice.SetRenderTarget(renderTarget);
            // Draw the scene here if this utility was responsible for rendering
            // For now, assume the scene is already drawn to the back buffer
            _graphicsDevice.SetRenderTarget(null);

            // Get the data from the render target
            var colors = new Color[width * height];
            renderTarget.GetData(colors);

            // Create a Texture2D from the data
            var texture = new Texture2D(_graphicsDevice, width, height);
            texture.SetData(colors);

            if (inlineBase64)
            {
                using (var stream = new MemoryStream())
                {
                    texture.SaveAsPng(stream, width, height);
                    var imageBytes = stream.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            }
            else
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                using (var stream = File.OpenWrite(filePath))
                {
                    texture.SaveAsPng(stream, width, height);
                }
                return filePath;
            }
        }
    }
}
