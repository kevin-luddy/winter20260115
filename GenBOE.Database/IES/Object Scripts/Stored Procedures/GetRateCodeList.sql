IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetRateCodeList]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetRateCodeList];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetRateCodeList]
(
	@RevisionID INT,
	@StartYear  varchar(50) = NULL,
	@EndYear    varchar(50) = NULL
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[GetRateCodeList]
**		Desc: Get rate codes, rate code details, and rate values by year.
**            If @StartYear and @EndYear parameters are provided, return rates
**            between those years (inclusive).
**            Otherwise, return rates for all years.
**			 
**		
**
**		Auth: brunworg
**		Date: 3/26/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Added Government and Commercial 
**                                      Burden Pool columns.
**                                      Added pivot columns thru 2099.
**		07/03/2017	brunworg			Added StartYear and EndYear parameters.
**                                      Return each year/rate as separate rows.
**      07/03/2017	brunworg			Modified where clause to remove duplicate code.
**		08/21/2017	brunworg			Remove AlternateDescription, DataTypeID,
**										and DataFormat from RateCode table.
**		08/22/2017	brunworg			Redesign Section and related tables.
*******************************************************************************/
SET NOCOUNT ON 

SELECT clu.Description, rc.Description, rc.RateCode, ISNULL(s.Title,'') as Section,
	    ISNULL(rc.CobraRateSet,'') as CobraRateSet, rc.CobraCode1ID as CobraCode1, 
	    ISNULL(reslu.Description,'') as ResourceType, 
		ISNULL(bplu1.BurdenPool,'') as GovernmentBurdenPool, ISNULL(bplu2.BurdenPool,'') as CommercialBurdenPool, 
		ISNULL(rtlu.Description,'') as RateType, rcy.year, rcy.rate
FROM dbo.Revision rev
JOIN dbo.RateCode rc on rev.ID = rc.RevisionID
JOIN dbo.CategoryLU clu on rc.CategoryID = clu.ID
LEFT OUTER JOIN dbo.Section s on rc.SectionID = s.ID
LEFT OUTER JOIN dbo.ResourceTypeLU reslu on rc.ResourceTypeID = reslu.ID
LEFT OUTER JOIN dbo.BurdenPoolLU bplu1 on rc.GovernmentBurdenPoolID = bplu1.ID
LEFT OUTER JOIN dbo.BurdenPoolLU bplu2 on rc.CommercialBurdenPoolID = bplu2.ID
LEFT OUTER JOIN dbo.RateTypeLU rtlu on rc.RateTypeID = rtlu.ID
JOIN dbo.RateCodeYear rcy on rc.ID= rcy.RateCodeID
WHERE rev.ID = @RevisionID AND ((@StartYear IS NULL AND @EndYear IS NULL) OR (rcy.Year >= @StartYear AND rcy.Year <= @EndYear))
ORDER BY clu.Description, rc.RateCode, rcy.Year;

GO