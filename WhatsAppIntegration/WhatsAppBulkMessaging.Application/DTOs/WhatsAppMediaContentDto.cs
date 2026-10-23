using System;
using System.Collections.Generic;
using System.Text;

namespace WhatsAppBulkMessaging.Application.DTOs
{
    public class WhatsAppMediaContentDto
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();

        public string ContentType { get; set; } = "application/octet-stream";

        public string? FileName { get; set; }
    }
}
