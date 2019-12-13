IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBanner]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBanner];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertBanner]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@SelectedApps			varchar(255),
	@StartDate				datetime2(7),
	@HoursToShow			int,
	@BannerText				varchar(500),
	@TurnOffTicker			bit
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertBanner]
	**		Desc:	Insert/Update a Banner
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 3/14/2018
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

			INSERT INTO [dbo].[Banner]
						([UpdateDate]
						,[SelectedApps]
						,[StartDate]
						,[HoursToShow]
						,[BannerText]
						,[TurnOffTicker]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@SelectedApps
						,@StartDate
						,@HoursToShow
						,@BannerText
						,@TurnOffTicker
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[Banner] WHERE [ID] = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].[Banner]
					   SET  UpdateDate = @UpdateDate
							,[SelectedApps] = @SelectedApps
							,[StartDate] = @StartDate
							,[HoursToShow] = @HoursToShow
							,[BannerText] = @BannerText
							,[TurnOffTicker] = @TurnOffTicker
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Banner with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
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