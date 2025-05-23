# Dynamsoft Barcode Reader Samples - .NET Edition

This repository contains multiple samples that demonstrates how to use the <a href="https://www.dynamsoft.com/barcode-reader/overview/" target="_blank">Dynamsoft Barcode Reader</a> .NET Edition.

## System Requirements

- Windows:
  - Supported Versions: Windows 8 and higher, or Windows Server 2012 and higher
  - Architecture: x64 and x86
  - Development Environment: Visual Studio 2012 or higher.

- Linux:
  - Supported Distributions: Ubuntu 14.04.4+ LTS, Debian 8+, CentOS 7+
  - Architectures: x64
  - Minimum GLIBC Version: GLIBC_2.18 or higher

- Supported .NET versions
  - .NET Framework 3.5 and above
  - .NET 6, 7, 8

## Samples

### Basic Barcode Reader Samples

| Sample | Description |
|---|---|
| [`ReadAnImage`](Samples/HelloWorld/ReadAnImage) | This sample demonstrates the simplest way to read barcodes from an image file and output barcode format and text. |
| [`ReadMultipleImages`](Samples/HelloWorld/ReadMultipleImages) | This sample demonstrates the simplest way to read barcodes from directory with image files and output barcode format and text. |
| [`GeneralSettings`](Samples/GeneralSettings) | This sample demonstrates how to configure general used settings and read barcodes from an image file. |
| [`ReadDPMBarcode`](Samples/ReadDPMBarcode) | This sample demonstrates how to read DPM (Direct Part Marking) barcodes and get barcode results. |

### Additional Samples using Capture Vision SDK

In addition to the classic barcode decoding samples listed above, the following samples go a step further by parsing the decoded results and showcasing more structured workflows.

> [!IMPORTANT]
> These samples use the `Dynamsoft.DotNet.CaptureVision.Bundle` package instead of `Dynamsoft.DotNet.BarcodeReader.Bundle`. If you're switching to these samples, make sure to install and use the correct package.

| Sample | Description |
| --- | --- |
| [`DriverLicenseScanner`](https://github.com/Dynamsoft/capture-vision-dotnet-samples/blob/main/Samples/DriverLicenseScanner) | Shows how to capture and extract user's information from driver license/ID. |
| [`VINScanner`](https://github.com/Dynamsoft/capture-vision-dotnet-samples/blob/main/Samples/VINScanner) | Shows how to capture and extract vehicle's information from Vehicle Identification Number (VIN). |

## License

The library requires a license to work, you use the API `LicenseManager.InitLicense` to initialize license key and activate the SDK.

These samples use a free public trial license which require network connection to function. You can request a 30-day free trial license key from <a href="https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=github&package=dotnet" target="_blank">Customer Portal</a> which works offline.

## Contact Us

<a href="https://www.dynamsoft.com/company/contact/">Contact Dynamsoft</a> if you have any questions.
