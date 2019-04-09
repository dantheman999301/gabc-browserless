using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Flurl.Http;
using McMaster.Extensions.CommandLineUtils;
using NBomber.Contracts;
using NBomber.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GabcBrowserless
{
    class Program
    {
        private const string Path = "test-pdf.pdf";
        private static readonly JsonSerializerSettings JsonSettings
            = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "Browserless-Demo"
            };
            app.HelpOption();

            app.Command("basic",  cmd =>
            {
                Console.WriteLine("Generating Basic PDF");
                var textToGenerate = cmd.Argument("text", "Text to generate a PDF from").IsRequired();
                cmd.OnExecute(async () => await GeneratePdf(textToGenerate.Value, openFile: true));
            });

            app.Command("stress",  cmd =>
            {
                Console.WriteLine("Stress testing PDFs");

                var step1 = Step.CreateAction("Generate PDF", ConnectionPool.None, async context =>
                {
                    var guid = Guid.NewGuid();
                    await GeneratePdf(guid.ToString(), $"{guid}.pdf");
                    return Response.Ok();
                });

                var scenario = ScenarioBuilder.CreateScenario("Generating PDFs!", step1)
                    .WithConcurrentCopies(10)
                    .WithDuration(TimeSpan.FromSeconds(30));

                cmd.OnExecute( () => NBomberRunner.RegisterScenarios(scenario)
                    .RunInConsole());
            });

            app.OnExecute(() =>
            {
                Console.WriteLine("Specify a subcommand");
                app.ShowHelp();
                return 1;
            });

            return app.Execute(args);
        }

        private static async Task GeneratePdf(string textToGenerate, string pathToSave = Path, bool openFile = false)
        {
            FlurlHttp.Configure(c => c.JsonSerializer =
                new Flurl.Http.Configuration.NewtonsoftJsonSerializer(JsonSettings));

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

            if (File.Exists(pathToSave))
            {
                File.Delete(pathToSave);
            }

            File.WriteAllBytes(pathToSave, await pdfBytes.Content.ReadAsByteArrayAsync());

            if (openFile)
            {
                new Process
                {
                    StartInfo = new ProcessStartInfo(pathToSave)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
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
