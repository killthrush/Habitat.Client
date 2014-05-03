using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Habitat.Client.Tests
{
    [TestClass]
    public class ExtensionsTests
    {
        #region TryGetProperty Tests

        [TestMethod]
        public void Get_existing_property_with_expected_type()
        {
            PropertyTestClass testObject = new PropertyTestClass();
            int length;
            Assert.IsTrue(testObject.TryGetProperty("Length", out length));
            Assert.AreEqual(testObject.Length, length);
        }

        [TestMethod]
        public void Get_existing_property_with_wrong_type()
        {
            PropertyTestClass testObject = new PropertyTestClass();
            string length;
            Assert.IsFalse(testObject.TryGetProperty("Length", out length));
            Assert.IsNull(length);
        }

        [TestMethod]
        public void Attempt_to_get_nonexistent_property()
        {
            PropertyTestClass testObject = new PropertyTestClass();
            int length;
            Assert.IsFalse(testObject.TryGetProperty("bar", out length));
            Assert.AreEqual(0, length);
        }

        [TestMethod]
        public void Attempt_to_get_property_using_null_reference()
        {
            PropertyTestClass testObject = null;
            int length;
            Assert.IsFalse(testObject.TryGetProperty("Length", out length));
            Assert.AreEqual(0, length);
        }

        #endregion TryGetProperty Tests

        #region TrySetProperty Tests

        [TestMethod]
        public void Set_existing_property_with_expected_type()
        {
            PropertyTestClass testObject = new PropertyTestClass();
            const int length = 7;
            Assert.IsTrue(testObject.TrySetProperty("Length", length));
            Assert.AreEqual(length, testObject.Length);
        }

        [TestMethod]
        public void Set_existing_property_with_wrong_type()
        {
            PropertyTestClass testObject = new PropertyTestClass();
            const string length = "foo";
            Assert.IsFalse(testObject.TrySetProperty("Length", length));
            Assert.AreEqual(0, testObject.Length);
        }

        [TestMethod]
        public void Attempt_to_set_nonexistent_property()
        {
            PropertyTestClass testObject = new PropertyTestClass();
            const int length = 7;
            Assert.IsFalse(testObject.TrySetProperty("bar", length));
            Assert.AreEqual(0, testObject.Length);
        }

        [TestMethod]
        public void Attempt_to_set_property_using_null_reference()
        {
            PropertyTestClass testObject = null;
            const int length = 7;
            Assert.IsFalse(testObject.TrySetProperty("Length", length));
        }

        #endregion TrySetProperty Tests

        #region Types

        internal class PropertyTestClass
        {
            public int Length { get; set; }
        }

        #endregion Types
    }
}
