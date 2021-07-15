using System.Collections.Generic;

namespace LatexClient
{
    public enum ImageFormats
    {
        Jpeg,
        Png,
        Tiff
    }

    public static class ImageFormatsExtensions
    {
        private static readonly Dictionary<ImageFormats, string> _formats = new Dictionary<ImageFormats, string>
            {
                {ImageFormats.Jpeg, "jpeg"},
                {ImageFormats.Png, "png"},
                {ImageFormats.Tiff, "tiff"}
            };

        public static string ToFormatString(this ImageFormats i)
        {
            return _formats[i];
        }

    }
}