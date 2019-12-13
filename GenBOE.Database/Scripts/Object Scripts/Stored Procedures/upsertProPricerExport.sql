IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProPricerExport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProPricerExport];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertProPricerExport]
(
@ProPricerExportID int,
@ProPricerExportName VARCHAR(100),
@WorkspaceID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [upsertProPricerExport]
**		Desc: Insert/Update a ProPricer Export
**			
**		
**
**		Auth: Don Canuso
**		Date: 4/27/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		7/12/19		twilson3			BOEJ-4037	System Pro Pricer Export 
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)

IF @ProPricerExportID  < 0  /*Insert Record*/
	BEGIN

	SET @UpdateDT = GETDATE()
	
	INSERT INTO [dbo].[ProPricerExport]
           ([ProPricerExportName]
           ,[WorkspaceID]
           ,[UpdateDT])
     OUTPUT inserted.ProPricerExportID INTO @Inserted
     VALUES
           (@ProPricerExportName
           ,@WorkspaceID
           ,@UpdateDT)


           
	SELECT @ProPricerExportID = ID FROM @Inserted
	

END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM dbo.ProPricerExport WHERE ProPricerExportID = @ProPricerExportID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			UPDATE [dbo].[ProPricerExport]
			   SET [ProPricerExportName] = @ProPricerExportName
				  ,[WorkspaceID] = @WorkspaceID
				  ,[UpdateDT] = @UpdateDT
			 WHERE ProPricerExportID = @ProPricerExportID
			
		END
		
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The Pro Pricer Export with Name ' + @ProPricerExportName +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @ProPricerExportID AS ProPricerExportID
GO