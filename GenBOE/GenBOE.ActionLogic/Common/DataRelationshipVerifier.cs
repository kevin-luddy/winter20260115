// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common
{
    using System;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common.Exceptions;

    public static class DataRelationshipVerifier
    {
        public static void VerifyDataRelation(Collection<IWorkspaceMembership> workspaces, int workspaceID)
        {
            if (workspaces == null)
            {
                throw new ArgumentNullException(nameof(workspaces));
            }

            foreach (IWorkspaceMembership workspace in workspaces)
            {
                VerifyDataRelation(workspace, workspaceID);
            }
        }

        public static void VerifyDataRelation(IWorkspaceMembership workspace, int workspaceID)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (workspace.WorkspaceID != workspaceID)
            {
                throw new InvalidDataRelationException("The DTO does not belong to the current workspace.");
            }
        }

        public static void VerifyDataRelation(Collection<IBOEMembership> boes, int boeID)
        {
            if (boes == null)
            {
                throw new ArgumentNullException(nameof(boes));
            }

            foreach (IBOEMembership boe in boes)
            {
                VerifyDataRelation(boe, boeID);
            }
        }

        public static void VerifyDataRelation(IBOEMembership boe, int boeID)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (boe.BoeID != boeID)
            {
                throw new InvalidDataRelationException("The DTO does not belong to the current BOE.");
            }
        }
    }
}
