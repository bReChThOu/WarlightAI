using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarlightAI.Helpers;

namespace WarlightAI.Tests
{
    [TestClass]
    public class HelpersTests
    {
        [TestMethod]
        public void ExtensionMethods_None()
        {
            //Arrange
            var filledList = new List<int> {1, 2, 3, 4, 5, 6};
            var emptyList = new List<int>();

            //Act
            var filledListIsEmpty = filledList.None();
            var emptyListIsEmpty = emptyList.None();

            //Assert
            Assert.IsFalse(filledListIsEmpty);
            Assert.IsTrue(emptyListIsEmpty);
        }
    }
}
