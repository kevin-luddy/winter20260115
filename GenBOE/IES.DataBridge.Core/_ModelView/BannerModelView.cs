// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using System.ComponentModel.DataAnnotations;
	using Newtonsoft.Json;

	/// <summary>
	/// Model View for a Banner
	/// </summary>
	public class BannerModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BannerModelView"/> class.
        /// </summary>
        public BannerModelView()
        {
            this.Id = -1;
        }

        /// <summary>
        /// Gets or sets the selected apps.
        /// </summary>
        [Required(ErrorMessage = "At least one Selected App is required.")]
        public ICollection<string> SelectedApps { get; set; }

        /// <summary>
        /// Sets the selected apps as string.
        /// </summary>
        internal string SelectedAppsAsString
        {
            set
            {
                this.SelectedApps = value.Split(',');
            }
        }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        [JsonIgnore]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets the start date as date time.
        /// </summary>
        [Required]
        public string StartDateAsString
        {
            get
            {
                return this.StartDate.ToShortDateString() + " " + this.StartDate.ToShortTimeString();
            }

            set
            {
                this.StartDate = DateTime.Parse(value);
            }
        }

        /// <summary>
        /// Gets or sets the hours to show.
        /// </summary>
        [Required]
        public int HoursToShow { get; set; }

        /// <summary>
        /// Gets or sets the banner text.
        /// </summary>
        public string BannerText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [turn off ticker].
        /// </summary>
        [Required]
        public bool TurnOffTicker { get; set; }
    }
}
