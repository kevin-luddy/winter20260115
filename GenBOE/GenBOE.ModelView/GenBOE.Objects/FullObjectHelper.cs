// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// Provides static methods for interacting with the Full Objects.
    /// </summary>
    public static class FullObjectHelper
    {
        private static bool? showEquivalentPersonsOption;
        private static object showEquivalentPersonsOptionLock = new object();
        
        /// <summary>
        /// Gets the Hours label depending on the system preferences and the workspace preferences.
        /// </summary>
        /// <param name="workspace">The workspace to use to determine the hours label.</param>
        /// <returns>The label for Hours/EPs.</returns>
        public static string HoursLabel(WorkspaceDTO workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            return FullObjectHelper.ShowEquivalentPersonsOption && workspace.IsUsingEquivalentPerson ? "EPs" : "Hours";
        }

        /// <summary>
        /// Returns true/false indicating whether to show the choice between Equivalent Persons and Hours for a workspace
        /// so that a workspace can be valued based on Hours or Equivalent Persons.
        /// If false, then all workspaces are always Hours.
        /// </summary>
        /// <returns>Boolean whether to show Equivalent Persons choice (and labels).</returns>
        public static bool ShowEquivalentPersonsOption
        {
            get
            {
                if (!showEquivalentPersonsOption.HasValue)
                {
                    lock (showEquivalentPersonsOptionLock)
                    {
                        if (!showEquivalentPersonsOption.HasValue)
                        {
                            showEquivalentPersonsOption = !string.IsNullOrEmpty(ConfigurationUtilities.GetAppSetting("ShowEquivalentPersonsOption"))
                                && ConfigurationUtilities.GetAppSetting("ShowEquivalentPersonsOption").ToLower().Equals("true");
                        }
                    }

                }

                return showEquivalentPersonsOption.Value;
            }
        }

        /// <summary>
        /// Refreshes the EP value -- only used for Unit Tests
        /// </summary>
        public static void RefreshEPForTests()
        {
            showEquivalentPersonsOption = null;
        }
    }
}
