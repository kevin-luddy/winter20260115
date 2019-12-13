// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Calculations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    
    public interface IBoeTaskElementRecalculation
    {
        /// <summary>
        /// Calculates MOQ Hours
        /// </summary>
        /// <param name="boeTaskElement">BOE Task element</param>
        /// <param name="fullWorkspace">workspace that contains both the BOE and the task element</param>
        /// <returns>string representing the calculated MOQ hours</returns>
        String CalculateMOQHoursTotal(BoeTaskElementDTO boeTaskElement, FullWorkspace fullWorkspace);

        /// <summary>
        /// This function recalculates the labor spreads
        /// </summary>
        /// <param name="laborTypes">The labor types.</param>
        /// <param name="ws">The workspace.</param>
        void AddRecalulatedLaborSpreadsOntoLaborTypes(ICollection<ResourceTypeDto> laborTypes, WorkspaceDTO ws);

        /// <summary>
        /// This function recalculates the labor spreads
        /// </summary>
        /// <param name="odcTypes">The odc types.</param>
        /// <param name="ws">The workspace.</param>
        void AddRecalulatedODCSpreadsOntoODCTypes(ICollection<OtherDirectCostType> odcTypes, WorkspaceDTO ws);

        /// <summary>
        /// Recalculate BOE Labor given a CLIN ID
        /// </summary>
        /// <param name="clin">The clin.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <param name="fullWorkspace">The full workspace.</param>
        /// <param name="incomingBoeTaskElements">The incoming boe task elements.</param>
        /// <param name="incomingWorkspaceVariables">The incoming workspace variables.</param>
        /// <returns>
        /// list of BOEs that have been updated
        /// </returns>
        
        List<BoeTaskElementDTO> RecalculateLaborWithClin(FullClin clin, VariableType variableType, FullWorkspace fullWorkspace, Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null);

        /// <summary>
        /// Recalculate the BOE Labor given a task/workspace variable ID
        /// </summary>
        /// <param name="variableID">task/workspace variable ID</param>
        /// <param name="variableType">Variable type</param>
        /// <param name="fullWorkspace">workspace</param>
        /// <param name="incomingBoeTaskElements">Associated Task elements.</param>
        /// <returns>BOE Task Elements that need to be updated</returns>
        Collection<BoeTaskElementDTO> RecalculateLaborWithVariable(int variableID, VariableType variableType, FullWorkspace fullWorkspace, Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null);

        /// <summary>
        /// Recalculate BOE Labor given a wbs ID
        /// </summary>
        /// <param name="wbsToCheck">The WBS to check.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <param name="fullWorkspace">The full workspace.</param>
        /// <param name="incomingBoeTaskElements">The incoming boe task elements.</param>
        /// <param name="incomingWorkspaceVariables">The incoming workspace variables.</param>
        /// <returns>
        /// list of BOEs to update
        /// </returns>
        
        List<BoeTaskElementDTO> RecalculateLaborWithWBS(FullWbs wbsToCheck, VariableType variableType, FullWorkspace fullWorkspace, Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null);

        /// <summary>
        /// Recalculate the BOE Labor given a task element.
        /// </summary>
        /// <param name="boeTaskElement">Task element to be recalculated.</param>
        /// <param name="variableType">Recalc using workspace or task variable.</param>
        /// <param name="fullWorkspace">Workspace object.</param>
        /// <param name="incomingBoeTaskElements">Task elements.</param>
        /// <param name="costPrecisionChanged">True if the cost precision has changed. This will only occur during a task element copy from a different workspace with a different cost precision.</param>
        /// <returns>Recalculated task element.</returns>
        BoeTaskElementDTO RecalculateLaborWithTaskElement(BoeTaskElementDTO boeTaskElement, VariableType variableType, FullWorkspace fullWorkspace, Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, bool costPrecisionChanged = false, bool hoursPrecisionChanged = false);

        /// <summary>
        /// Recalculate labor for a BOE
        /// </summary>
        /// <param name="boe"></param>
        Collection<BoeTaskElementDTO> RecalculateLaborWithBoe(FullBoe boe, VariableType variableType, FullWorkspace fullWorkspace, Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null);

        /// <summary>
        /// Adjusts the labor totals.
        /// </summary>
        /// <param name="boeTaskElement">The boe task element.</param>
        /// <param name="MOQTotal">The moq total.</param>
        void AdjustLaborTotals(BoeTaskElementDTO boeTaskElement, decimal MOQTotal);
    }
}
