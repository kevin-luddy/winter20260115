// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Calculations
{
    using GenBOE.Dtos;

    public interface IVariableSelectBOEtoSumCalculation
    {
        decimal GetTaskVarLabelTotal(OrdinaryVariableDto taskVar, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
        decimal GetTotalBasedOnBoeID(int boeId, System.Collections.ObjectModel.Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
        decimal GetTotalBasedOnCLINID(int clinId, System.Collections.ObjectModel.Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
        decimal GetTotalBasedOnWBSID(int wbsId, System.Collections.ObjectModel.Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
        decimal GetWorkspaceVarLabelTotal(WorkspaceVariableDTO workspaceVar, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
    }
}
