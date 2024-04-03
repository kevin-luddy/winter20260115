IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAcvUsageDataComplete]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].GetAcvUsageDataComplete;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAcvUsageDataComplete]  AS
	/******************************************************************************
	**		 
	**		Name: [GetAcvUsageDataComplete]
	**		Desc: SSRS: Create Acv Usage Data Report - Complete Records Only
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
		SELECT YEAR(pc.ProposalSubmittalDate) AS Year, lob.LineOfBusinessName AS Lob, COUNT(1) AS Count
				FROM ACV.dbo.CostVolume c, genTrac.dbo.Proposal p, genTrac.dbo.LineOfBusinessLU lob, genTrac.dbo.ProposalChecklist pc
				WHERE		
					c.PtmTrackingNumber = p.ProposalTrackingID	
					AND p.LineOfBusinessID = lob.LineOfBusinessID
					AND p.ProposalID = pc.ProposalID
					AND pc.ProposalSubmittalDate IS NOT NULL
				GROUP BY lob.LineOfBusinessName, YEAR(pc.ProposalSubmittalDate);

GRANT EXECUTE ON OBJECT::dbo.GetAcvUsageDataComplete TO generationReporter;
GO
