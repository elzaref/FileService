using FileService.Database;
using FileService.Model;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly StorageContext _context;
        private readonly string _uploadFolder;

        public StorageController(StorageContext context, IWebHostEnvironment env)
        {
            _context = context;
            _uploadFolder = Path.Combine(env.ContentRootPath, "uploads");
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var fileId = Guid.NewGuid();
            var filePath = Path.Combine(_uploadFolder, fileId.ToString() + Path.GetExtension(file.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileMetadata = new FileMetadata
            {
                Id = fileId,
                Filename = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                Size = file.Length
            };

            _context.FileMetadatas.Add(fileMetadata);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFile), new { id = fileId }, new { fileId });
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> GetFile(Guid id)
        {
            var fileMetadata = await _context.FileMetadatas.FindAsync(id);

            if (fileMetadata == null)
            {
                return NotFound();
            }

            var stream = new FileStream(fileMetadata.FilePath, FileMode.Open, FileAccess.Read);
            return File(stream, fileMetadata.ContentType, fileMetadata.Filename);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFile(Guid id)
        {
            var fileMetadata = await _context.FileMetadatas.FindAsync(id);

            if (fileMetadata == null)
            {
                return NotFound();
            }

            if (System.IO.File.Exists(fileMetadata.FilePath))
            {
                System.IO.File.Delete(fileMetadata.FilePath);
            }

            _context.FileMetadatas.Remove(fileMetadata);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("metadata/{id}")]
        public async Task<IActionResult> GetMetadata(Guid id)
        {
            var fileMetadata = await _context.FileMetadatas.FindAsync(id);

            if (fileMetadata == null)
            {
                return NotFound();
            }

            return Ok(fileMetadata);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateFileMetadata(Guid id, [FromBody] FileMetadata updatedMetadata)
        {
            if (id != updatedMetadata.Id)
            {
                return BadRequest("File ID mismatch.");
            }

            var fileMetadata = await _context.FileMetadatas.FindAsync(id);

            if (fileMetadata == null)
            {
                return NotFound();
            }

            fileMetadata.Filename = updatedMetadata.Filename;
            fileMetadata.ContentType = updatedMetadata.ContentType;
            fileMetadata.Size = updatedMetadata.Size;

            _context.FileMetadatas.Update(fileMetadata);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }


}