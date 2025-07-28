using Aspose.Cells;
using LuckyOffice.Common;
using LuckyOffice.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO.Compression;

namespace LuckyOffice.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Convert(UploadFileViewModel model)
        {
            var allFiles = new List<(Stream stream, string fileName)>();
            var errors = new List<string>();

            try
            {
                if (model.ExcelFiles != null && model.ExcelFiles.Any(f => f != null && f.Length > 0))
                {
                    foreach (var file in model.ExcelFiles.Where(f => f != null && f.Length > 0))
                    {
                        var fileExtension = Path.GetExtension(file.FileName).ToLower();
                        if (ExcelConstants.SupportedExtensions.Contains(fileExtension))
                        {
                            var memoryStream = new MemoryStream();
                            await file.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                            allFiles.Add((memoryStream, file.FileName));
                        }
                        else
                        {
                            errors.Add($"File '{file.FileName}' is not valid");
                        }
                    }
                }

                if (!allFiles.Any())
                {
                    ViewBag.Message = "Please select valid Excel files.";
                    return View("Index");
                }

                if (errors.Any())
                {
                    ViewBag.Message = $"Some errors occurred: {string.Join("; ", errors)}";
                }

                if (allFiles.Count == 1)
                {
                    return await ConvertSingleFileFromStream(allFiles[0].stream, allFiles[0].fileName);
                }
                else
                {
                    return await ConvertMultipleFilesFromStreams(allFiles);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file conversion.");
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View("Index");
            }
            finally
            {
                foreach (var (stream, _) in allFiles)
                {
                    stream?.Dispose();
                }
            }
        }

        private async Task<IActionResult> ConvertSingleFileFromStream(Stream fileStream, string fileName)
        {
            try
            {
                fileStream.Position = 0;
                Workbook workbook = new Workbook(fileStream);

                using (var finalStream = new MemoryStream())
                {
                    workbook.Save(finalStream, SaveFormat.Xlsx);
                    finalStream.Position = 0;

                    byte[] fileBytes = await Task.Run(() => finalStream.ToArray());

                    string outputFileName = Path.GetFileNameWithoutExtension(fileName) + $"-{DateTime.Now:yyyyMMdd_HHmmss}" + ".xlsx";
                    return File(fileBytes,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                outputFileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while converting single file.");
                return BadRequest("Đã xảy ra lỗi khi xử lý file.");
            }
        }

        private async Task<IActionResult> ConvertMultipleFilesFromStreams(List<(Stream stream, string fileName)> files)
        {
            using (var zipStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var (stream, fileName) in files)
                    {
                        stream.Position = 0;
                        Workbook workbook = new Workbook(stream);

                        using (var cleanedStream = new MemoryStream())
                        {
                            workbook.Save(cleanedStream, SaveFormat.Xlsx);
                            cleanedStream.Position = 0;

                            string outputFileName = Path.GetFileNameWithoutExtension(fileName) + ".xlsx";
                            var zipEntry = archive.CreateEntry(outputFileName);

                            using (var zipEntryStream = zipEntry.Open())
                            {
                                await cleanedStream.CopyToAsync(zipEntryStream);
                            }
                        }
                    }
                }

                zipStream.Position = 0;
                string zipFileName = $"converted_files_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                return File(zipStream.ToArray(), "application/zip", zipFileName);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
