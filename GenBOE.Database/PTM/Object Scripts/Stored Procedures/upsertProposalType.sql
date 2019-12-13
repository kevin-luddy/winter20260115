IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalType]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProposalType]
	**		Desc:	Insert/Update Proposal Type LU values 
	**			
	**		
	**
	**		Auth: Dusan Palider
	**		Date: 7/17/2016
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].[ProposalTypeLU]
						(
							ProposalType,
							IsActive
						)
				OUTPUT inserted.ProposalTypeId INTO @Inserted
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
			UPDATE [dbo].[ProposalTypeLU]
			   SET 
					ProposalType = @Text,
					IsActive = @IsActive
				WHERE 
					ProposalTypeId = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO