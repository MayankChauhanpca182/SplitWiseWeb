using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace SplitWiseWeb.Controllers;

public class FileController : Controller
{
    public FileController()
    {
    }

    private string GetContentType(string path)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

    // GET View
    public IActionResult View(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return NotFound();
        }

        // Build the absolute path
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        string contentType = GetContentType(fullPath);

        byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
        return File(fileBytes, contentType);
    }

}
