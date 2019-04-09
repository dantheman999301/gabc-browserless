using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GabcBrowserless
{
    class Program
    {
        private const string Path = "test-pdf.pdf";
        private static readonly JsonSerializerSettings JsonSettings
            = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        static async Task Main(string[] args)
        {
            FlurlHttp.Configure(c =>
            {
                c.JsonSerializer = new Flurl.Http.Configuration.NewtonsoftJsonSerializer(JsonSettings);
            });

            Console.WriteLine("Enter some text to generate a PDF");
            Console.WriteLine("");
            var textToGenerate = Console.ReadLine();
            var pdfBytes = await "http://localhost:3000/pdf"
                .PostJsonAsync(new PdfOptions
                {
                    Html = $"<h1>{textToGenerate}</h1>",
                    Options = new Options
                    {
                        DisplayHeaderFooter = false,
                        PrintBackground = false
                    }
                });

            if (File.Exists(Path))
            {
                File.Delete(Path);
            }

            File.WriteAllBytes(Path, await pdfBytes.Content.ReadAsByteArrayAsync());

            new Process
            {
                StartInfo = new ProcessStartInfo(Path)
                {
                    UseShellExecute = true
                }
            }.Start();
        }
    }

    public class PdfOptions
    {
        public string Html { get; set; }
        public Options Options { get; set; }
    }

    public class Options
    {
        public bool DisplayHeaderFooter { get; set; }
        public bool PrintBackground { get; set; }
    }
}
