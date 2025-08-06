using Dynamsoft.Core;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.License;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerSideBarcodeDecoder
{
    [ApiController]
    public class DecodeController : ControllerBase
    {
        [HttpPost("decode_barcode")]
        public async Task<IActionResult> UploadData()
        {
            if (Request.ContentType != null)
            {
                byte[] fileBytes;
                if (Request.ContentType.StartsWith("multipart/form-data"))
                {
                    var form = await Request.ReadFormAsync();
                    var file = form.Files["file"];

                    if (file != null)
                    {
                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }
                    else
                    {
                        return BadRequest("No file found in multipart data.");
                    }
                }
                else if (Request.ContentType == "application/json")
                {
                    using var ms = new MemoryStream();
                    await Request.Body.CopyToAsync(ms);

                    if (TryParseJson(ms.ToArray(), out var doc)
                        && doc.RootElement.TryGetProperty("data", out var dataElement)
                        && dataElement.TryGetBytesFromBase64(out byte[]? bytes))
                    {
                        fileBytes = bytes;
                    }
                    else
                    {
                        return BadRequest("Invalid JSON");
                    }
                }
                else
                {
                    using var ms = new MemoryStream();
                    await Request.Body.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }

                (int errorCode, string errorString, List<string> texts) = Program.DecodeBarcodes(fileBytes);
                if (errorCode != (int)EnumErrorCode.EC_OK)
                {
                    return BadRequest(new { error_string = errorString, error_code = errorCode });
                }
                else
                {
                    return Ok(texts);
                }
            }

            return BadRequest("Unsupported content type.");
        }

        private bool TryParseJson(byte[] bytes, [NotNullWhen(true)] out JsonDocument? document)
        {
            var reader = new Utf8JsonReader(bytes);
            return JsonDocument.TryParseValue(ref reader, out document);
        }
    }

    internal class Program
    {
        internal static (int, string, List<string>) DecodeBarcodes(byte[] fileBytes)
        {
            CaptureVisionRouter cvRouter = new CaptureVisionRouter();
            CapturedResult result = cvRouter.Capture(fileBytes, PresetTemplate.PT_READ_BARCODES);

            int errorCode = 0;
            string errorString = string.Empty;
            if (result.GetErrorCode() != (int)EnumErrorCode.EC_OK && result.GetErrorCode() != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
            {
                errorCode = result.GetErrorCode();
                errorString = result.GetErrorString();
                Console.WriteLine($"Error: {errorCode}, {errorString}");
            }
            else
            {
                errorCode = (int)EnumErrorCode.EC_OK;
                errorString = "Successful.";
            }

            List<string> texts = new List<string>();
            DecodedBarcodesResult? barcodeResult = result.GetDecodedBarcodesResult();
            if (barcodeResult == null || barcodeResult.GetItems().Length == 0)
            {
                Console.WriteLine("No barcode detected.");
            }
            else
            {
                BarcodeResultItem[] items = barcodeResult.GetItems();
                Console.WriteLine($"Decoded {items.Length} barcodes.");
                foreach (var item in items)
                    texts.Add(item.GetText());
            }

            return (errorCode, errorString, texts);
        }

        public static void Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            int errorCode = 1;
            string errorMsg;

            // Initialize license.
            // You can request and extend a trial license from https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
            // The string 'DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9' here is a free public trial license. Note that network connection is required for this license to work.
            errorCode = LicenseManager.InitLicense("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9", out errorMsg);
            if (errorCode != (int)EnumErrorCode.EC_OK && errorCode != (int)EnumErrorCode.EC_UNSUPPORTED_JSON_KEY_WARNING)
            {
                Console.WriteLine("License initialization failed: ErrorCode: " + errorCode + ", ErrorString: " + errorMsg);
            }
            else
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Listen(IPAddress.Parse("127.0.0.1"), 8080);
                });
                builder.Services.AddControllers();

                var app = builder.Build();
                app.MapControllers();

                app.Run();
            }

            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();
        }
    }
}