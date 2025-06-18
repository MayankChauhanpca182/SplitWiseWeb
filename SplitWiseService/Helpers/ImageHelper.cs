using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SplitWiseService.Helpers;

public static class ImageHelper
{
    public static string GetRandomImage()
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/defaultimages");
        if (!Directory.Exists(folderPath))
        {
            return null;
        }

        string[] imageFiles = Directory.GetFiles(folderPath).ToArray();

        if (imageFiles.Length == 0)
            return null;

        Random random = new Random();
        string selectedFile = imageFiles[random.Next(imageFiles.Length)];

        return $"/defaultimages/{Path.GetFileName(selectedFile)}";
    }

    public static string UploadImage(IFormFile imageFile)
    {
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string fileName = $"{DateTime.Now.Ticks}{Path.GetExtension(imageFile.FileName).ToLower()}";
        string filePath = Path.Combine(uploadsFolder, fileName);

        // Compress the image
        using (Image image = Image.Load(imageFile.OpenReadStream()))
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(800, 800),
                Mode = ResizeMode.Max
            }));

            image.Save(filePath);
        }

        return $"/uploads/{fileName}";
    }

    public static void DeleteImage(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath) || imagePath.StartsWith("/defaultimages/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", imagePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return;
    }
}
