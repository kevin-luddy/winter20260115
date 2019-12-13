IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceRMSTravelEscalationRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceRMSTravelEscalationRate];
GO

CREATE PROCEDURE [dbo].[upsertWorkspaceRMSTravelEscalationRate]
(
@TravelEscalationRateID int,
@WorkspaceID int,
@UpdateDT datetime2,
@Year int,
@Escalation decimal(7, 5),
@PerDiemEscalation DECIMAL(7,5),
@MiscEscalation DECIMAL(7,5)
)
AS
/******************************************************************************
**		 
**		Name: [upsertWorkspaceRMSTravelNonzoneFeesAndCosts]
**		Desc: Update the Escalation Rate for an RMS at Workspace Level.
**
**		Auth: Tim Wilson
**		Date: 10/31/16
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		12/10/2016	Dusan				Added PerDiem and Misc Rates
******************************************************************************/

SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF @TravelEscalationRateID  < 0  /*Insert Record*/
	BEGIN	
		/*Year must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[WorkspaceRMSTravelEscalationRate] WHERE [Year] = @Year AND [WorkspaceID] = @WorkspaceID)
		BEGIN
			SET @ErrorMessage =   'There is already a Workspace Travel Escalation Rate with Year: ' + CAST (@Year AS char(4))
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END

		DECLARE @InsertedEscalation AS Table (TravelEscalationRateID int)
		INSERT INTO [dbo].[WorkspaceRMSTravelEscalationRate]
			   (
				[UpdateDT]
			    ,[WorkspaceID]
				,[Year]
				,[Escalation]
				,[PerDiemRate]
				,[MiscRate]
			   )
		OUTPUT inserted.TravelEscalationRateID INTO @InsertedEscalation            
		VALUES
			   (
			   @UpdateDT,
			   @WorkspaceID,
			   @Year,
			   @Escalation,
			   @PerDiemEscalation,
			   @MiscEscalation
			   )
		SELECT @TravelEscalationRateID = TravelEscalationRateID FROM @InsertedEscalation
	END
ELSE
	/*Update*/
	BEGIN
		-- Cannot check UpdateDT, since we are allowing this to be explicitly set by the front-end to keep track if this row is equal to the system row
		
		/*Year must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[WorkspaceRMSTravelEscalationRate] 
					WHERE  [Year] = @Year AND [WorkspaceID] = @WorkspaceID AND
					TravelEscalationRateID <> @TravelEscalationRateID)
		BEGIN
			SET @ErrorMessage =   'There is already a Workspace Travel Escalation Rate with Year: ' + CAST (@Year AS char(4))
			RAISERROR (
				@ErrorMessage, -- Message text.
				11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END
				
		UPDATE [dbo].[WorkspaceRMSTravelEscalationRate]
		SET [Year]=@Year, 
			[Escalation]=@Escalation, 
			[PerDiemRate]=@PerDiemEscalation,
			[MiscRate]=@MiscEscalation,
			[UpdateDT] = @UpdateDT
		WHERE 
			[TravelEscalationRateID]=@TravelEscalationRateID 
				
	END

IF @@ERROR = 0
	SELECT	@TravelEscalationRateID AS TravelEscalationRateID
GO