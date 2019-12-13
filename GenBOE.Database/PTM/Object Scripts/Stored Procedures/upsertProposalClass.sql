IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalClass]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalClass];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalClass]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProposalClass]
	**		Desc:	Insert/Update Proposal Class LU values 
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

			INSERT INTO [dbo].ProposalClassLU
						(
							ProposalClass,
							IsActive
						)
				OUTPUT inserted.ProposalClassId INTO @Inserted
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
			UPDATE [dbo].ProposalClassLU
			   SET 
					ProposalClass = @Text,
					IsActive = @IsActive
				WHERE 
					ProposalClassID = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO