namespace GenBOE.Tests
{
    using System;
    using IES.Common;
    using HtmlAgilityPack;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HtmlAgilityPackUtilitiesTest
    {
        [TestMethod]
        public void CleanUpGenBoeMuckFromNodes_Test()
        {
            // Original Good text, properly encoded..
            string expectedHtml =
              "<div id=\"sources-element\" class=\"form-element\">"
            + "    <div class=\"wrapper\"><p>ICU II EMD MARS_SWIFT PHW LBR CODE_Location.xlsx</p></div>"
            + "</div>";

            // Stuff GenBOE does to the original good text, when the page goes read only..
            string inputHtmlWithBoeMuck =
              "<div id=\"sources-element\" class=\"form-element\">"
            + "    <div class=\"wrapper\">"
            + "        <textarea id=\"DataSource\" class=\"display-none\" name=\"DataSource\" maxlength=\"10\" jquery191021047627543446945=\"61\">&lt;p&gt;ICU II EMD MARS_SWIFT PHW LBR CODE_Location.xlsx&lt;/p&gt;</textarea> "
            + "        <div class=\"replacedWidgetText\" title=\"\" jquery191021047627543446945=\"273\">"
            + "             <p>ICU II EMD MARS_SWIFT PHW LBR CODE_Location.xlsx</p>"
            + "        </div>"
            + "    </div>"
            + "</div>";

            // Create the HTML Document & the nodes on which we'll be working..
            HtmlDocument document = new HtmlDocument(); document.LoadHtml(inputHtmlWithBoeMuck);
            HtmlNodeCollection allNodes = HtmlAgilityPackUtilities.GetAllNodes(document);

            // Strip out the bad nodes
            HtmlAgilityPackUtilities.ScrubPotentialGenBoeReadOnlyMuck(allNodes);

            // bad stuff stripped out of the original Html Document
            Assert.IsTrue(string.Equals(document.DocumentNode.InnerHtml, expectedHtml, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
