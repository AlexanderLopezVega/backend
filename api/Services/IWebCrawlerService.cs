namespace api.Services
{
    public interface IWebCrawlerService
    {
        //  Methods
        Task<string> Download3DModelAsync(Guid id, string imagePath, string savePath = "Public/3D Models/Temp");
    }
}