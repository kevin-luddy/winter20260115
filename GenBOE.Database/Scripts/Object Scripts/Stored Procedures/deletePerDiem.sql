IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deletePerDiem]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deletePerDiem];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deletePerDiem]
(
@PerDiemID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deletePerDiem]
**		Desc: Delete PerDiem 
**			
**		
**
**		Auth: Don Canuso
**		Date: 01/10/12
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		5/22/12		dcanuso				Columns removed during Locking redesign
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[PerDiem] WHERE PerDiemID = @PerDiemID ) = @UpdateDT
		BEGIN
		
			DELETE FROM dbo.PerDiem
			WHERE PerDiemID = @PerDiemID/*Column Removed during Locking Redesign AND LockedRate = 0*/
	
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Per Diem with ID ' + CAST(@PerDiemID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO