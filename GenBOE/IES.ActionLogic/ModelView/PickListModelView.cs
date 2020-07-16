// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ModelView
{
    using System.Reflection;
    using IES.Common.PickList;

    /// <summary>
    /// Model View for editing a PickList DTO
    /// </summary>
    /// <seealso cref="IES.Common.PickList.PickListDto" />
    public class PickListModelView : PickListDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PickListModelView"/> class.
        /// </summary>
        /// <param name="dto">The dto.</param>
        public PickListModelView(PickListDto dto)
        {
            if (dto != null)
            {
                foreach (PropertyInfo prop in dto.GetType().GetProperties())
                {
                    if (prop.CanWrite)
                    {
                        this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(dto, null), null);
                    }
                }
            }
        }

        /// <summary>
        /// Used for Testing and MVC binding only
        /// </summary>
        public PickListModelView()
        {
        }

        /// <summary>
        /// Gets or sets the PTM identifier.
        /// </summary>
        public int PtmId { get; set; }

        /// <summary>
        /// Gets or sets the BOE identifier.
        /// </summary>
        public int BoeId { get; set; }
    }
}
