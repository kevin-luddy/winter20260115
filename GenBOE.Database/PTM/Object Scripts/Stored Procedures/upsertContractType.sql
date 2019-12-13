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
	@IsActive	BIT,
	/*Comma separated list of Contract Type Group IDs with no comma at the end*/
	@GroupIds   VARCHAR(1000)
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
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].[ContractTypeLU]
						(
							ContractType,
							IsActive
						)
				OUTPUT inserted.ContractTypeId INTO @Inserted
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
			UPDATE [dbo].[ContractTypeLU]
			   SET 
					ContractType = @Text,
					IsActive = @IsActive
				WHERE 
					ContractTypeId = @Id
		END

	-- Update the XREFs to Groups
	DELETE FROM dbo.ContractTypeGroupXREF
		WHERE ContractTypeId = @Id

	INSERT INTO dbo.ContractTypeGroupXREF ([ContractTypeGroupID], [ContractTypeID]) 
			SELECT Item, @Id FROM [SplitString] (@GroupIds, ',', DEFAULT)

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO