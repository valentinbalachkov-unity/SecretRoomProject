namespace _LocalMemeProj.ImageUploadService
{
    public interface IImageUploadService
    {
        public void UploadImage(byte[] imageBytes, string lobbyId, string imageId, System.Action<string> onSuccess);
    }
}
