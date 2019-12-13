IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRequestTypes]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRequestTypes];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertRequestTypes]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertRequestTypes]
	**		Desc:	Insert/Update Request Types LU values 
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

			INSERT INTO [dbo].RequestTypeLU
						(
							RequestType,
							IsActive
						)
				OUTPUT inserted.RequestTypeID INTO @Inserted
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
			UPDATE [dbo].RequestTypeLU
			   SET 
					RequestType = @Text,
					IsActive = @IsActive
				WHERE 
					RequestTypeID = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO


