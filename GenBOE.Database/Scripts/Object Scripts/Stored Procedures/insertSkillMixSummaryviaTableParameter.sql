CREATE OR ALTER PROCEDURE [dbo].[insertSkillMixSummaryviaTableParameter]
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