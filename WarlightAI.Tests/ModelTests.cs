using WarlightAI.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace WarlightAI.Tests
{
	[TestClass]
	public class ModelTests
	{
		[TestMethod]
		public void SuperRegion_ChildRegions()
		{
            //Arrange
            Region region = new Region() { ID = 1, IsWasteland = true, RegionStatus = RegionStatus.PossibleStartingRegion };
            SuperRegion superregion = new SuperRegion() { ID = 1 };

            //Act
            superregion.AddChildRegion(region);
            int childcount = superregion.ChildRegions.Count;

            //Assert
            Assert.IsTrue(childcount == 1);
            Assert.AreEqual(superregion.ChildRegions.First().ID, region.ID);
            Assert.AreEqual(superregion.ChildRegions.First().RegionStatus, region.RegionStatus);
            Assert.AreEqual(superregion.ChildRegions.First().IsWasteland, region.IsWasteland);
		}
	}
}
