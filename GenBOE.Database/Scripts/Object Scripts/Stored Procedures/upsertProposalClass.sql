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
	**		Auth: Tim Wilson
	**		Date: 5/14/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			----------------------------------------
	**		5/21/2018	twilson3			BOEJ-3363 Fixed insertion of Proposal Class
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			SELECT @Id = MAX(ProposalClassId) + 1 FROM [dbo].ProposalClassLU
			
			INSERT INTO [dbo].ProposalClassLU
						(
							ProposalClassId,
							ProposalClass,
							IsActive
						)
				VALUES
						(
							@Id,
							@Text,
							@IsActive
						)
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