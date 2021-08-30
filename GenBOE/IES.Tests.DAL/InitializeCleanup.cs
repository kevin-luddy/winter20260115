// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test initialization and cleanup
    /// </summary>
    [TestClass]
    static public class InitializeCleanup
    {
        /// <summary>
        /// Test data
        /// </summary>
        private static TestData testData = TestData.GetInstance();

        /// <summary>
        /// Initialize a test
        /// </summary>
        /// <param name="context">Test context object</param>
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            testData.Initialize();
        }

        /// <summary>
        /// Cleanup a test
        /// </summary>
        [AssemblyCleanup]
        public static void Cleanup()
        {
            testData.Cleanup();
        }
    }
}
