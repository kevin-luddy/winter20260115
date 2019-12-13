IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertTravelMiscellaneousRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertTravelMiscellaneousRate];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertTravelMiscellaneousRate]
(
@TravelMiscRateID int,
@TransportationMode varchar(25),
@MiscellaneousRate money,
@SortCode tinyint,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [upsertTravelMiscellaneousRate]
**		Desc: Insert/Update Miscellaneous Rates for Travel
**				Transportation Mode and Sort Code must be 
**				individually unique
**
**
**		
**
**		Auth: Don Canuso
**		Date: 6/21/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		9/12/11		dcanuso				Developer found error when saving - Need 
**										to add  AND LockedRate = 0
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE	@ErrorMessage varchar (500)



IF @TravelMiscRateID < 0  /*Insert Record*/
	BEGIN
		/*Transportation Mode must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[TravelMiscRate] WHERE TransportationMode = @TransportationMode/*Column Removed during Locking Redesign AND LockedRate = 0*/)
		BEGIN
			SET @ErrorMessage =   'There is already a Travel Miscellaneous Rate with Transportation Mode: ' + @TransportationMode
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END

		/*Sort Mode must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[TravelMiscRate] WHERE SortCode = @SortCode/*Column Removed during Locking Redesign AND LockedRate = 0*/)
		BEGIN
			SET @ErrorMessage =   'There is already a Travel Miscellaneous Rate with Sort Code: ' + CAST(@SortCode AS varchar(10))
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END
		
		SET @UpdateDT = GETDATE()
		
		INSERT INTO [dbo].[TravelMiscRate]
           ([TransportationMode]
           ,[MiscellaneousRate]
           ,[SortCode]
           ,[MiscRateInUse]
           ,[UpdateDT])
		OUTPUT inserted.TravelMiscRateID INTO @Inserted
	    VALUES
           (@TransportationMode
           ,@MiscellaneousRate
           ,@SortCode
           ,0 /*New Rate so Not In Use - MiscRateInUse*/
           ,@UpdateDT)


		SELECT @TravelMiscRateID = ID FROM @Inserted
			
	END
ELSE /*Update*/
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[TravelMiscRate] WHERE TravelMiscRateID = @TravelMiscRateID) = @UpdateDT
		BEGIN	
		SET @UpdateDT = GETDATE()
			
		/*Transportation Mode must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[TravelMiscRate] 
					WHERE TransportationMode = @TransportationMode AND 
					TravelMiscRateID <> @TravelMiscRateID/*Column Removed during Locking Redesign AND LockedRate = 0*/)
		BEGIN
			SET @ErrorMessage =   'There is already a Travel Miscellaneous Rate with Transportation Mode: ' + @TransportationMode
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END

		/*Sort Mode must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[TravelMiscRate] 
					WHERE SortCode = @SortCode AND 
					TravelMiscRateID <> @TravelMiscRateID/*Column Removed during Locking Redesign AND LockedRate = 0*/)
		BEGIN
			SET @ErrorMessage =   'There is already a Travel Miscellaneous Rate with Sort Code: ' + CAST(@SortCode AS varchar(10))
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END

		UPDATE [dbo].[TravelMiscRate]
			SET [TransportationMode] = @TransportationMode,
				[MiscellaneousRate] =  @MiscellaneousRate,
				[SortCode] = @SortCode,
				[UpdateDT] = @UpdateDT
		WHERE 
			TravelMiscRateID = @TravelMiscRateID
		END
	END

IF @@ERROR = 0
	SELECT	@TravelMiscRateID AS TravelMiscRateID
GO