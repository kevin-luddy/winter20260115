IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceRMSTravelNonzoneFeesAndCosts]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceRMSTravelNonzoneFeesAndCosts];
GO

CREATE PROCEDURE [dbo].[upsertWorkspaceRMSTravelNonzoneFeesAndCosts]
(
@FeesAndCostsID int,
@WorkspaceID int,
@UpdateDT datetime2,
@ModeID int,
@TravelAgencyFee money,
@MiscOther money
)
AS
/******************************************************************************
**		 
**		Name: [upsertWorkspaceRMSTravelNonzoneFeesAndCosts]
**		Desc: Update the Travel Agency Fee and Misc/Other cost for an RMS Travel Nonzone Mode.
**
**		Auth: Tim Wilson
**		Date: 10/31/16
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
******************************************************************************/

SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF @FeesAndCostsID  < 0  /*Insert Record*/
	BEGIN	
		/*ModeID must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE [ModeID] = @ModeID AND [WorkspaceID] = @WorkspaceID)
		BEGIN
			SET @ErrorMessage =   'There is already a Workspace Travel NonZone Fees and Costs with Mode ID: ' + CAST (@ModeID AS varchar(4))
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END

		DECLARE @InsertedFee AS Table (FeesAndCostsID int)
		INSERT INTO [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts]
			   (
				[UpdateDT]
			    ,[WorkspaceID]
				,[ModeID]
				,[TravelAgencyFee]
				,[MiscOther]
			   )
		OUTPUT inserted.FeesAndCostsID INTO @InsertedFee            
		VALUES
			   (
			   @UpdateDT,
			   @WorkspaceID,
			   @ModeID,
			   @TravelAgencyFee,
			   @MiscOther
			   )
		SELECT @FeesAndCostsID = FeesAndCostsID FROM @InsertedFee
	END
ELSE
	/*Update*/
	BEGIN
		-- Cannot check UpdateDT, since we are allowing this to be explicitly set by the front-end to keep track if this row is equal to the system row
		
		/*ModeID must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] 
					WHERE  [ModeID] = @ModeID AND [WorkspaceID] = @WorkspaceID AND
					FeesAndCostsID <> @FeesAndCostsID)
		BEGIN
			SET @ErrorMessage =   'There is already a Workspace Travel NonZone Fees and Costs with Mode ID: ' + CAST (@ModeID AS varchar(4))
			RAISERROR (
				@ErrorMessage, -- Message text.
				11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END

		UPDATE [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts]
		SET [TravelAgencyFee]=@TravelAgencyFee, 
			[MiscOther]=@MiscOther, 
			[ModeID] = @ModeID,
			[UpdateDT] = @UpdateDT
		WHERE [FeesAndCostsID]=@FeesAndCostsID
	END

IF @@ERROR = 0
	SELECT	@FeesAndCostsID AS FeesAndCostsID
GO