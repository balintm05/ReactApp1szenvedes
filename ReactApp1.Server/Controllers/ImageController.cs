﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server.Data;
using ReactApp1.Server.Entities;
using ReactApp1.Server.Models;
using ReactApp1.Server.Services.Image;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ReactApp1.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController(IWebHostEnvironment environment, IConfiguration configuration, IImageService imageService) : ControllerBase
    {
        [Authorize(Roles ="Admin")]
        [HttpGet("get")]
        public async Task<List<Images>?> GetImages()
        {
            return await imageService.getImages();
        }
        [Authorize(Policy ="InternalOnly")]
        [HttpPost("upload")]
        public async Task<ActionResult<ImageUploadResponse>> UploadImage(IFormFile file, [FromServices] DataBaseContext dbContext)
        {
            try
            {
                var fileStorageConfig = configuration.GetSection("FileStorage");
                var maxSize = fileStorageConfig.GetValue<long>("MaxFileSize", 20 * 1024 * 1024);
                var allowedExts = fileStorageConfig.GetValue<string[]>("AllowedExtensions") ?? [".jpg", ".jpeg", ".png"];
                var imagesDir = Path.Combine(environment.ContentRootPath, fileStorageConfig["ImageStoragePath"] ?? "Images");
                if (file == null || file.Length == 0)
                    return BadRequest(new ImageUploadResponse { Error = new("No file provided") });

                if (file.Length > maxSize)
                    return BadRequest(new ImageUploadResponse { Error = new($"Max size: {maxSize / 1024 / 1024}MB") });

                var fileExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExts.Contains(fileExt))
                    return BadRequest(new ImageUploadResponse { Error = new($"Allowed formats: {string.Join(", ", allowedExts)}") });
                Directory.CreateDirectory(imagesDir); 
                var originalFileName = $"{Guid.NewGuid()}{fileExt}";
                var originalFilePath = Path.Combine(imagesDir, originalFileName);
                await using (var stream = new FileStream(originalFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var processedFileName = fileExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                       fileExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                    ? originalFileName
                    : $"{Path.GetFileNameWithoutExtension(originalFileName)}.jpg";

                var processedFilePath = Path.Combine(imagesDir, processedFileName);
                if (!processedFileName.Equals(originalFileName))
                {
                    await ProcessImage(originalFilePath, processedFilePath);
                    System.IO.File.Delete(originalFilePath); 
                }
                var imageRecord = new Images
                {
                    FileName = processedFileName,
                    OriginalFileName = file.FileName,
                    RelativePath = $"/images/{processedFileName}",
                    ContentType = processedFileName.EndsWith(".png") ? "image/png" : "image/jpeg",
                    FileSize = new FileInfo(processedFilePath).Length,
                    UploadDate = DateTime.UtcNow
                };
                await dbContext.Images.AddAsync(imageRecord);
                await dbContext.SaveChangesAsync();
                var absoluteUrl = $"{Request.Scheme}://{Request.Host}/images/{processedFileName}";
                return Ok(new ImageUploadResponse
                {
                    Id = imageRecord.Id,
                    Url = absoluteUrl,
                    OriginalSize = file.Length,
                    ProcessedSize = imageRecord.FileSize,
                    UploadDate = imageRecord.UploadDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ImageUploadResponse { Error = new($"Internal error: {ex.Message}") });
            }
        }
        private static async Task ProcessImage(string inputPath, string outputPath, int maxWidth = 1920, int quality = 80)
        {
            using var image = await Image.LoadAsync(inputPath);
            image.Mutate(x => x.AutoOrient());

            if (image.Width > maxWidth)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth, 0),
                    Mode = ResizeMode.Max
                }));
            }
            var encoder = new JpegEncoder { Quality = quality };
            await image.SaveAsync(outputPath, encoder);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteImage(int id, [FromServices] DataBaseContext dbContext)
        {
            var image = await dbContext.Images.FindAsync(id);
            if (image == null)
                return NotFound();

            var imagesDir = Path.Combine(environment.ContentRootPath,
                configuration["FileStorage:ImageStoragePath"] ?? "Images");
            var filePath = Path.Combine(imagesDir, image.FileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            dbContext.Images.Remove(image);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}

