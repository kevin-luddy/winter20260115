IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertContractType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertContractType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertContractType]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertContractType]
	**		Desc:	Insert/Update Contract Type LU values 
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

	IF @Id  < 0 
		/* Insert */
		BEGIN
			SELECT @Id = MAX(ContractTypeId) + 1 FROM [dbo].[ContractTypeLU]
			
			INSERT INTO [dbo].[ContractTypeLU]
						(
							ContractTypeId,
							ContractType,
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
			UPDATE [dbo].[ContractTypeLU]
			   SET 
					ContractType = @Text,
					IsActive = @IsActive
				WHERE 
					ContractTypeId = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO