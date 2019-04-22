using System;
using System.IO;
using System.Threading.Tasks;
using Browserless.API.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Flurl;
using Flurl.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Serialization;

namespace Browserless.API
{
    public static class PdfGeneratorFunction
    {
        private static readonly string BrowserlessUrl = Environment.GetEnvironmentVariable("browserless");
        private static readonly JsonSerializerSettings JsonSettings
            = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };


        [FunctionName(nameof(GenerteAndSave))]
        public static async Task GenerteAndSave(
            [ServiceBusTrigger("browserless-apps", Connection = "servicebusconnection")] string pdfRequestJson,
            [Blob("pdf", FileAccess.ReadWrite, Connection = "storageconnectionstring")] CloudBlobContainer pdfContainer,
            ILogger log)
        {
            FlurlHttp.Configure(c => c.JsonSerializer =
                new Flurl.Http.Configuration.NewtonsoftJsonSerializer(JsonSettings));

            log.LogInformation("Generating PDF from Browserless");
            var pdfRequest = JsonConvert.DeserializeObject<PdfRequest>(pdfRequestJson);
            var pdfResponse = await BrowserlessUrl
                .PostJsonAsync(new PdfOptions
                {
                    Html = $"{pdfRequest.Html}",
                    Options = new Options
                    {
                        DisplayHeaderFooter = false,
                        PrintBackground = false
                    }
                });

            var pdfBlob = pdfContainer.GetBlockBlobReference($"{pdfRequest.Id}.pdf");
            await pdfBlob.UploadFromStreamAsync(await pdfResponse.Content.ReadAsStreamAsync());
        }
    }
}
