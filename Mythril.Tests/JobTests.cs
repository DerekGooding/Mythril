using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.GameLogic;
using Mythril.GameLogic.Jobs;
using System.Linq;

namespace Mythril.Tests
{
    [TestClass]
    public class JobTests
    {
        [TestMethod]
        public void ResourceManager_LoadsJobData_Correctly()
        {
            // Act
            var resourceManager = new ResourceManager();

            // Assert
            Assert.IsNotNull(resourceManager.Jobs);
            Assert.IsTrue(resourceManager.Jobs.Count > 0);

            var squireJob = resourceManager.Jobs.FirstOrDefault(j => j.Name == "Squire") as Squire;
            Assert.IsNotNull(squireJob);
            Assert.AreEqual("A basic warrior in training.", squireJob.Description);
            Assert.AreEqual(3, squireJob.Abilities.Count);
        }

        [TestMethod]
        public void PartyManager_LinksJobsToCharacters_Correctly()
        {
            // Arrange
            var resourceManager = new ResourceManager();

            // Act
            var partyManager = new PartyManager(resourceManager);

            // Assert
            var hero = partyManager.PartyMembers.FirstOrDefault(c => c.Name == "Hero");
            Assert.IsNotNull(hero);
            Assert.IsNotNull(hero.Job);
            Assert.AreEqual("Squire", hero.Job.Name);
            Assert.IsInstanceOfType(hero.Job, typeof(Squire));
        }
    }
}
