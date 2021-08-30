// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test EnumUtilities
    /// </summary>
    [TestClass]
    public class EnumUtilitiesTest
    {
        /// <summary>
        /// Test Compare with exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void U_Compare_Exception_Test()
        {
            NonEnumTestStruct a = new NonEnumTestStruct();
            NonEnumTestStruct b = new NonEnumTestStruct();

            IES.Common.EnumUtilities.Compare<NonEnumTestStruct>(a, b);
        }

        /// <summary>
        /// Test Compare <code>Role</code>
        /// </summary>
        [TestMethod]
        public void U_Compare_Role_Test()
        {
            List<CompareRoleTestDataItem> data = new List<CompareRoleTestDataItem>
            {
                new CompareRoleTestDataItem { RoleA = PtmRole.Admin, RoleB = PtmRole.Admin, Comparison = 0 },
                new CompareRoleTestDataItem { RoleA = PtmRole.Admin, RoleB = PtmRole.Pricer, Comparison = 1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.Admin, RoleB = PtmRole.PeerReviewer, Comparison = 1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.Admin, RoleB = PtmRole.NotSet, Comparison = 1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.Admin, RoleB = PtmRole.ProposalSetupAdmin, Comparison = 1},

                new CompareRoleTestDataItem { RoleA = PtmRole.Pricer, RoleB = PtmRole.Admin, Comparison = -1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.Pricer, RoleB = PtmRole.Pricer, Comparison = 0 },
                new CompareRoleTestDataItem { RoleA = PtmRole.Pricer, RoleB = PtmRole.PeerReviewer, Comparison = 1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.Pricer, RoleB = PtmRole.NotSet, Comparison = 1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.Pricer, RoleB = PtmRole.ProposalSetupAdmin, Comparison = 1 },

                new CompareRoleTestDataItem { RoleA = PtmRole.ProposalSetupAdmin, RoleB = PtmRole.Admin, Comparison = -1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.ProposalSetupAdmin, RoleB = PtmRole.Pricer, Comparison = -1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.ProposalSetupAdmin, RoleB = PtmRole.ProposalSetupAdmin, Comparison = 0 },
                new CompareRoleTestDataItem { RoleA = PtmRole.ProposalSetupAdmin, RoleB = PtmRole.PeerReviewer, Comparison = 1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.ProposalSetupAdmin, RoleB = PtmRole.NotSet, Comparison = 1 },

                new CompareRoleTestDataItem { RoleA = PtmRole.PeerReviewer, RoleB = PtmRole.Admin, Comparison = -1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.PeerReviewer, RoleB = PtmRole.Pricer, Comparison = -1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.PeerReviewer, RoleB = PtmRole.PeerReviewer, Comparison = 0 },
                new CompareRoleTestDataItem { RoleA = PtmRole.PeerReviewer, RoleB = PtmRole.ProposalSetupAdmin, Comparison = 1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.PeerReviewer, RoleB = PtmRole.NotSet, Comparison = 1 },

                new CompareRoleTestDataItem { RoleA = PtmRole.NotSet, RoleB = PtmRole.Admin, Comparison = -1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.NotSet, RoleB = PtmRole.Pricer, Comparison = -1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.NotSet, RoleB = PtmRole.PeerReviewer, Comparison = -1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.NotSet, RoleB = PtmRole.ProposalSetupAdmin, Comparison = -1 },
                new CompareRoleTestDataItem { RoleA = PtmRole.NotSet, RoleB = PtmRole.NotSet, Comparison = 0 }
            };

            foreach (CompareRoleTestDataItem test in data)
            {
                int actualComparison = IES.Common.EnumUtilities.Compare<PtmRole>(test.RoleA, test.RoleB);

                Assert.AreEqual(actualComparison, test.Comparison);
            }
        }

        /// <summary>
        /// Test Compare Enum
        /// </summary>
        [TestMethod]
        public void U_Compare_Enum_Test()
        {
            Assert.AreEqual(0, IES.Common.EnumUtilities.Compare<EnumUtilitiesTestEnum>(EnumUtilitiesTestEnum.MOE, EnumUtilitiesTestEnum.MOE));
            Assert.AreEqual(1, IES.Common.EnumUtilities.Compare<EnumUtilitiesTestEnum>(EnumUtilitiesTestEnum.MOE, EnumUtilitiesTestEnum.MANNY));
            Assert.AreEqual(-1, IES.Common.EnumUtilities.Compare<EnumUtilitiesTestEnum>(EnumUtilitiesTestEnum.MOE, EnumUtilitiesTestEnum.JACK));
        }

        /// <summary>
        /// Compare Role test data item
        /// </summary>
        private class CompareRoleTestDataItem
        {
            /// <summary>
            /// First role
            /// </summary>
            public PtmRole RoleA { set; get; }

            /// <summary>
            /// Second role
            /// </summary>
            public PtmRole RoleB { set; get; }

            /// <summary>
            /// Expected comparison result
            /// </summary>
            public int Comparison { set; get; }
        }

        /// <summary>
        /// Test values for comparison
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented")]
        private enum EnumUtilitiesTestEnum
        {
            MANNY = 0,
            MOE = 1,
            JACK = 2
        }

        /// <summary>
        /// Represents an enumerated type
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Methods are not implemented or used.")]
        private struct NonEnumTestStruct : IComparable, IFormattable, IConvertible
        {
            public int CompareTo(object obj)
            {
                throw new NotImplementedException();
            }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                throw new NotImplementedException();
            }

            public TypeCode GetTypeCode()
            {
                throw new NotImplementedException();
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public byte ToByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public char ToChar(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public double ToDouble(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public short ToInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public int ToInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public long ToInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public float ToSingle(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public string ToString(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }
        }
    }
}
