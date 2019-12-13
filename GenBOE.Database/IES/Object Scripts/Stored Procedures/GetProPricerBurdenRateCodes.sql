IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetProPricerBurdenRateCodes]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetProPricerBurdenRateCodes];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetProPricerBurdenRateCodes]
(
	@RevisionID INT
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[GetProPricerBurdenRateCodes]
**		Desc: Get ProPricer Burden Rate Codes
**		
**
**		Auth: brunworg
**		Date: 3/26/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Added ESCAL burden element column.
**		08/03/2017	brunworg			Updated to reflect new data model with
**										revisionId in BurdenPoolLU table.
*******************************************************************************/
SET NOCOUNT ON 

SELECT * FROM (
  SELECT bplu.[BurdenPool], bplu.[Description]
	,belu.[BurdenElement] as BurdenElement, rc.[RateCode]
  FROM [dbo].[Revision] rev
  JOIN [dbo].[BurdenPoolLU] bplu on bplu.RevisionId = rev.[ID]
  JOIN [dbo].[ProPricerBurdenRateMap] map on map.[BurdenPoolID] = bplu.[ID]
  JOIN [dbo].[RateCode] rc on map.[RateCodeId] = rc.[ID]
  JOIN [dbo].[BurdenElementLU] belu on map.[BurdenElementID] = belu.[ID]
  WHERE rev.[ID] = @RevisionID) as src
  PIVOT (max(src.[RateCode]) for src.[BurdenElement] in ([ESCAL],[OH Dev],[OH Prod],[OH FBM],[OH LVS],
			[OH Hunts],[OH Offsite],[OH Michoud],[OH Mich MH],[OH Prg Uni],[OH Pro DIR],
			[OH Pro MSC],[OH Pro Serv],[OH On Serv],[OH Off Serv],[OH PH4],[OH PH5],
			[Fringe],[Fringe T2],[FRG Serv Corp],[FRG Serv LMOS],[G&A],[G&A T2],
			[FCCOM Dev],[FCCOM Prod],[FCCOM FBM],[FCCOM LVS],[FCCOM Hunt],[FCCOM Ofst],
			[FCCOM Mich],[FCM Prg Un],[FCCOM Proc],[FCCOM ProM],[FCCOM G&A],[FCM T2 G&A],
			[FCCM IWTA],[FCCM PH2],[FCCM PH3],[FCCM PH4],[Fee/Prft])) as piv
ORDER BY BurdenPool;

GO
