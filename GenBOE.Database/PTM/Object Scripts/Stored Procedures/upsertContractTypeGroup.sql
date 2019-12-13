IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertContractTypeGroup]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertContractTypeGroup];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertContractTypeGroup]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertContractTypeGroup]
	**		Desc:	Insert/Update Contract Type Group LU values 
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

			INSERT INTO [dbo].[ContractTypeGroupLU]
						(
							ContractTypeGroup,
							IsActive
						)
				OUTPUT inserted.ContractTypeGroupId INTO @Inserted
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
			UPDATE [dbo].[ContractTypeGroupLU]
			   SET 
					ContractTypeGroup = @Text,
					IsActive = @IsActive
				WHERE 
					ContractTypeGroupId = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO