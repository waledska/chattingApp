namespace chattingApp.Services
{
    public interface ITransferPhotosToPathWithStoreService
    {
        string GetPhotoPath(IFormFile model);
        bool DeleteFile(string path);
    }
}
