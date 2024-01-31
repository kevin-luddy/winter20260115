using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IES.Common;
using IES.Common.Exceptions;

namespace GenBOE.Tests.Common
{
    [TestClass]
    public class GenBOEUtilitiesTest
    {
        [TestMethod]
        public void CloneTest()
        {
			TestObject toClone = new TestObject
            {
                Attribute1 = 1,
                Attribute2 = 2,
                Attribute3 = 3
            };

			TestObject result = GenBOEUtilities.Clone(toClone);

            Assert.AreEqual(toClone.Attribute1, result.Attribute1);
            Assert.AreEqual(toClone.Attribute2, result.Attribute2);
            Assert.AreEqual(toClone.Attribute3, result.Attribute3);
            Assert.AreNotEqual(toClone, result);

            toClone = null;
            result = GenBOEUtilities.Clone(toClone);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ConvertHtmlToTextTest()
        {
            string html = "<table><tr><td style=\"color: black\">Cell contents</td></tr></table>";
            string converted = GenBOEUtilities.ConvertHtmlToText(html);
            Assert.AreEqual("Cell contents", converted);
        }

        [TestMethod]
        public void ScrubRichTextForSaveTest()
        {
            string html =
                "<table style='margin: auto auto auto 4.65pt; border-collapse: collapse; border-width: 1px 0px 0px 1px;'>"
                +   "<tbody>"
                +       "<tr style='height: 0.2in;'>"
                +           "<td style='padding: 0in 5.4pt; border: 1pt solid windowtext; width: 86.25pt; height: 0.2in; background-color: transparent;'>"
                +               "<p style='margin: 0in 0in 0pt; text-align: center;'><strong><span style='color: black;'>CDRL</span></strong></p>"
                +           "</td>"
                +       "</tr>"
                +   "</tbody>"
                + "</table>";

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();

            string scrubbed = GenBOEUtilities.ScrubRichTextForSave(html, validationMessages);

            string expected = "<table style='margin: auto auto auto 4.65pt;border-collapse: collapse;border-top-width:1px;border-bottom-width:0;border-right-width:0;border-left-width:1px;'><tbody><tr style='height: 0.2in;'><td style='padding: 0in 5.4pt; border: 1pt solid black; width: 86.25pt; height: 0.2in; background-color: transparent;'><p style='margin: 0in 0in 0pt; text-align: center;'><strong><span style='color: black;'>CDRL</span></strong></p></td></tr></tbody></table>";

            Assert.AreEqual(expected, scrubbed);
        }

        [TestMethod]
        public void AdjustDateTimePrecisionTest()
        {
            DateTime dt = new DateTime(2020, 6, 21);

            DateTime day = GenBOEUtilities.AdjustDateTimePrecision(dt, DateTimePrecision.Day);
            Assert.AreEqual(21, day.Day);
            Assert.AreEqual(6, day.Month);
            Assert.AreEqual(2020, day.Year);
            Assert.AreEqual(12, day.Hour);
            Assert.AreEqual(0, day.Minute);
            Assert.AreEqual(0, day.Second);

            DateTime month = GenBOEUtilities.AdjustDateTimePrecision(dt, DateTimePrecision.Month);
            Assert.AreEqual(15, month.Day);
            Assert.AreEqual(6, month.Month);
            Assert.AreEqual(2020, month.Year);
            Assert.AreEqual(12, day.Hour);
            Assert.AreEqual(0, month.Minute);
            Assert.AreEqual(0, month.Second);
        }

        #region Exception Test

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Clone_Exception1()
        {
			NonSerializableTestObject toClone = new NonSerializableTestObject
            {
                Attribute1 = 1,
                Attribute2 = 2,
                Attribute3 = 3
            };

			NonSerializableTestObject result = GenBOEUtilities.Clone(toClone);

            Assert.AreEqual(toClone.Attribute1, result.Attribute1);
            Assert.AreEqual(toClone.Attribute2, result.Attribute2);
            Assert.AreEqual(toClone.Attribute3, result.Attribute3);
            Assert.AreNotEqual(toClone, result);
        }

        #endregion Exception Test

        [Serializable()]
        class TestObject
        {
            public int Attribute1 { get; set; }
            public int Attribute2 { get; set; }
            public int Attribute3 { get; set; }
        }

        class NonSerializableTestObject
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public int Attribute1 { get; set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public int Attribute2 { get; set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public int Attribute3 { get; set; }
        }
    }
}
