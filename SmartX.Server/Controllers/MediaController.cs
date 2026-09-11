using Microsoft.AspNetCore.Mvc;

namespace SmartX.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedReceipts");

    public MediaController()
    {
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    [HttpPost("upload/{truckId}")]
    public async Task<IActionResult> UploadReceipt(string truckId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var fileName = $"{truckId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { FileId = fileName, OriginalName = file.FileName, SizeBytes = file.Length });
    }

    [HttpDelete("{fileName}")]
    public IActionResult DeleteReceipt(string fileName)
    {
        var filePath = Path.Combine(_uploadPath, fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found.");

        System.IO.File.Delete(filePath);
        return Ok(new { Message = "File deleted successfully." });
    }
}