IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertTravelEscalationRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertTravelEscalationRate];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertTravelEscalationRate]
(
@TravelEscalationRateID int,
@Year int,
@DevEscalation decimal (7,5),
@LMSIEscalation decimal (7,5),
@MiscRate DECIMAL(7,5),
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [upsertTravelEscalationRate]
**		Desc: Insert/Update Travel Escalation Rates for Travel
**				Year must be unique
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
**		9/16/11		dcanuso				WI 5163 Escalation Rate to 5 decimal points
**		12/10/2016	Dusan				Added MiscRate into the table
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE	@ErrorMessage varchar (500)

IF @TravelEscalationRateID < 0  /*Insert Record*/
	BEGIN
		/*Year must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[TravelEscalationRate] WHERE [Year] = @Year)
		BEGIN
			SET @ErrorMessage =   'There is already a Travel Escalation Rate with Year: ' + CAST (@Year AS char(4))
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END

		SET @UpdateDT = GETDATE()
		
		INSERT INTO [dbo].[TravelEscalationRate]
				   ([Year]
				   ,[DevEscalation]
				   ,[LMSIEscalation]
				   ,[MiscRate]
				   ,[UpdateDT])
		OUTPUT inserted.TravelEscalationRateID INTO @Inserted
		 VALUES
			   (@Year
			   ,@DevEscalation
			   ,@LMSIEscalation
			   ,@MiscRate
			   ,@UpdateDT)

		SELECT @TravelEscalationRateID = ID FROM @Inserted
			
	END
ELSE /*Update*/
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[TravelEscalationRate] WHERE TravelEscalationRateID = @TravelEscalationRateID) = @UpdateDT
		BEGIN	
		SET @UpdateDT = GETDATE()
			
		/*Year must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[TravelEscalationRate] 
					WHERE  [Year] = @Year AND 
					TravelEscalationRateID <> @TravelEscalationRateID)
		BEGIN
			SET @ErrorMessage =   'There is already a Travel Escalation Rate with Year: ' + CAST (@Year AS char(4))
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END

		UPDATE [dbo].[TravelEscalationRate]
		   SET [Year] = @Year
			  ,[DevEscalation] = @DevEscalation
			  ,[LMSIEscalation] = @LMSIEscalation
			  ,[MiscRate] = @MiscRate
			  ,[UpdateDT] = @UpdateDT
		WHERE 
			TravelEscalationRateID = @TravelEscalationRateID
		END
	END

IF @@ERROR = 0
	SELECT	@TravelEscalationRateID AS TravelEscalationRateID
GO