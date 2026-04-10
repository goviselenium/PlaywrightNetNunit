using Microsoft.Playwright;
using Serilog;

namespace PlaywrightFramework.Utils
{
    /// <summary>
    /// Utility for handling video recordings
    /// </summary>
    public static class VideoUtil
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(VideoUtil));

        /// <summary>
        /// Get the path to the video file for a page context
        /// </summary>
        public static async Task<string?> GetVideoPathAsync(IPage page)
        {
            try
            {
                var context = page.Context;
                if (context == null)
                    return null;

                var video = page.Video;
                if (video == null)
                    return null;

                // The video file will be available after the page is closed
                var path = await video.PathAsync();
                Log.Information("Video saved at: {path}", path);
                return path;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not get video path");
                return null;
            }
        }

        /// <summary>
        /// Save the video explicitly
        /// </summary>
        public static async Task<string?> SaveVideoAsync(IPage page, string outputPath)
        {
            try
            {
                var video = page.Video;
                if (video == null)
                    return null;

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
                await video.SaveAsAsync(outputPath);
                Log.Information("Video saved to: {path}", outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to save video to: {path}", outputPath);
                return null;
            }
        }
    }
}
