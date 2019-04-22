using System;
using System.IO;
using System.Threading.Tasks;
using Browserless.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Browserless.API
{
    public static class ApiFunctions
    {
        [FunctionName(nameof(Generate))]
        public static async Task<IActionResult> Generate(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "generate")] HttpRequest req,
            [ServiceBus("browserless-apps", Connection = "servicebusconnection")] IAsyncCollector<PdfRequest> pdfToGenerate,
            ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var pdfRequest = JsonConvert.DeserializeObject<PdfRequest>(requestBody);
            pdfRequest.Id = Guid.NewGuid();

            log.LogInformation("Putting request onto queue");

            await pdfToGenerate.AddAsync(pdfRequest);

            return new OkObjectResult(pdfRequest.Id);
        }

        [FunctionName(nameof(RequestStatus))]
        public static async Task<IActionResult> RequestStatus(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status/{id}")] HttpRequest req,
            string id,
            [Blob("pdf/{id}.pdf", FileAccess.ReadWrite, Connection = "storageconnectionstring")] CloudBlob pdfBlob,
            ILogger log)
        {
            log.LogInformation($"Getting PDF Status - {id}");
            if (!await pdfBlob.ExistsAsync()) return new NotFoundObjectResult("PDF not found");

            var stream = new MemoryStream();
            await pdfBlob.DownloadToStreamAsync(stream);
            stream.Position = 0;
            return new FileStreamResult(stream, "application/pdf");

        }
    }
}
