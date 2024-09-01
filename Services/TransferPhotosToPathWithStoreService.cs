
using System.Drawing;


namespace chattingApp.Services
{
    public class TransferPhotosToPathWithStoreService: ITransferPhotosToPathWithStoreService
    {
        // uploading photos!

        // for upload only one image
        public string GetPhotoPath(IFormFile model)
        {
            if (model == null || model.Length == 0)
            {
                // Handle the case where no file is provided
                return "error, IFormFile model can't be empty";
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var maxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB

            var imagesFolderName = "Images"; // Path relative to the project root

            // Validate file type
            var fileExtension = Path.GetExtension(model.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                // Handle invalid file type
                return "error, file format should be only { \".jpg\", \".jpeg\", \".png\", \".gif\" }";
            }

            // Validate file size
            if (model.Length > maxFileSizeInBytes)
            {
                // Handle oversized file
                return "error, image size can't be bigger than 10MB";
            }

            // to check the this image is squared img 
            try
            {
                // Check if image is square
                using (var image = Image.FromStream(model.OpenReadStream()))
                {
                    if (image.Width != image.Height)
                    {
                        return "error, image is not square";
                    }
                }
            }
            catch (Exception)
            {
                return "error, invalid image file";
            }

            string uniquePhotoName = Guid.NewGuid() + fileExtension;
            // Construct the full path
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), imagesFolderName, uniquePhotoName);

            try
            {
                // Save the file
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    model.CopyTo(fileStream);
                }

                return fullPath;
            }
            catch (Exception ex)
            {
                // Handle exceptions, log or rethrow based on your requirements
                // You might want to consider returning an error message or throwing a custom exception
                Console.WriteLine($"Error saving file: {ex.Message}");
                return "error, some thing went wrong!";
            }
        }

        

        // delete un needed images 
        public bool DeleteFile(string path)
        {
            // Check if file exists with its full path    
            if (File.Exists(path))
            {
                // If file found, delete it    
                File.Delete(path);
                Console.WriteLine("File deleted.");
                return true;
            }
            else
            {
                Console.WriteLine("File not found");
                return false;
            }
        }
    }
}
