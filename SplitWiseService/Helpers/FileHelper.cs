using Microsoft.AspNetCore.Http;

namespace SplitWiseService.Helpers;

public static class FileHelper
{
    public static string UploadFile(IFormFile file, string existingFilePath = null)
    {
        // Delete existing file if specified
        DeleteFile(existingFilePath);

        // Ensure the uploads folder exists
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Create a unique filename
        string fileName = $"{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(uploadsFolder, fileName);

        // Save the file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        // Return relative path to be stored in DB or shown in frontend
        return $"/uploads/{fileName}";
    }

    public static void DeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || filePath.StartsWith("/defaultimages/", StringComparison.OrdinalIgnoreCase)
        || filePath.StartsWith("/default", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", filePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return;
    }
}
