IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertCommonDisclosureSkillMixviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertCommonDisclosureSkillMixviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT 1 FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_CommonDisclosureSkillMix' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_CommonDisclosureSkillMix];
GO

CREATE TYPE [dbo].[TT_CommonDisclosureSkillMix] AS TABLE(
		[Rationale] varchar(255) NOT NULL
		,[Included] bit DEFAULT 0 NOT NULL
		,[ProposedHours] decimal(11, 2) NOT NULL
		,[HistoricalHours] decimal(11, 2) NOT NULL
		,[BOESkillMix] decimal(5, 2) NOT NULL
		,[LaborSkillMix] decimal(5, 2) NOT NULL
		,[ResourceID] [varchar](20) NOT NULL
		,[BusinessResourceID] [varchar](20) NOT NULL
		,[BOEID] int NOT NULL
		,[BOETaskElementID] int NOT NULL
		,[MOQTypeSelectionID] int NOT NULL
		,[IsPercentLocked] bit NOT NULL
		,[IsUserInput] bit NOT NULL
		/* OrderID is automatically added in the code, so it HAS to be last */
		,[OrderID] int NOT NULL
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
**		Auth: Breanne Nowicki
**		Date: 6/2024
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**      06/16/24	e302876				PROPH-2018 Common Disclosure Skill Mix DB Table
**	    07/10/24    e302876             PROPH-2157: Drop [SkillMixID] Column from Common Disclosure Skill Mix Table
**		07/11/24	twilson3			proph-2166 Missing Column
*******************************************************************************/
BEGIN
	DECLARE @DistinctMOQTypeSelectionID int
	SELECT @DistinctMOQTypeSelectionID = MOQTypeSelectionID
	FROM (
		SELECT DISTINCT MOQTypeSelectionID
		FROM @CommonDisclosureSkillMixTableParameter
	) AS temp_CommonDisclosureSkillMix

	DELETE FROM [dbo].[CommonDisclosureSkillMix]
	WHERE [MOQTypeSelectionID] = @DistinctMOQTypeSelectionID

	INSERT INTO [dbo].[CommonDisclosureSkillMix]
		([Rationale]
		 ,[Included]
		 ,[ProposedHours]
		 ,[HistoricalHours]
		 ,[BOESkillMix]
		 ,[LaborSkillMix]
		 ,[ResourceID]
		 ,[BusinessResourceID]
		 ,[BOEID]
		 ,[BOETaskElementID]
		 ,[MOQTypeSelectionID]
		 ,[IsPercentLocked]
		 ,[IsUserInput]
		)
	SELECT T.[Rationale]
		 ,T.[Included]
		 ,T.[ProposedHours]
		 ,T.[HistoricalHours]
		 ,T.[BOESkillMix]
		 ,T.[LaborSkillMix]
		 ,T.[ResourceID]
		 ,T.[BusinessResourceID]
		 ,T.[BOEID]
		 ,T.[BOETaskElementID]
		 ,T.[MOQTypeSelectionID]
		 ,T.[IsPercentLocked]
		 ,T.[IsUserInput]
	FROM @CommonDisclosureSkillMixTableParameter T
END

IF @@ERROR = 0
	SELECT COUNT(*) FROM @CommonDisclosureSkillMixTableParameter
GO