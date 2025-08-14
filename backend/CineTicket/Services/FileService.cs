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

            string rootPath = Path.Combine(_env.WebRootPath, "Images", folderName);
            Directory.CreateDirectory(rootPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(rootPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối (để lưu vào DB và hiển thị)
            return $"/Images/{folderName}/{fileName}";
        }
    }
}
