using Microsoft.AspNetCore.Mvc;

namespace SmartX.Server.Controllers;

//DELCARATION OF AI USAGE
//Gemini. (2026) . Setup of API MediaController including upload and delete functionality. Available at: 
//https://share.gemini.google/O9Qf6fnMtm4B (Accessed: 12/09/2026).

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedReceipts");
    private readonly ILogger<MediaController> _logger;

    public MediaController(ILogger<MediaController> logger)
    {
        _logger = logger;
    }

    [HttpPost("upload/{truckId}")]
    public async Task<IActionResult> UploadReceipt(string truckId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var fileName = $"{truckId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(_uploadPath, fileName);

        try
        {
            Directory.CreateDirectory(_uploadPath);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, HttpContext.RequestAborted);
            return Ok(new { FileId = fileName, OriginalName = file.FileName, SizeBytes = file.Length });
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(exception, "Unable to save receipt for truck {TruckId}.", truckId);
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "The receipt could not be saved.");
        }
    }

    [HttpDelete("{fileName}")]
    public IActionResult DeleteReceipt(string fileName)
    {
        var filePath = Path.Combine(_uploadPath, fileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found.");

        try
        {
            System.IO.File.Delete(filePath);
            return Ok(new { Message = "File deleted successfully." });
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(exception, "Unable to delete receipt {FileName}.", fileName);
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "The receipt could not be deleted.");
        }
    }
}
