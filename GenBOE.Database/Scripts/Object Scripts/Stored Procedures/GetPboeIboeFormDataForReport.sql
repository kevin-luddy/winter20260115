IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPboeIboeFormDataForReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].GetPboeIboeFormDataForReport;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetPboeIboeFormDataForReport]  AS
	/******************************************************************************
	**		 
	**		Name: [GetPboeIboeFormDataForReport]
	**		Desc: SSRS: Create PBOE / IBOE Data Report
	**			
	**		
	**
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		3/20/24		Dusan				Initial Creation
	*******************************************************************************/

	SET NOCOUNT ON
		SELECT * FROM (
			SELECT 
					w.WorkspaceShortName, w.WorkspaceName, w.TrackingNumber, 'PBOE' AS FormType,
					pboe.FormName, pboe.UpdateDT AS UpdateDate, pboe.Poc AS 'Subcontract Administrator', pboe.Approver AS 'Subcontract Proposal Manager',
					'' AS 'Prepared By', '' AS 'Approved By'
				FROM Workspace w, BOEFormPBOE pboe
					WHERE w.WorkspaceID = pboe.WorkspaceID
			UNION
			SELECT 
					w.WorkspaceShortName, w.WorkspaceName, w.TrackingNumber, 'IBOE' AS FormType,
					iboe.FormName, iboe.UpdateDT AS UpdateDate, '' AS 'Subcontract Administrator', '' AS 'Subcontract Proposal Manager',
					iboe.Poc AS 'Prepared By', iboe.Approver AS 'Approved By'
				FROM Workspace w, BOEFormIBOE iboe
					WHERE w.WorkspaceID = iboe.WorkspaceID
				) a
		ORDER BY TrackingNumber, FormType

GRANT EXECUTE ON OBJECT::dbo.GetPboeIboeFormDataForReport TO generationReporter;
GO

