IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertSkillMixviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertSkillMixviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT 1 FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_SkillMix' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_SkillMix];
GO

CREATE TYPE [dbo].[TT_SkillMix] AS TABLE(
		[Rationale] varchar(255) NOT NULL
		,[Included] bit DEFAULT 0
		,[ProposedHours] decimal(11, 2) NOT NULL
		,[HistoricalHours] decimal(11, 2) NOT NULL
		,[BOESkillMix] decimal(5, 2) NOT NULL
		,[LaborSkillMix] decimal(5, 2) NOT NULL
		,[ResourceOld] [varchar](20) NOT NULL
		,[ResourceNew] [varchar](20) NOT NULL
		,[BOEID] int NOT NULL
		,[BOETaskElementID] int NOT NULL
		,[IsUserInput] bit DEFAULT 0 NOT NULL
		/* OrderID is automatically added in the code, so it HAS to be last */
		,[OrderID] [int] NOT NULL
);
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertSkillMixviaTableParameter]
(
	@SkillMixTableParameter [dbo].[TT_SkillMix] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertSkillMixviaTableParameter]
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
**      06/12/24	e374897				PROPH-2047 Initial insert logic
**		07/11/24	twilson3			proph-2166 Missing Column
**		10/01/24	e405721				PROPH-2394 SkillMix V2 Removing Columns
*******************************************************************************/
BEGIN
	DECLARE @DistinctBOETaskElementID int
	SELECT @DistinctBOETaskElementID = BOETaskElementID
	FROM (
		SELECT DISTINCT BOETaskElementID
		FROM @SkillMixTableParameter
	) AS temp_SkillMix

	DELETE FROM [dbo].[SkillMix]
	WHERE [BOETaskElementID] = @DistinctBOETaskElementID

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
		 ,[IsUserInput]
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
		,IsUserInput
	FROM @SkillMixTableParameter
END

IF @@ERROR = 0
	SELECT COUNT(*) FROM @SkillMixTableParameter
GO