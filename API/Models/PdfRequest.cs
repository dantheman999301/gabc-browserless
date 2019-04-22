using System;

namespace Browserless.API.Models
{
    public class PdfRequest
    {
        public Guid? Id { get; set; }
        public string Html { get; set; }
    }
}