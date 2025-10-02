IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertSkillMixSummaryviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertSkillMixSummaryviaTableParameter];
GO

-- Drop type
IF  EXISTS (SELECT 1 FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_SkillMixSummary' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_SkillMixSummary];
GO

CREATE TYPE [dbo].[TT_SkillMixSummary] AS TABLE(
	[Rationale] [varchar](255) NOT NULL,
	[Included] [bit] NOT NULL DEFAULT ((0)),
	[ProposedHours] [decimal](11, 2) NOT NULL,
	[HistoricalHours] [decimal](11, 2) NOT NULL,
	[ResourceHours] [decimal](11, 2) NOT NULL,
	[BusinessResourceHours] [decimal](11, 2) NOT NULL,
	[BOESkillMix] [decimal](5, 2) NOT NULL,
	[LaborSkillMix] [decimal](5, 2) NOT NULL,
	[ResourceID] [varchar](20) NOT NULL,
	[BusinessResourceID] [varchar](20) NOT NULL,
	[BOEID] [int] NOT NULL,
	[BOETaskElementID] [int] NOT NULL,
	[IsUserInput] [bit] NOT NULL,
	[OrderID] [int] NOT NULL
)
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertSkillMixSummaryviaTableParameter]
(
	@skillMixSummaryTableParameter [dbo].[TT_SkillMixSummary] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertSkillMixSummaryviaTableParameter]
**		Desc: Insert/Update data into SkillMix Summary Table
**			
**		
**
**		Auth: Oyetoro Oyeyemi
**		Date: 9/2025
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**      9/24/25		e378233 			PROPH-3302 Skill Mix Summary DB Table
*****************************************************************************/
BEGIN
	DECLARE @DistinctBOETaskElementID int
	SELECT @DistinctBOETaskElementID = BOETaskElementID
	FROM (
		SELECT DISTINCT BOETaskElementID
		FROM @SkillMixSummaryTableParameter
	) AS temp_SkillMixSummary

	DELETE FROM [dbo].[SkillMixSummary]
	WHERE [BOETaskElementID] = @DistinctBOETaskElementID

	INSERT INTO [dbo].[SkillMixSummary]
		([Rationale]
		 ,[Included]
		 ,[ProposedHours]
		 ,[HistoricalHours]
		 ,[ResourceHours]
		 ,[BusinessResourceHours]
		 ,[BOESkillMix]
		 ,[LaborSkillMix]
		 ,[ResourceID]
		 ,[BusinessResourceID]
		 ,[BOEID]
		 ,[BOETaskElementID]
		 ,[IsUserInput]
		)
	SELECT T.[Rationale]
		 ,T.[Included]
		 ,T.[ProposedHours]
		 ,T.[HistoricalHours]
		 ,T.[ResourceHours]
		 ,T.[BusinessResourceHours]
		 ,T.[BOESkillMix]
		 ,T.[LaborSkillMix]
		 ,T.[ResourceID]
		 ,T.[BusinessResourceID]
		 ,T.[BOEID]
		 ,T.[BOETaskElementID]
		 ,T.[IsUserInput]
	FROM @SkillMixSummaryTableParameter T
END

IF @@ERROR = 0
	SELECT COUNT(*) FROM @SkillMixSummaryTableParameter
GO