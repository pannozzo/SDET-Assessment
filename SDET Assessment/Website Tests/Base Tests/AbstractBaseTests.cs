using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using static SDET_Assessment.Resources;

namespace SDET_Assessment
{
    [TestFixture]
    public abstract class AbstractBaseTests
    {
        public void ClickElement(By locator)
        {
            try
            {
                IWebElement element = wait.Until(ExpectedConditions.ElementToBeClickable(locator));

                try
                {
                    element.Click();
                }

                catch (StaleElementReferenceException)
                {
                    ClickElement(locator);
                }
            }

            catch (StaleElementReferenceException)
            {
                ClickElement(locator);
            }
        }
        public bool TextInElementIsFound(By locator, string expectedText)
        {
            string text = "";
            bool isFound = false;
            try
            {
                IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(locator));
                try
                {
                    text = element.Text;
                    isFound = wait.Until(ExpectedConditions.TextToBePresentInElement(element, expectedText));
                }

                catch (StaleElementReferenceException)
                {
                    TextInElementIsFound(locator, expectedText);
                }
            }
            catch (StaleElementReferenceException)
            {
                TextInElementIsFound(locator, expectedText);
            }

            return isFound;
        }

        public string GetTextFromElement(By locator)
        {
            string text = "";
            try
            {
                IWebElement element = wait.Until(ExpectedConditions.ElementIsVisible(locator));
                try
                {
                    text = element.Text;
                }

                catch (StaleElementReferenceException)
                {
                    GetTextFromElement(locator);
                }
            }
            catch (StaleElementReferenceException)
            {
                GetTextFromElement(locator);
            }

            return text;
        }

        public IWebDriver CreateDriver()
        {
            SetBrowserDownloadLocation();

            switch (browser)
            {
                case BrowserType.Chrome:
                    return new ChromeDriver(chromeOptions);
                case BrowserType.Firefox:
                    return new FirefoxDriver();
                case BrowserType.Edge:
                    return new EdgeDriver();
                default:
                    return null;
            }
        }

        public void SetBrowserDownloadLocation()
        {
            Directory.CreateDirectory(downloadFilePath);

            chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("download.default_directory", downloadFilePath);
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("download.directory_upgrade", true);
            chromeOptions.AddUserProfilePreference("intl.accept_languages", "nl");
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");

            //Not implemented
            //firefoxOptions = new FirefoxOptions();
            //edgeOptions = new EdgeOptions();


        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Structure",
            "NUnit1032:An IDisposable field/property should be Disposed in a TearDown method",
            Justification = "driver.Quit() properly handles disposal in TearDown.")]
        protected IWebDriver driver;
        protected IWait<IWebDriver> wait;
        protected BrowserType browser;
        private ChromeOptions chromeOptions;
        private FirefoxOptions firefoxOptions;
        private EdgeOptions edgeOptions;
        public string downloadFilePath = Path.Combine(Path.GetTempPath(), @"downloadedForecast\forecast.json");

        protected AbstractBaseTests(BrowserType browserType) => this.browser = browserType;

    }
}