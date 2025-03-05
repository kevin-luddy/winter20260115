// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Common.Calculations
{
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.DataBridge.Core.Misc;

	public interface IVariableSelectBOEtoSumCalculation
	{
		decimal GetTaskVarLabelTotal(OrdinaryVariableDto taskVar, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
		decimal GetTotalBasedOnBoeID(int boeId, System.Collections.ObjectModel.Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
		decimal GetTotalBasedOnCLINID(int clinId, System.Collections.ObjectModel.Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
		decimal GetTotalBasedOnWBSID(int wbsId, System.Collections.ObjectModel.Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
		decimal GetWorkspaceVarLabelTotal(WorkspaceVariableDTO workspaceVar, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc);
	}
}
