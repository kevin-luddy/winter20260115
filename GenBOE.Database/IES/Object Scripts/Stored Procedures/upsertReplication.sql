IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertReplication]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertReplication];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertReplication]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@From			varchar(50),
	@To				varchar(50)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertReplication]
	**		Desc:	Insert/Update a Rate Code Replication
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 5/29/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[RateCodeReplication]
						([UpdateDate]
						,[From]
						,[To]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@From
						,@To
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[RateCodeReplication] WHERE [ID] = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].[RateCodeReplication]
					   SET  UpdateDate = @UpdateDate
							,[From] = @From
							,[To] = @To
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Rate Code Replication with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO