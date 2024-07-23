
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace backend.Services
{
    public class WebCrawlerService(IWebHostEnvironment hostEnvironment) : IWebCrawlerService
    {
        //  Constants
        private const string ModelGeneratorURL = "https://stabilityai-triposr.hf.space";
        private const int WaitSeconds = 30;
        private const string ImageInputXpath = "//*[@id=\"content_image\"]/div[2]/div/button/input";
        private const string ImagePreviewXPath = "//*[@id=\"content_image\"]/div[2]/div[2]/div/img";
        private const string GenerateButtonXPath = "//*[@id=\"generate\"]";
        private const string GLBTabXPath = "//*[@id=\"component-16-button\"]";
        private const string DownloadLinkXPath = "//*[@id=\"component-17\"]/div[2]/div/a";

        //  Fields
        private readonly IWebHostEnvironment m_HostEnvironment = hostEnvironment;

        //  Interface implementations
        async Task<string> IWebCrawlerService.Download3DModelAsync(string imagePath, string savePath) => await Download3DModelAsync(imagePath, savePath);

        //  Methods
        public async Task<string> Download3DModelAsync(string imagePath, string savePath = "Public/3D Models")
        {
            //  Setup Selenium WebDriver
            ChromeOptions options = new();
            options.AddArgument("--headless");

            ChromeDriver driver = new(options);

            try
            {
                //  Navigate to URL
                driver.Navigate().GoToUrl(ModelGeneratorURL);

                WebDriverWait wait = new(driver, new TimeSpan(0, 0, WaitSeconds));

                //  Find image input element
                IWebElement imageInput = wait.Until(ExpectedConditions.ElementExists(By.XPath(ImageInputXpath)));

                //  Upload image
                imageInput.SendKeys(imagePath);

                //  Wait for image to upload
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(ImagePreviewXPath)));

                //  Find generate button
                IWebElement generateButton = driver.FindElement(By.XPath(GenerateButtonXPath));

                //  Click to generate
                generateButton.Click();

                //  Find tab button
                IWebElement tabButton = driver.FindElement(By.XPath(GLBTabXPath));

                tabButton.Click();

                //  Find download link
                IWebElement downloadLink = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(DownloadLinkXPath)));

                Thread.Sleep(1000);

                //  Get download URL
                string downloadURL = downloadLink.GetAttribute("href");

                Console.WriteLine(downloadURL);

                //  Download 3D model
                string storageFolder = Path.Combine(m_HostEnvironment.ContentRootPath, savePath);
                string downloadFilePath = Path.Combine(storageFolder, $"{Path.GetFileNameWithoutExtension(imagePath)}.glb");
                using HttpClient client = new();

                HttpResponseMessage response = await client.GetAsync(downloadURL);

                response.EnsureSuccessStatusCode();
                await File.WriteAllBytesAsync(downloadFilePath, await response.Content.ReadAsByteArrayAsync());

                return downloadFilePath;
            }
            catch (Exception ex) { throw new ApplicationException("Could not download 3D model from the URL with provided image", ex); }
            finally { driver.Quit(); }
        }
    }
}