IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteCLIN]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteCLIN];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteCLIN]
(
@CLINID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteCLIN]
**		Desc: Delete CLIN. If a CLIN is deleted, then it also needs to be deleted 
**             in WBS_CLIN_BOE_XREF
**			
**		Deleting CLIN: Deletes CLIN, deletes related BOEs, Removed CLIN from WBS (XREF)
**
**		Auth: Kristine Goodwin and Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/19/10		dcanuso				RETURN was not working properly - corrected
**		9/13/10		dcanuso				Soft deletes removed
**		9/14/10		dcanuso				Kristine noticed that BOEs were not deleted.  
**										I had the temp table in the wrong place
**										Updated to fix
***		11/9/10		dcanuso				Removal of [WBS_CLIN_BOE_XREF] from database
**										Columns Added to BOE table
**		11/24/10	dcanuso				When a CLIN is deleted, all BOEs that are
**										using that CLIN are also deleted
**		3/7/11		dcanuso				[WBS_CLIN_BOE_XREF] is BACK
**										You can have just a CLIN with NO BOE and No WBS
**										BOEs will have to be deleted before a user can
**										delete the CLIN and that is done via UI
**										Delete any reference in XREF table before deleting WBS
**										A WBS (CLIN) can be deleted if it has only a CLIN (WBS) associated with it
**										A WBS (CLIN) can not be deleted if it has a BOE (BOE) associated with it
**		4/28/11		dcanuso				WI 3199 - Should be able to delete from Sum tables
******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[CLIN] WHERE CLINID = @CLINID) = @UpdateDT
		BEGIN
			
			DELETE FROM dbo.WBS_CLIN_BOE_XREF
			WHERE CLINID = @CLINID AND BOEID IS NULL
			
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF WHERE CLINID = @CLINID
			
			DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF WHERE CLINID = @CLINID

			DELETE FROM dbo.CLIN WHERE CLINID = @CLINID
				
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The CLIN with ID ' + CAST(@CLINID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END


GO