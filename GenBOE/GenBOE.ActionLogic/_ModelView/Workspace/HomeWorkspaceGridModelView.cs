// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class HomeWorkspaceGridModelView
    {
        public HomeWorkspaceGridModelView()
        {
            items = new Collection<HomeWorkspaceModelView>();
        }

        public ICollection<HomeWorkspaceModelView> items { get; set; }
        public String CurrentUserDisplayName { get; set; }
    }
}

