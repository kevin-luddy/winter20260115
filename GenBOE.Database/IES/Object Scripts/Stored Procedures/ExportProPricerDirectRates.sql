 IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportProPricerDirectRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ExportProPricerDirectRates];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ExportProPricerDirectRates]
(
	@RevisionID INT
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[ExportProPricerDirectRates]
**		Desc: Generate ProPricer Direct Rate Export data for both Government
**            and Commercial rates.
**            The results will be a UNION of the following:
**            1) Rate Codes that do not need Rate Code Extensions.
**            2) Rate Codes that are mapped by activity type and
**               need to be expanded using the Rate Code Extensions.
**            3) Pro Pricer Equivalent Rate Codes.
**               The rates for these activity types will be
**               exported a second time substituting the first 4 characters
**               of the rate code as follows.
**	    	      XXDD => XADD
**	    	      XXLM => XMLM
**	    	      XXZD => XCZD
**	    	      XXZP => XCZP
**		
**
**		Auth: brunworg
**		Date: 3/20/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Updated to reflect new data model.
**		7/12/2017	brunworg			Changed RateCodeYear - Year field
**                                      from varchar(50) to int.
**		08/22/2017	brunworg			Redesign Section and related tables.
*******************************************************************************/
SET NOCOUNT ON 

SELECT reslu.[Description], '01/' + CAST(rcy.[Year] as varchar(4)) as StartDate, '12/' + CAST(rcy.[Year] as varchar(4)) as EndDate
	  ,rc.[RateCode] + ISNULL(rcelu.[RateCodeExtension],'') as Resource  
      ,xref.[Description]
      ,'' as ResourceClass
	  ,ISNULL(bplu1.BurdenPool, '') as GovernmentBurdenPool
	  ,ISNULL(bplu2.BurdenPool, '') as CommercialBurdenPool
	  ,rtlu.[Description]
	  ,'' as EffectiveDate
	  ,rcy.[Rate] as BaseRate
	  ,0 as Step
	  ,0 as Factor
  FROM [dbo].[Revision] rev
  JOIN [dbo].[RateCode] rc on rc.[RevisionID] = rev.[ID]
  JOIN [dbo].[ProPricerRateCodeXref] xref on rc.ID = xref.RateCodeID
  LEFT OUTER JOIN [dbo].[RateCodeExtensionLU] rcelu on xref.RateCodeExtensionID = rcelu.ID
  JOIN [dbo].[ResourceTypeLU] reslu on rc.[ResourceTypeID] = reslu.[ID]
  JOIN [dbo].[RateTypeLU] rtlu on rc.[RateTypeID] = rtlu.[ID]
  JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu1 on rc.[GovernmentBurdenPoolID] = bplu1.[ID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu2 on rc.[CommercialBurdenPoolID] = bplu2.[ID]
  WHERE rev.[ID] = @RevisionID
UNION
SELECT reslu.[Description], '01/' + CAST(rcy.[Year] as varchar(4)) as StartDate, '12/' + CAST(rcy.[Year] as varchar(4)) as EndDate
	  ,REPLACE(REPLACE(REPLACE(REPLACE(rc.[RateCode],'XXDD','XXAD'),'XXLM','XMLM'),'XXZD','XCZD'),'XXZP','XCZP') + ISNULL(rcelu.[RateCodeExtension],'') as Resource 
      ,xref.[Description]
      ,'' as ResourceClass
	  ,ISNULL(bplu1.BurdenPool, '') as GovernmentBurdenPool
	  ,ISNULL(bplu2.BurdenPool, '') as CommercialBurdenPool
	  ,rtlu.[Description]
	  ,'' as EffectiveDate
	  ,rcy.[Rate] as BaseRate
	  ,0 as Step
	  ,0 as Factor
  FROM [dbo].[Revision] rev
  JOIN [dbo].[RateCode] rc on rc.[RevisionID] = rev.[ID]
  JOIN [dbo].[ProPricerRateCodeXref] xref on rc.ID = xref.RateCodeID
  LEFT OUTER JOIN [dbo].[RateCodeExtensionLU] rcelu on xref.RateCodeExtensionID = rcelu.ID
  JOIN [dbo].[ResourceTypeLU] reslu on rc.[ResourceTypeID] = reslu.[ID]
  JOIN [dbo].[RateTypeLU] rtlu on rc.[RateTypeID] = rtlu.[ID]
  JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu1 on rc.[GovernmentBurdenPoolID] = bplu1.[ID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu2 on rc.[CommercialBurdenPoolID] = bplu2.[ID]
  WHERE rev.[ID] = @RevisionID AND LEFT(rc.RateCode,4) in ('XXDD', 'XXLM', 'XXZD', 'XXZP')
ORDER BY Resource, EndDate;

GO