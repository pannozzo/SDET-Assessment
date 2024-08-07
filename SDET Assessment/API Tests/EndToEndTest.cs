using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V125.Target;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using static SDET_Assessment.Resources;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace SDET_Assessment
{
    [TestFixture(BrowserType.Chrome)]
    [TestFixture(BrowserType.Firefox)]
    [TestFixture(BrowserType.Edge)]
    public class EndToEndTest : AbstractBaseTests
    {
        HttpClient httpClient;
        public EndToEndTest(BrowserType browserType) : base(browserType) => browser = browserType;

        [SetUp]
        public void Setup()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8081/");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            driver = CreateDriver();
            driver.Url = "http://localhost:8080/";
            driver.Manage().Window.Maximize();

            Thread.Sleep(500); //The browser needs a second to actually open before it can start interacting with the page

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));

            wait.Until(ExpectedConditions.TitleIs("Home"));
        }

        [TearDown]
        public void TearDown() {
            driver.Quit();
            httpClient.Dispose();
        }

        [TestCase(2024, 01, 01, 10, WeatherSummary.Mild)]
        public async Task End_To_End_Integration_Test(int year, int month, int day, int temperatureC, WeatherSummary summary)
        {
            if (browser == BrowserType.Firefox || browser == BrowserType.Edge)
            {
                Assert.Ignore("Ran out of time to troubleshoot specific configurations for setting FireFox/Edge's downloading paths");
            }

            string originalTableText = "";
            string uploadedTableText = "";
            string uploadFile = Path.Combine(Path.GetTempPath(), "forecast.json");
            string downloadFile = downloadFilePath + @"\forecast.json";

            using HttpResponseMessage postResponse = await httpClient.PostAsJsonAsync(
               "weatherforecast",
               new WeatherRecord(
                   Date: new DateOnly(year, month, day),
                   TemperatureC: temperatureC,
                   Summary: summary
               ));

            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(postResponse.Headers.Location, Is.Not.Null);

            int postResponseId = int.Parse(Regex.Match(postResponse.Headers.Location.ToString(), @"\d+$", RegexOptions.RightToLeft).Value);

            using HttpResponseMessage getResponse = await httpClient.GetAsync($"weatherforecast/{postResponseId}");

            JObject jsonResponse = JObject.Parse(await getResponse.Content.ReadAsStringAsync());
            JArray jArray = new JArray();
            jArray.Add(jsonResponse);
            File.WriteAllText(uploadFile, jArray.ToString());

            ClickElement(By.LinkText("Weather"));

            IWebElement originalTable = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
            originalTableText = originalTable.Text;

            Assert.That(originalTableText, Is.Not.Empty);

            IWebElement fileInput = driver.FindElement(By.CssSelector("input[type=file]"));
            fileInput.SendKeys(uploadFile);

            wait.Until(driver => driver.FindElement(By.ClassName("table")).Text != originalTableText);
            IWebElement uploadedTable = wait.Until(d => driver.FindElement(By.ClassName("table")));
            uploadedTableText = uploadedTable.Text;

            Assert.That(originalTableText, Is.Not.EqualTo(uploadedTableText));

            ClickElement(By.TagName("button"));

            wait.Until(d => File.Exists(downloadFile));

            JArray uploadedJson = JArray.Parse(File.ReadAllText(uploadFile));
            JArray downloadedJson = JArray.Parse(File.ReadAllText(downloadFile));

            //The JSON member "id" is not preserved in the downloaded file.
            //See README.txt for notes
            foreach (var item in uploadedJson)
            {
                item["id"]?.Parent.Remove();
            }

            bool isJsonEqual = JArray.DeepEquals(downloadedJson, uploadedJson);


            Assert.That(isJsonEqual, Is.True);
        }
    }
}