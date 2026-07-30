using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Blazor.Components;

namespace Mythril.Tests;

[TestClass]
public class PwaInstallButtonTests : BunitTestBase
{
    [TestMethod]
    public void PwaInstallButton_RendersCorrectly_WithDataTestId()
    {
        // Act
        var cut = RenderComponent<PwaInstallButton>();

        // Assert - verify stable DOM anchor data-testid
        var button = cut.Find("[data-testid='pwa-install-button']");
        Assert.IsNotNull(button);
    }

    [TestMethod]
    public void PwaInstallButton_TogglesInfoModal_WhenClicked()
    {
        // Act
        var cut = RenderComponent<PwaInstallButton>();
        var button = cut.Find("[data-testid='pwa-install-button']");
        button.Click();

        // Assert - modal should open when clicked
        var closeButton = cut.Find("[data-testid='pwa-modal-close']");
        Assert.IsNotNull(closeButton);

        // Close modal
        closeButton.Click();
        Assert.AreEqual(0, cut.FindAll("[data-testid='pwa-modal-close']").Count);
    }
}
