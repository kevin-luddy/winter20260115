IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAcvUsageData]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].GetAcvUsageData;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAcvUsageData]  AS
	/******************************************************************************
	**		 
	**		Name: [GetAcvUsageData]
	**		Desc: SSRS: Create Acv Usage Data Report
	**			
	**		
	**
	**		Auth: Don Canuso
	**		Date: 12/3/2014
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		3/20/24		Dusan				Initial Creation
	*******************************************************************************/

	SET NOCOUNT ON
		SELECT YEAR(c.CreatedDt) AS Year, lob.LineOfBusinessName AS Lob, COUNT(1) AS Count
				FROM ACV.dbo.CostVolume c, genTrac.dbo.Proposal p, genTrac.dbo.LineOfBusinessLU lob		
				WHERE		
					c.PtmTrackingNumber = p.ProposalTrackingID	
					AND p.LineOfBusinessID = lob.LineOfBusinessID	
				GROUP BY lob.LineOfBusinessName, YEAR(c.CreatedDT);


GRANT EXECUTE ON OBJECT::dbo.GetAcvUsageData TO generationReporter;
GO
