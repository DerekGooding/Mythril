using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Blazor.Pages;

namespace Mythril.Tests;

[TestClass]
public class MobileLayoutTests : BunitTestBase
{
    [TestMethod]
    public void Home_RendersMobilePartyTab_AndSwitchesActiveState()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert - verify mobile party tab exists in DOM
        var partyTab = cut.Find("[data-testid='party-tab']");
        Assert.IsNotNull(partyTab);
        Assert.AreEqual("Party", partyTab.TextContent.Trim());

        // Click party tab
        partyTab.Click();

        // Verify active class is applied to party tab button
        Assert.IsTrue(partyTab.ClassList.Contains("active"));
    }
}
