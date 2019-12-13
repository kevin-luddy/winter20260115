IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposalType]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProposalType]
	**		Desc:	Delete Proposal Type LU values 
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

	DELETE FROM dbo.[ProposalTypeLU]
		WHERE ProposalTypeId = @Id

GO