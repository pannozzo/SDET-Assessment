using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V125.Target;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using static SDET_Assessment.Resources;
using System;

namespace SDET_Assessment
{
    [TestFixture(BrowserType.Chrome)]
    [TestFixture(BrowserType.Firefox)]
    [TestFixture(BrowserType.Edge)]
    public class NavigationTests : AbstractBaseTests
    {
        public NavigationTests(BrowserType browserType) : base(browserType) => browser = browserType;

        [SetUp]
        public void Setup()
        {
            driver = CreateDriver();
            driver.Url = "http://localhost:8080/";
            driver.Manage().Window.Maximize();

            Thread.Sleep(500); //The browser needs a second to actually open before it can start interacting with the page

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));

            wait.Until(ExpectedConditions.TitleIs("Home"));
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }

        [Test]
        public void Title_Reads_Home_When_The_Home_Tab_Is_Clicked()
        {
            ClickElement(By.LinkText("Counter"));
            wait.Until(ExpectedConditions.TitleIs("Counter"));

            ClickElement(By.LinkText("Home"));
            bool titleIsHome = wait.Until(ExpectedConditions.TitleIs("Home"));
            Assert.That(titleIsHome, Is.True);
        }

        [Test]
        public void Title_Reads_Home_When_The_SDET_Assessment_Link_Is_Clicked()
        {
            ClickElement(By.LinkText("Counter"));
            wait.Until(ExpectedConditions.TitleIs("Counter"));

            ClickElement(By.ClassName("navbar-brand"));
            bool titleIsHome = wait.Until(ExpectedConditions.TitleIs("Home"));
            Assert.That(titleIsHome, Is.True);
        }

        [Test]
        public void Title_Reads_Counter_When_The_Counter_Tab_Is_Clicked()
        {
            ClickElement(By.LinkText("Counter"));
            bool titleIsCounter = wait.Until(ExpectedConditions.TitleIs("Counter"));
            Assert.That(titleIsCounter, Is.True);
        }

        [Test]
        public void Title_Reads_Weather_When_The_Weather_Tab_Is_Clicked()
        {
            ClickElement(By.LinkText("Weather"));
            bool titleIsWeather = wait.Until(ExpectedConditions.TitleIs("Weather"));
            Assert.That(titleIsWeather, Is.True);
        }

        [TestCase("Home")]
        [TestCase("Counter")]
        [TestCase("Weather"),
            Description("The aria-current=page indicates to users of assistive technology which page" +
                        "in a navigation menu they're currently on, this should be set to the current page.")]

        public void Aria_Current_Is_Set_To_Page_On_Navigation_Item_That_Matches_Current_Page(string targetPage)
        {
            ClickElement(By.LinkText(targetPage));
            wait.Until(ExpectedConditions.TitleIs(targetPage));

            IWebElement currentPageLink = wait.Until(d => driver.FindElement(By.LinkText(targetPage)));

            string ariaCurrent = currentPageLink.GetAttribute("aria-current");

            Assert.That(ariaCurrent, Is.Not.Null);
            Assert.That(ariaCurrent, Is.EqualTo("page"));
        }

        [TestCase("Home")]
        [TestCase("Counter")]
        [TestCase("Weather"),
            Description("The aria-current attribute should only highlight the current page's link" +
                        "to aid assistive technology users with page navigation")]

        public void Aria_Current_Is_Not_Set_On_Navigation_Item_That_Do_Not_Match_Current_Page(string targetPage)
        {
            List<IWebElement> pageLinks = new List<IWebElement>();

            ClickElement(By.LinkText(targetPage));
            wait.Until(ExpectedConditions.TitleIs(targetPage));

            if(targetPage == "Home")
            {
                pageLinks.Add(wait.Until(d => driver.FindElement(By.LinkText("Counter"))));
                pageLinks.Add(wait.Until(d   => driver.FindElement(By.LinkText("Weather"))));
            }
            else if(targetPage == "Counter")
            {
                pageLinks.Add(wait.Until(d => driver.FindElement(By.LinkText("Home"))));
                pageLinks.Add(wait.Until(d => driver.FindElement(By.LinkText("Weather"))));
            }
            else if(targetPage == "Weather")
            {
                pageLinks.Add(wait.Until(d => driver.FindElement(By.LinkText("Home"))));
                pageLinks.Add(wait.Until(d => driver.FindElement(By.LinkText("Counter"))));
            }

            string ariaCurrent1 = pageLinks[0].GetAttribute("aria-current");
            string ariaCurrent2 = pageLinks[1].GetAttribute("aria-current");

            Assert.Multiple(() =>
            {
                Assert.That(ariaCurrent1, Is.Null);
                Assert.That(ariaCurrent2, Is.Null);
            });
        }

        [Test, Description("Setting collapsible menu items to aria-hidden=true can aid assistive" +
                            "technology users with page navigation by not cluttering the primary content of the page")]
        public void Collapsible_Menu_Items_Have_Aria_Hidden_Set_To_True()
        {
            IWebElement homeMenuOption = wait.Until(d => driver.FindElement(By.ClassName("bi-house-door-fill-nav-menu")));
            IWebElement counterMenuOption = wait.Until(d => driver.FindElement(By.ClassName("bi-plus-square-fill-nav-menu")));
            IWebElement weatherMenuOption = wait.Until(d => driver.FindElement(By.ClassName("bi-list-nested-nav-menu")));

            string ariaHiddenHome = homeMenuOption.GetAttribute("aria-hidden");
            string ariaHiddenCounter = counterMenuOption.GetAttribute("aria-hidden");
            string ariaHiddenWeather = weatherMenuOption.GetAttribute("aria-hidden");
            Assert.Multiple(() =>
            {
                Assert.That(ariaHiddenHome, Is.Not.Null);
                Assert.That(ariaHiddenHome, Is.EqualTo("true"));

                Assert.That(ariaHiddenCounter, Is.Not.Null);
                Assert.That(ariaHiddenCounter, Is.EqualTo("true"));

                Assert.That(ariaHiddenWeather, Is.Not.Null);
                Assert.That(ariaHiddenWeather, Is.EqualTo("true"));
            });

        }
    }
}