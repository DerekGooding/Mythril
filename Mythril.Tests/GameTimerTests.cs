using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class GameTimerTests : ResourceManagerTestBase
{
    [TestMethod]
    public void ResourceManager_Tick_IncrementsCurrentTime()
    {
        double initialTime = _resourceManager!.CurrentTime;
        _resourceManager.Tick(1.0);
        Assert.AreEqual(initialTime + 1.0, _resourceManager.CurrentTime);
        
        _resourceManager.Tick(0.5);
        Assert.AreEqual(initialTime + 1.5, _resourceManager.CurrentTime);
    }

    [TestMethod]
    public void ResourceManager_Initialize_ResetsCurrentTime()
    {
        _resourceManager!.Tick(100.0);
        Assert.AreEqual(100.0, _resourceManager.CurrentTime);
        
        _resourceManager.Initialize();
        Assert.AreEqual(0, _resourceManager.CurrentTime);
    }

    [TestMethod]
    public void ResourceManager_CurrentTimeFormatted_Works()
    {
        // Reset to 0
        _resourceManager!.Initialize();
        Assert.AreEqual("0:00", _resourceManager.CurrentTimeFormatted);

        _resourceManager.Tick(65.0);
        Assert.AreEqual("1:05", _resourceManager.CurrentTimeFormatted);

        _resourceManager.Tick(3600.0); // +1 hour
        // 1:01:05
        Assert.AreEqual("1:01:05", _resourceManager.CurrentTimeFormatted);
    }
}
