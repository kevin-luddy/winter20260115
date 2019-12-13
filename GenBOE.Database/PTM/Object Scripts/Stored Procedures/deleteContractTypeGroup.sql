IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteContractTypeGroup]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteContractTypeGroup];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteContractTypeGroup]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteContractTypeGroup]
	**		Desc:	Delete Contract Type Group LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.ContractTypeGroupXREF
		WHERE ContractTypeGroupId = @Id

	DELETE FROM dbo.[ContractTypeGroupLU]
		WHERE ContractTypeGroupId = @Id

GO