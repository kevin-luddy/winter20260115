IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProgramArea]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProgramArea];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProgramArea]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT,
	@ParentId	INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProgramArea]
	**		Desc:	Insert/Update Program Area LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			----------------------------------------
	**		5/21/2018	twilson3			BOEJ-3363 Remove Long Text
	**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].[ProgramAreaLU]
						(
							[ProgramAreaName],
							IsActive,
							[LineOfBusinessID]
						)
				OUTPUT inserted.[ProgramAreaID] INTO @Inserted
				VALUES
						(
							@Text,
							@IsActive,
							@ParentId
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			UPDATE [dbo].[ProgramAreaLU]
			   SET 
					[ProgramAreaName] = @Text,
					IsActive = @IsActive,
					[LineOfBusinessID] = @ParentId
				WHERE 
					[ProgramAreaID] = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO