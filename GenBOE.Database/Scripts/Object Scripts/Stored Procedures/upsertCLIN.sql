IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertCLIN]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertCLIN];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertCLIN]
(
@CLINID int,
@CLINNumber varchar(459),
@CLINTitle varchar(100),
@CLINStartDate date,
@CLINEndDate date,
@WorkspaceID int,
@UpdateDT datetime2,
@DisplayedCLINNumber varchar(50),
@ContractTypeID int
)
AS
/******************************************************************************
**		 
**		Name: [upsertCLIN]
**		Desc: Insert/Update CLIN data in Manage CLINs

**
**		
**
**		Auth: Don Canuso
**		Date: August 2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/19		dcanuso				RETURN was not working properly - corrected
**		9/13/10		dcanuso				Soft deletes removed
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		9/5/12		mbasquil			WI 10910 Added IsNull() checks to properly
**										handle null values	
**		9/13/12		mbasquil			WI 11267 updating to handle new field
**										DisplayedCLINNumber
**		9/27/12		dcanuso				Processing for CLIN Number now needs to
**										process Displayed CLIN Number
**		11/1/16		buckwalj			BOEJ-1484 Added ContractID column
**		4/25/2017	twilson3			BOEJ-2121 Project Map updates
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedCLIN AS Table (CLINID int)
DECLARE	@ErrorMessage varchar (500)

IF @CLINID < 0  /*Insert Record*/
	BEGIN
		IF EXISTS	(SELECT 1 FROM [dbo].[CLIN]
					WHERE	IsNull(DisplayedCLINNumber, -9999) = IsNUll(@DisplayedCLINNumber, -9999) AND
							WorkspaceID = @WorkspaceID
					)
		BEGIN
			SET @ErrorMessage =   'There is already a CLIN with Displayed CLIN Number ' + @DisplayedCLINNumber
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END
					
		ELSE
		BEGIN
			SET @UpdateDT = GETDATE()
			
			INSERT INTO [dbo].[CLIN]
				([CLINNumber]
				,[CLINTitle]
				,[CLINStartDate]
				,[CLINEndDate]
				,[WorkspaceID]
				,[UpdateDT]
				,[DisplayedCLINNumber]
				,[ContractTypeID])
			 OUTPUT inserted.CLINID INTO @InsertedCLIN
			 VALUES
				   (
				   @CLINNumber,
				   @CLINTitle,
				   @CLINStartDate,
				   @CLINEndDate,
				   @WorkspaceID,
				   @UpdateDT,
				   @DisplayedCLINNumber,
				   @ContractTypeID
				   )
		           
			  SELECT @CLINID = CLINID FROM @InsertedCLIN
		  END
		
END
ELSE /*Update*/
	BEGIN
		IF (SELECT UpdateDT 
				FROM [dbo].[CLIN] 
				WHERE	CLINID = @CLINID 
		) = @UpdateDT
			/*Update Dates Match*/
			BEGIN
			
				IF EXISTS	(SELECT 1 FROM [dbo].[CLIN]
								WHERE	IsNull(DisplayedCLINNumber, -9999) = IsNull(@DisplayedCLINNumber, -9999) AND
										WorkspaceID = @WorkspaceID AND
										CLINID <> @CLINID
					)
					BEGIN
						SET @ErrorMessage =   'There is already a CLIN with Displayed CLIN Number ' + @DisplayedCLINNumber
						RAISERROR (
							@ErrorMessage, -- Message text.
							11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
						RETURN
					END
				ELSE
					BEGIN			
						SET @UpdateDT = GETDATE()
						
						UPDATE [dbo].[CLIN]
						   SET [CLINNumber] = @CLINNumber
							  ,[CLINTitle] = @CLINTitle
							  ,[CLINStartDate] = @CLINStartDate
							  ,[CLINEndDate] = @CLINEndDate
							  ,[UpdateDT] = @UpdateDT
							  ,[DisplayedCLINNumber] = @DisplayedCLINNumber
							  ,[ContractTypeID] = @ContractTypeID
						 WHERE CLINID = @CLINID 
					END
			END
		ELSE
			BEGIN

				SET @ErrorMessage =   'The CLIN with Displayed CLIN Number ' + @DisplayedCLINNumber + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN 
			END
END

IF @@ERROR = 0
	SELECT	@CLINID AS CLINID
GO