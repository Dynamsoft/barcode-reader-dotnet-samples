# Dynamsoft Barcode Reader Samples - .Net Edition

This repository contains multiple samples that demonstrates how to use the <a href="https://www.dynamsoft.com/barcode-reader/overview/?product=dbr&utm_source=samples&package=dotnet" target="_blank">Dynamsoft Barcode Reader</a> .Net Edition.

## Requirements
- Operating systems:
  - Windows: 7, 8, 10, 2003, 2008, 2008 R2, 2012 and above
- Environment: 
  - Visual Studio 2008 and above
- Framework supported: 
  - .NET Framework 2.0
  - .NET Framework 4.0 and above

## Samples

| Sample | Description |
|---|---|
| HelloWorld | This sample demonstrates the simplest way to read barcodes from an image file. |
| GeneralSettings | This sample demonstrates how to configure general settings and read barcodes from an image file. |
| ProcessDocumentsByBarcodes | This sample demonstrates how to renaming, splitting or classifying a batch of documents using barcodes result. |
| DecodeFromScannerAndWebcam | This sample demonstrates how to read barcodes from scanner, webcam and local files. |
| ImageDecoding | This sample demonstrates how to decode images in various format (including Disk File/File Bytes in Memory/Raw Buffer/Base64 String/Bitmap) when using Dynamsoft Barcode Reader. |
| SpeedFirstSettings | This sample demonstrates how to configure Dynamsoft Barcode Reader to read barcodes as fast as possible. The downside is that read-rate and accuracy might be affected. |
| ReadRateFirstSettings | This sample demonstrates how to configure Dynamsoft Barcode Reader to read as many barcodes as possible at one time. The downside is that speed and accuracy might be affected. It is recommended to apply these configurations when decoding multiple barcodes from a single image. |
| AccuracyFirstSettings | This sample demonstrates how to configure Dynamsoft Barcode Reader to read barcodes as accurately as possible. The downside is that speed and read-rate might be affected. It is recommended to apply these configurations when misreading is unbearable. |


## License

The library requires a license to work, you use the API InitLicenseFromDLS to tell the program where to fetch your license.

If the Organization ID is not specified or set as "200001", a 7-day (public) trial license will be used by default which is the case in these samples.

Note that network connection is required for this license to work.

When using your own license, locate the following line and specify your Organization ID.  
// organizationID = "200001";

If you don't have a license yet, you can request a trial from <a href="https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet" target="_blank">Customer Portal</a>.

For more information, please refer to https://www.dynamsoft.com/license-server/docs/about/licensefaq.html.

## Contact Us

https://www.dynamsoft.com/company/contact/