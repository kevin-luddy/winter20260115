using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.ActionLogic.ModelView
{
    [ExcludeFromCodeCoverage]
    public abstract class PersistedDataModelView
    {
        protected PersistedDataModelView()
        {
            this.UpdateDate = DateTime.MinValue;
        }

        public DateTime UpdateDate { get; set; }

        public string UpdateDateLong
        {
            get
            {
                return this.UpdateDate.Ticks.ToString();
            }
            set
            {
                long val;
                if (long.TryParse(value, out val))
                {
                    this.UpdateDate = new DateTime(val);
                }
            }
        }
    }
}
