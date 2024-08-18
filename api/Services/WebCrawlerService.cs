using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace api.Services
{
    public class WebCrawlerService(IWebHostEnvironment hostEnvironment) : IWebCrawlerService
    {
        //  Constants
        private const string ModelGeneratorURL = "https://stabilityai-triposr.hf.space";
        private const int WaitSeconds = 60 * 5;
        private const string ImageInputXpath = "//*[@id=\"content_image\"]/div[2]/div/button/input";
        private const string ImagePreviewXPath = "//*[@id=\"content_image\"]/div[2]/div[2]/div/img";
        private const string GenerateButtonXPath = "//*[@id=\"generate\"]";
        private const string DownloadLinkXPath = "//*[@id=\"component-17\"]/div[2]/div/a";

        //  Fields
        private readonly IWebHostEnvironment m_HostEnvironment = hostEnvironment;

        //  Interface implementations
        async Task<string> IWebCrawlerService.Download3DModelAsync(Guid id, string imagePath, string savePath) => await Download3DModelAsync(id, imagePath, savePath);

        //  Methods
        public async Task<string> Download3DModelAsync(Guid id, string imagePath, string savePath = "Public/3D Models/Temp")
        {
            //  Setup Selenium WebDriver
            ChromeOptions options = new();
            options.AddArgument("--headless");

            ChromeDriver driver = new(options);
            WebDriverWait wait = new(driver, new TimeSpan(0, 0, WaitSeconds));

            try
            {
                //  Navigate to URL
                Console.WriteLine("> Navigating to site");
                driver.Navigate().GoToUrl(ModelGeneratorURL);

                //  Find image input element
                IWebElement imageInput = wait.Until(ExpectedConditions.ElementExists(By.XPath(ImageInputXpath)));

                //  Upload image
                Console.WriteLine("> Uploading image");
                imageInput.SendKeys(imagePath);

                //  Wait for image to upload
                Console.WriteLine("> Waiting for image to upload");
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(ImagePreviewXPath)));

                //  Find generate button
                IWebElement generateButton = driver.FindElement(By.XPath(GenerateButtonXPath));

                //  Click to generate
                Console.WriteLine("> Generating");
                generateButton.Click();

                //  Find download link
                Console.WriteLine("> Finding download link");
                IWebElement downloadLink = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(DownloadLinkXPath)));

                wait.Until((IWebDriver _) => downloadLink.GetAttribute("href") != null);

                //  Get download URL
                Console.WriteLine("> Getting download URL");
                string? downloadURL =
                    downloadLink.GetAttribute("href")
                    ?? throw new ArgumentException($"Could not find download URL from element {downloadLink.TagName}");

                //  Determine paths
                string folder = Path.Combine(m_HostEnvironment.ContentRootPath, savePath);
                string filename = $"{Path.GetFileNameWithoutExtension(id.ToString())}.obj";
                string relativePath = Path.Combine(savePath, filename);

                //  Download OBJ file
                Console.WriteLine("> Downloading OBJ file");
                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(downloadURL);
                response.EnsureSuccessStatusCode();
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(Path.Combine(folder, filename), fileBytes);

                Console.WriteLine("> Finished crawling");

                return relativePath;
            }
            catch (Exception ex) { throw new ApplicationException("Could not download 3D model from the URL with provided image", ex); }
            finally { driver.Quit(); }
        }
    }
}