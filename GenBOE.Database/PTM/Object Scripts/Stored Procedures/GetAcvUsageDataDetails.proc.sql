IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAcvUsageDataDetails]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].GetAcvUsageDataDetails;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAcvUsageDataDetails]  AS
	/******************************************************************************
	**		 
	**		Name: [GetAcvUsageDataDetails]
	**		Desc: SSRS: Create Acv Usage Data Report - Details
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
		SELECT c.PtmTrackingNumber, c.PtmTitle, c.WorkspaceId, c.WorkspaceShortName, c.WorkspaceName,
				cU.NTID AS CvAuthorNtid, cU.DisplayName AS CvAuthorName, -- THIS WILL NEED TO CHANGE ONCE THE MULTIPLE AUTHORS ARE REDONE
				c.CreatedDT AS CostVolumeCreatedDate, c.UpdateDT AS CostVolumeUpdateDate,
				p.ProposalTitle, p.UpdateDate AS PtmUpdateDate, pS.ProposalStatus, p.DateCreated AS PtmCreatedDate, p.DateAssigned AS AssignedDate,
				pT.ProposalType, p.IsIWTA, p.ProgramName, p.Customer, pCT.CustomerType, pIs.ISGSRole, pR.RequestType, p.RFPNumber, lob.LineOfBusinessName,
				pA.ProgramAreaName, pp.PricingTool, p.PricingToolName, pb.BOETool, p.BOEToolName, pC.CostVolumeTool, p.CostVolumeToolName, p.AnticipatedDeliveryDate,
				p.EstimatedProposalValue, p.IsActive, pAuthor.DisplayName AS PtmAuthor, RFPIssuedDate, RFPReceivedDate, pCG.ContractTypeGroup, p.WorkflowStatus
				FROM ACV.dbo.CostVolume c, genTrac.dbo.Proposal p, genTrac.dbo.LineOfBusinessLU lob, ACV.dbo.[User] as cU, genTrac.dbo.ProposalStatusLU pS,
					genTrac.dbo.ProposalTypeLU pT, genTrac.dbo.CustomerTypeLU pCT, genTrac.dbo.ISGSRoleLU pIs, genTrac.dbo.RequestTypeLU pR,
					genTrac.dbo.ProgramAreaLU pA, genTrac.dbo.PricingToolLU pP, genTrac.dbo.BOEToolLU pB, genTrac.dbo.CostVolumeToolLU pC, 
					genTrac.dbo.genTRACUser pAuthor, genTrac.dbo.ContractTypeGroupLU pCG
				WHERE		
					c.PtmTrackingNumber = p.ProposalTrackingID	
					AND p.LineOfBusinessID = lob.LineOfBusinessID
					AND c.AuthorId = cU.Id
					AND p.ProposalStatusID = pS.ProposalStatusID
					AND p.ProposalTypeID = pT.ProposalTypeID
					AND p.CustomerTypeID = pCT.CustomerTypeID
					AND p.ISGSRoleID = pIs.ISGSRoleID
					AND p.RequestTypeID = pR.RequestTypeID
					AND p.ProgramAreaID = pA.ProgramAreaID
					AND p.PricingToolID = pP.PricingToolID
					AND p.BOEToolID = pB.BOEToolID
					AND p.CostVolumeToolID = pC.CostVolumeToolID
					AND p.CreatedByUserID = pAuthor.UserID
					AND p.ContractTypeGroupID = pCG.ContractTypeGroupID

GRANT EXECUTE ON OBJECT::dbo.GetAcvUsageDataDetails TO generationReporter;
GO
