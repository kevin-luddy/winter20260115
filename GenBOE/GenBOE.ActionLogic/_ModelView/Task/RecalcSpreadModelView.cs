// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    public class RecalcSpreadModelView
    {
        public decimal? value { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public SpreadCurves? curve { get; set; }

        public RateType? rateType { get; set; }

        public decimal? percentSpread { get; set; }

        public bool? percentLocked { get; set; }

        public ICollection<LaborSpreadDataModelView> spreads { get; set; }

        /// <summary>
        /// Returns true if valid.
        /// </summary>
        public bool IsValid()
        {
            return start.HasValue && end.HasValue && curve.HasValue && rateType.HasValue && percentLocked.HasValue &&
                ((percentLocked.Value && percentSpread.HasValue) || (!percentLocked.Value && value.HasValue));
        }
    }
}
