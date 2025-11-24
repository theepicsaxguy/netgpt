
namespace NetGPT.Infrastructure.Tools
{
    using System.ComponentModel;
    using System.Threading.Tasks;

    public sealed class FileProcessingToolPlugin
    {
        [Description("Extract text from a PDF file")]
        public static async Task<string> ExtractPdfText(
            [Description("URL or path to PDF file")] string fileUrl)
        {
            await Task.Delay(100);
            return "Extracted text from PDF...";
        }

        [Description("Analyze an image and describe its contents")]
        public static async Task<string> AnalyzeImage(
            [Description("URL to image")] string imageUrl)
        {
            await Task.Delay(100);
            return "Image analysis: The image contains...";
        }
    }
}
