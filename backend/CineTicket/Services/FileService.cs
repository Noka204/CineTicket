using Microsoft.AspNetCore.Hosting;

namespace CineTicket.Services
{
    public class FileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty");

            var webroot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webroot))
                throw new InvalidOperationException("WebRootPath is not configured. Ensure UseStaticFiles() is enabled.");

            string rootPath = Path.Combine(webroot, "Images", folderName);
            Directory.CreateDirectory(rootPath);

            string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(rootPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // đường dẫn tương đối để lưu DB/hiển thị frontend
            return $"/Images/{folderName}/{fileName}";
        }
    }
}
