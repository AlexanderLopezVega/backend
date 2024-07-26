namespace api.Services
{
    public interface IWebCrawlerService
    {
        //  Methods
        Task<string> Download3DModelAsync(string imagePath, string savePath = "Public/3D Models");
    }
}