using Microsoft.AspNetCore.Mvc;

namespace ElAmir.Controllers
{
    public class ElAmirController : BaseApiController
    {
        private readonly ExcelImportService _excelImportService;

        public ElAmirController(ExcelImportService excelImportService)
        {
            _excelImportService = excelImportService;
        }

        [HttpPost("upload")]
        public async Task<ActionResult> UploadExcelFileAsync([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Save the file to a temporary location
            var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "ExcelFiles", file.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? "wwwroot/Upload/ExcelFiles");

            await using var fileStream = new FileStream(filepath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            // Return the file path or an identifier (e.g., filename)
            return Ok(new { filePath = filepath });
        }

        [HttpPost("insertdata")]
        public async Task<IActionResult> InsertDataFromExcelFile([FromQuery] string fileName, [FromQuery] string tableName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            try
            {
                // Construct the file path using the provided file name
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "ExcelFiles", $"{fileName}.xlsx");

                // Check if the file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"File with name {fileName} not found.");
                }

                // Create a MemoryStream to store the file content
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var memoryStream = new MemoryStream();
                
                // Copy the content of the FileStream to the MemoryStream
                await fileStream.CopyToAsync(memoryStream);

                // Reset the position of the memory stream before using it
                memoryStream.Position = 0;

                // Dynamically get the entity type from the provided name
                Type entityType = Type.GetType($"ElAmir.Data.Entities.{tableName}");
                if (entityType == null)
                {
                    return BadRequest($"Entity type {tableName} not found.");
                }

                // Pass the MemoryStream and entity type to the service for processing
                await _excelImportService.ImportExcelData(memoryStream, entityType);

                return Ok("File processed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error during import: {ex.Message}");
            }
        }


        [HttpGet("download")]
        public async Task<IActionResult> DownloadExcelFileAsync([FromQuery] string fileName)
        {
            string path = Path.Combine("wwwroot", "Upload", "ExcelFiles", $"{fileName}.xlsx");

            if (System.IO.File.Exists(path))  
            {
                var fileStream = await Task.Run(() => System.IO.File.OpenRead(path));
                return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(path));  
            }  
            return NotFound(); 
        }
    }
}
