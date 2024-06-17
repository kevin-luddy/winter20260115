IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertCommonDisclosureSkillMixviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertCommonDisclosureSkillMixviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT 1 FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_CommonDisclosureSkillMix' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_CommonDisclosureSkillMix];
GO


CREATE TYPE [dbo].[TT_CommonDisclosureSkillMix] AS TABLE(
		[Rationale] varchar(255) NULL
		,[Included] bit DEFAULT 0
		,[ProposedHours] decimal(11, 2) NULL
		,[HistoricalHours] decimal(11, 2) NULL
		,[BOESkillMix] decimal(5, 2) NULL
		,[LaborSkillMix] decimal(5, 2) NULL
		,[ResourceID] [varchar](max) NULL
		,[BusinessResourceID] [varchar](max) NULL
		,[SkillMixID] int NULL
);
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertCommonDisclosureSkillMixviaTableParameter]
(
	@CommonDisclosureSkillMixTableParameter [dbo].[TT_CommonDisclosureSkillMix] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertCommonDisclosureSkillMixviaTableParameter]
**		Desc: Insert/Update data into Common Disclosure SkillMix Table
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
**      06/16/24	e302876				PROPH-2018 Common Disclosure Skill Mix DB Table
*******************************************************************************/
BEGIN
	DECLARE @DistinctSkillMixID int
	SELECT @DistinctSkillMixID = SkillMixID
	FROM (
		SELECT DISTINCT SkillMixID
		FROM @CommonDisclosureSkillMixTableParameter
	) AS temp_CommonDisclosureSkillMix

	DELETE FROM [dbo].[CommonDisclosureSkillMix]
	WHERE [SkillMixID] = @DistinctSkillMixID

	INSERT INTO [dbo].[CommonDisclosureSkillMix]
		([Rationale]
		 ,[Included]
		 ,[ProposedHours]
		 ,[HistoricalHours]
		 ,[BOESkillMix]
		 ,[LaborSkillMix]
		 ,[ResourceID]
		 ,[BusinessResourceID]
		 ,[SkillMixID]
		 )
	SELECT Rationale
		,Included
		,ProposedHours
		,HistoricalHours
		,BOESkillMix
		,LaborSkillMix
		,ResourceID
		,BusinessResourceID
		,SkillMixID
	FROM @CommonDisclosureSkillMixTableParameter
END

IF @@ERROR = 0
	SELECT COUNT(*) FROM @CommonDisclosureSkillMixTableParameter
GO