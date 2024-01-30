// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Web.ControllerLogic
{
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests for GenBOEControllerLogic
    /// </summary>
    [TestClass]
    public class GenBOEControllerLogicTest
    {
        public GenBOEControllerLogicTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        private Mock<RMSZoneTravelRatesFeesDataLoader> zoneTravelRatesFeesLoader = new Mock<RMSZoneTravelRatesFeesDataLoader>();
        private Mock<IOffloadRatesDTOLoader> offloadRatesLoader = new Mock<IOffloadRatesDTOLoader>();

        # region Create System

        /// <summary>
        /// Creates the IS&GS subject under test
        /// </summary>
        /// <returns>the <see cref="GenBOEControllerLogic"/> to test</returns>
        private GenBOEControllerLogic CreateSystemISGS()
        {
            return new GenBOEControllerLogic();
        }

        private GenBOEControllerLogicMST CreateSystemRMS()
        {
            return new GenBOEControllerLogicMST(this.zoneTravelRatesFeesLoader.Object, this.offloadRatesLoader.Object);
        }

        #endregion

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        /// <summary>
        /// Tests the DoAnExtraScrubbingForRTEInput method
        /// </summary>
        [TestMethod]
        public void DoAnExtraScrubbingForRTEInput_Test1()
        {
            GenBOEControllerLogic sut = this.CreateSystemISGS();
            string input = string.Empty;
            string expectedResult = string.Empty;

            string result = sut.DoAnExtraScrubbingForRTEInput(input);

            Assert.IsTrue(result == expectedResult);
        }

        /// <summary>
        /// Tests the DoAnExtraScrubbingForRTEInput method
        /// </summary>
        [TestMethod]
        public void DoAnExtraScrubbingForRTEInput_Test2()
        {
            GenBOEControllerLogic sut = this.CreateSystemISGS();
            string input = "<script> hello world </script><!-- some comment -->hello world <script language=\"js\"> hello world </script> and some more stuff <script> blah blah </script>";
            string expectedResult = "hello world  and some more stuff ";

            string result = sut.DoAnExtraScrubbingForRTEInput(input);

            Assert.IsTrue(result == expectedResult);
        }

        /// <summary>
        /// Tests the DoAnExtraScrubbingForRTEInput method
        /// </summary>
        [TestMethod]
        public void DoAnExtraScrubbingForRTEInput_Test3()
        {
            GenBOEControllerLogic sut = this.CreateSystemISGS();
            string input = "hello world and some more stuff ";
            string expectedResult = "hello world and some more stuff ";

            string result = sut.DoAnExtraScrubbingForRTEInput(input);

            Assert.IsTrue(result == expectedResult);
        }

        /// <summary>
        /// Tests the DoAnExtraScrubbingForRTEInput method
        /// </summary>
        [TestMethod]
        public void DoAnExtraScrubbingForRTEInput_Test4()
        {
            GenBOEControllerLogic sut = this.CreateSystemISGS();
            string input = "<script>hello world and some more stuff ";
            string expectedResult = "<script>hello world and some more stuff ";

            string result = sut.DoAnExtraScrubbingForRTEInput(input);

            Assert.IsTrue(result == expectedResult);
        }

        /// <summary>
        /// Tests the DoAnExtraScrubbingForRTEInput method
        /// </summary>
        [TestMethod]
        public void DoAnExtraScrubbingForRTEInput_Test5()
        {
            GenBOEControllerLogic sut = this.CreateSystemISGS();
            string input = "<SCRiPT>hello world and some more stuff </ScRiPt>";
            string expectedResult = string.Empty;

            string result = sut.DoAnExtraScrubbingForRTEInput(input);

            Assert.IsTrue(result == expectedResult);
        }

        /// <summary>
        /// Test the base AreOffloadRatesOutOfDate method, which always returns false
        /// </summary>
        [TestMethod]
        public void TestAreOffloadRatesOutOfDate()
        {
            GenBOEControllerLogic sut = this.CreateSystemISGS();
            bool result = sut.AreOffloadRatesOutOfDate(1);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test the RMS AreOffloadRatesOutOfDate method, which should return a valid bool
        /// </summary>
        [TestMethod]
        public void TestAreOffloadRatesOutOfDate_RMS()
        {
            GenBOEControllerLogic sut = this.CreateSystemRMS();
            this.offloadRatesLoader.Setup(x => x.AreCurrentOffloadRatesOutOfDate(It.IsAny<int>())).Returns(true);
            bool result = sut.AreOffloadRatesOutOfDate(1);
            Assert.IsTrue(result);
        }
    }
}
