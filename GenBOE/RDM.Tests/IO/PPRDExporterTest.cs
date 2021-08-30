// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.IO
{
    using IES.ActionLogic.IO.Export;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test helper methods of the PPRD Export that don't directly work with the document/openxml items
    /// </summary>
    [TestClass]
    public class PPRDExporterTest
    {
        /// <summary>
        /// Create Sut
        /// </summary>
        /// <returns>sut</returns>
        private PPRDExporter CreateSut()
        {
            return new PPRDExporter();
        }

        /// <summary>
        /// Test ReplaceParagraphTags returns proper result
        /// </summary>
        [TestMethod]
        public void TestReplaceParagraphTags()
        {
            PPRDExporter sut = this.CreateSut();

            string htmlTextWithP = "<p>Test text 1</p><p style=\"color:red\">Test text 2</p>";
            string result = sut.ReplaceParagraphTags(htmlTextWithP);

            Assert.AreEqual("<div>Test text 1</div><div style=\"color:red\">Test text 2</div>", result);
        }
    }
}
