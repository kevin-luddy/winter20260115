IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProPricerRateCodeXref]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProPricerRateCodeXref];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProPricerRateCodeXref]
(
	@Id					INT,
	@UpdateDate			datetime2(7),
	@RateCodeID			INT,
	@Description		varchar(255),
	@RateCodeExtensionID	INT,
	@ResourceClassID	INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProPricerRateCodeXref]
	**		Desc:	Insert/Update a ProPricer Rate Code cross reference
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before upsert.
	**		8/22/2017	brunworg			Redesign Section and related tables.
	**		12/22/2017	brunworg			Add ResourceClassID field.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].ProPricerRateCodeXref
						([UpdateDate]
						,[RateCodeID]
						,[Description]
						,[RateCodeExtensionID]
						,[ResourceClassID]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
						,@RateCodeID
						,@Description
						,@RateCodeExtensionID
						,@ResourceClassID
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[ProPricerRateCodeXref] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].ProPricerRateCodeXref
					   SET  UpdateDate = @UpdateDate
						   ,RateCodeID = @RateCodeID
						   ,Description = @Description
						   ,RateCodeExtensionID = @RateCodeExtensionID
						   ,ResourceClassID = @ResourceClassID
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The ProPricer Rate Code Cross Reference with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
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