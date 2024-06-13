IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertSkillMixviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertSkillMixviaTableParameter];
GO

CREATE TYPE [dbo].[TT_SkillMix] AS TABLE(
		[Rationale] varchar(255) NULL
		,[Included] bit DEFAULT 0
		,[ProposedHours] decimal(11, 2) NULL
		,[HistoricalHours] decimal(11, 2) NULL
		,[BOESkillMix] decimal(5, 2) NULL
		,[LaborSkillMix] decimal(5, 2) NULL
		,[ResourceOld] [varchar](max) NULL
		,[ResourceNew] [varchar](max) NULL
		,[BOEID] int NULL
		,[BOETaskElementID] int NULL
		,[MOQTypeSelectionID] int NULL
);
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertSkillMixviaTableParameter]
(
	@SkillMixTableParameter [dbo].[TT_SkillMix] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [upsertSkillMixviaTableParameter]
**		Desc: Insert/Update data into SkillMix Table
**			
**		
**
**		Auth: Hyun Dong Lee
**		Date: 6/2024
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**      06/12/24	e374897				PROPH-2047 Initial upsert logic
*******************************************************************************/
BEGIN
	DECLARE @DistinctMOQTypeSelectionID int
	SELECT @DistinctMOQTypeSelectionID = MOQTypeSelectionID
	FROM (
		SELECT DISTINCT MOQTypeSelectionID
		FROM @SkillMixTableParameter
	) AS temp_SkillMix

	DELETE FROM [dbo].[SkillMix]
	WHERE [MOQTypeSelectionID] = @DistinctMOQTypeSelectionID

	INSERT INTO [dbo].[SkillMix]
		([Rationale]
		 ,[Included]
		 ,[ProposedHours]
		 ,[HistoricalHours]
		 ,[BOESkillMix]
		 ,[LaborSkillMix]
		 ,[ResourceOld]
		 ,[ResourceNew]
		 ,[BOEID]
		 ,[BOETaskElementID]
		 ,[MOQTypeSelectionID]
		 )
	SELECT Rationale
		,Included
		,ProposedHours
		,HistoricalHours
		,BOESkillMix
		,LaborSkillMix
		,ResourceOld
		,ResourceNew
		,BOEID
		,BOETaskElementID
		,MOQTypeSelectionID
	FROM @SkillMixTableParameter
END

IF @@ERROR = 0
	SELECT COUNT(*) FROM @SkillMixTableParameter
GO