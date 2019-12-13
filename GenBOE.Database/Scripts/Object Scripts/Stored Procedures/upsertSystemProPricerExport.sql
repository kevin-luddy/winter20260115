IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertSystemProPricerExport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertSystemProPricerExport];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertSystemProPricerExport]
(
@SystemProPricerExportID int,
@ProPricerExportName VARCHAR(100),
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [upsertSystemProPricerExport]
**		Desc: Insert/Update a ProPricer Export
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 7/15/2019
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		7/12/19		twilson3			BOEJ-4037	System Pro Pricer Export 
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)

IF @SystemProPricerExportID  < 0  /*Insert Record*/
	BEGIN

	SET @UpdateDT = GETDATE()
	
	INSERT INTO [dbo].[SystemProPricerExport]
           ([ProPricerExportName]
           ,[UpdateDT])
     OUTPUT inserted.SystemProPricerExportID INTO @Inserted
     VALUES
           (@ProPricerExportName
           ,@UpdateDT)


           
	SELECT @SystemProPricerExportID = ID FROM @Inserted
	

END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM dbo.SystemProPricerExport WHERE SystemProPricerExportID = @SystemProPricerExportID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			-- Delete the attached resources/tasks (they will be re-inserted after this update)
			DELETE FROM dbo.SystemProPricerFieldXREF
			WHERE 
				SystemProPricerExportID = @SystemProPricerExportID
				
			DELETE FROM dbo.SystemProPricerCustomFieldXREF
			WHERE 
				SystemProPricerExportID = @SystemProPricerExportID 
			
			UPDATE [dbo].[SystemProPricerExport]
			   SET [ProPricerExportName] = @ProPricerExportName
				  ,[UpdateDT] = @UpdateDT
			 WHERE SystemProPricerExportID = @SystemProPricerExportID
			
		END
		
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The System Pro Pricer Export with Name ' + @ProPricerExportName +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @SystemProPricerExportID AS SystemProPricerExportID
GO