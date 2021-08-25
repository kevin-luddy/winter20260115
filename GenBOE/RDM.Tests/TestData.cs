// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Common;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// The data created during tests
    /// </summary>
    public class TestData
    {
        /// <summary>
        /// Generates Rate Config test data.
        /// </summary>
        /// <returns>Collection of RateConfigModelViews similar to that returned by RateConfigLoader.GetAll method.</returns>
        public static ICollection<RateConfigModelView> GetRateConfigTestData()
        {
            return new Collection<RateConfigModelView> {
                new RateConfigModelView { Id = 1, RateTarget = "PPRD", Precision = 6 },
                new RateConfigModelView { Id = 2, RateTarget = "PPRD", RateCategory = RateCategory.DirectLabor, Prefix = "$", Precision = 2 },
                new RateConfigModelView { Id = 3, RateTarget = "PPRD", RateCategory = RateCategory.Fccom, Suffix = "%", Precision = 6, Multiplier = 100 },
                new RateConfigModelView { Id = 4, RateTarget = "PPRD", RateCategory = RateCategory.Fringe, Suffix = "%", Precision = 6, Multiplier = 100 },
                new RateConfigModelView { Id = 5, RateTarget = "PPRD", RateCategory = RateCategory.GA, Suffix = "%", Precision = 6, Multiplier = 100 },
                new RateConfigModelView { Id = 6, RateTarget = "PPRD", RateCategory = RateCategory.LaborEscalationFactor, Precision = 4 },
                new RateConfigModelView { Id = 7, RateTarget = "PPRD", RateCategory = RateCategory.LaborEscalationPercentage, Suffix = "%", Precision = 4, Multiplier = 100 },
                new RateConfigModelView { Id = 8, RateTarget = "PPRD", RateCategory = RateCategory.NonLaborEscalationFactor, Precision = 4 },
                new RateConfigModelView { Id = 9, RateTarget = "PPRD", RateCategory = RateCategory.NonLaborEscalationPercentage, Suffix = "%", Precision = 4, Multiplier = 100 },
                new RateConfigModelView { Id = 10, RateTarget = "PPRD", RateCategory = RateCategory.Overhead, Suffix = "%", Precision = 6, Multiplier = 100 },
                new RateConfigModelView { Id = 11, RateTarget = "PPRD", RateCategory = RateCategory.SMConv, Precision = 0 },
                new RateConfigModelView { Id = 12, RateTarget = "PPRD", RateCategory = RateCategory.ServiceCenter, Prefix = "$", Precision = 2 },
                new RateConfigModelView { Id = 13, RateTarget = "PPRD", RateCategory = RateCategory.TravelFee, Prefix = "$", Precision = 2 },
                new RateConfigModelView { Id = 14, RateTarget = "PPRD", RateCategory = RateCategory.TravelMlge, Prefix = "$", Precision = 3 },
                new RateConfigModelView { Id = 15, RateTarget = "PPRD", RateCategory = RateCategory.TravelOtc, Prefix = "$", Precision = 2 },
                new RateConfigModelView { Id = 16, RateTarget = "PPRD", RateCategory = RateCategory.TravelRc, Prefix = "$", Precision = 2 },
                new RateConfigModelView { Id = 17, RateTarget = "Rate", Precision = 6 },
                new RateConfigModelView { Id = 18, RateTarget = "Rate", RateCategory = RateCategory.DirectLabor, Prefix = "$", Precision = 2 },
                new RateConfigModelView { Id = 19, RateTarget = "Rate", RateCategory = RateCategory.Fccom, Precision = 6 },
                new RateConfigModelView { Id = 20, RateTarget = "Rate", RateCategory = RateCategory.Fringe, Precision = 6 },
                new RateConfigModelView { Id = 21, RateTarget = "Rate", RateCategory = RateCategory.GA, Precision = 6 },
                new RateConfigModelView { Id = 22, RateTarget = "Rate", RateCategory = RateCategory.LaborEscalationFactor, Precision = 4 },
                new RateConfigModelView { Id = 23, RateTarget = "Rate", RateCategory = RateCategory.LaborEscalationPercentage, Precision = 4 },
                new RateConfigModelView { Id = 24, RateTarget = "Rate", RateCategory = RateCategory.NonLaborEscalationFactor, Precision = 4 },
                new RateConfigModelView { Id = 25, RateTarget = "Rate", RateCategory = RateCategory.NonLaborEscalationPercentage, Precision = 4 },
                new RateConfigModelView { Id = 26, RateTarget = "Rate", RateCategory = RateCategory.Overhead, Precision = 6 },
                new RateConfigModelView { Id = 27, RateTarget = "Rate", RateCategory = RateCategory.SMConv, Precision = 0 },
                new RateConfigModelView { Id = 28, RateTarget = "Rate", RateCategory = RateCategory.ServiceCenter, Prefix = "$", Precision = 2 },
                new RateConfigModelView { Id = 29, RateTarget = "Rate", RateCategory = RateCategory.TravelFee, Prefix = "$", Precision = 2 },
                new RateConfigModelView { Id = 30, RateTarget = "Rate", RateCategory = RateCategory.TravelMlge, Prefix = "$", Precision = 3 },
                new RateConfigModelView { Id = 31, RateTarget = "Rate", RateCategory = RateCategory.TravelOtc, Prefix = "$", Precision = 2 },
                new RateConfigModelView { Id = 32, RateTarget = "Rate", RateCategory = RateCategory.TravelRc, Prefix = "$", Precision = 2 }
            };
        }
    }
}
