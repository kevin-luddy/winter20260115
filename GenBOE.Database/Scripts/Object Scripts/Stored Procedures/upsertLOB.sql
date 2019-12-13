IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertLOB]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertLOB];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertLOB]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertLOB]
	**		Desc:	Insert/Update LOB LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/14/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			----------------------------------------
	**		5/21/2018	twilson3			BOEJ-3363 Remove Long Text
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].[LineOfBusiness]
						(
							[LineOfBusinessName],
							IsActive
						)
				OUTPUT inserted.LineOfBusinessID INTO @Inserted
				VALUES
						(
							@Text,
							@IsActive
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			UPDATE [dbo].[LineOfBusiness]
			   SET 
					[LineOfBusinessName] = @Text,
					IsActive = @IsActive
				WHERE 
					LineOfBusinessID = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO