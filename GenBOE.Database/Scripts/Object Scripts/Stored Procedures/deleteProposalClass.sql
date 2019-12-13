IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalClass]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalClass];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposalClass]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProposalClass]
	**		Desc:	Delete Proposal Class LU values 
	**			
	**		
	**
	**		Auth: Tim Wilson
	**		Date: 5/14/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.ProposalClassLU
		WHERE ProposalClassId = @Id

GO
