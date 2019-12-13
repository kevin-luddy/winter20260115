IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteContractType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteContractType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteContractType]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteContractType]
	**		Desc:	Delete Contract Type LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/14/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.[ContractTypeLU]
		WHERE ContractTypeId = @Id

GO