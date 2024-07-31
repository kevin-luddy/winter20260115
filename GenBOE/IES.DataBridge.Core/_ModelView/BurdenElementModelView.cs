// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    /// <summary>
    /// The Model View used for Burden Elements.
    /// </summary>
    public class BurdenElementModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BurdenElementModelView"/> class.
        /// </summary>
        public BurdenElementModelView()
        {
            this.Id = -1;
            this.DisplayOrder = -1;
            this.Name = string.Empty;
            this.Description = string.Empty;
			this.DisplayOrder1LMX = -1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BurdenElementModelView"/> class.
		/// </summary>
		/// <param name="id">Burden Element Id value.</param>
		/// <param name="displayOrder">Display Order value.</param>
		/// <param name="name">Burden Element name.</param>
		/// <param name="description">Burden Element description.</param>
		/// <param name="displayOrder1LMX">Display Order 1LMX</param>
		public BurdenElementModelView(int id, int displayOrder, string name, string description, int displayOrder1LMX)
		{
            this.Id = id;
            this.DisplayOrder = displayOrder;
			this.DisplayOrder1LMX = displayOrder1LMX;
			this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets or sets the Burden Element Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the burden element name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

		/// <summary>
		/// Gets or sets the display order for 1LMX
		/// </summary>
		public int DisplayOrder1LMX { get; set; }
	}
}
