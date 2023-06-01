using Humanizer;

namespace Aiursoft.BaGet.Web
{
    public static class RazorExtensions
    {
        public static string ToMetric(this long value)
        {
            return ((double) value).ToMetric();
        }
    }
}
