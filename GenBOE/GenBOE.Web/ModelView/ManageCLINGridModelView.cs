// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenBOE.ActionLogic.ModelView.Clin;
using IES.Common.PickList;

namespace GenBOE.Web.ModelView
{
    public class ManageCLINGridModelView 
    {
        public ManageCLINGridModelView()
            : base()
        {
            ClinResults = new Collection<ManageCLINModelView>();
        }

        public Collection<ManageCLINModelView> ClinResults { get; set; }
        public bool HideContractType { get; internal set; }

        public ICollection<PickListDto> ContractTypeList { get; set; }
    }
}
