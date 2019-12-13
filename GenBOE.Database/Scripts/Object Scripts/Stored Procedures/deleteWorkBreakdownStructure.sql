IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkBreakdownStructure]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkBreakdownStructure];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteWorkBreakdownStructure]
(
@WBSID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkBreakdownStructure]
**		Desc: Delete WorkBreakdownStructure and all sub-elements
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/19/10		dcanuso				RETURN was not working properly - corrected
**		9/13/10		dcanuso				Soft deletes removed
**		10/6/10		DCANUSO				BOE PROCESSING CHANGED
***		11/9/10		dcanuso				Removal of [WBS_CLIN_BOE_XREF] from database
**										Columns Added to BOE table
**		12/30/10	dcanuso				Alias was incorrect - Fixed
**		01/05/11	dcanuso				WI 1299 
**										Adding Deletion for:
**										dbo.BOEApprovalHistory	
**										dbo.BOEApproval
**										dbo.BOECommentHistory
**										dbo.BOEComment
**										dbo.BOEStateHistory
**		3/2/11		dcanuso				Changes to the way a WBS is deleted:
**										Previously, when deleting a WBS in the database, 
**										all BOE information associated with the WBS was 
**										also deleted.  
**										In the new design, users will not be able to 
**										delete a WBS if there is a BOE associated with it. 
**										If BOEs are associated with it and they want to 
**										delete the WBS, they will need to delete the BOEs 
**										first or change the WBS for the BOEs to another WBS.
**										Then they will see the delete button enabled for 
**										the WBS on the Manage WBS page.
**										If there is a BOE associated with the WBS, 
**										the text "In use" will be displayed in place 
**										of the WBS (US 849). Also, WBS elements that have 
**										BOEs written for WBS elements under it also cannot 
**										be deleted (US 1919).
**		3/7/11		dcanuso				[WBS_CLIN_BOE_XREF] is BACK
**										BOEs will have to be deleted before a user can
**										delete the WBS and that is done via UI
**										Delete any reference in XREF table before deleting WBS
**										A WBS (CLIN) can be deleted if it has only a CLIN (WBS) associated with it
**										A WBS (CLIN) can not be deleted if it has a BOE (BOE) associated with it
**		11/11/11	dcanuso				Checking dbo.DTCElement.DTCElementInUseFlag
**		12/1/11		dcanuso				Updating to fix DTC to WS DTC
**		12/7/2017	twilson3			BOEJ-2250 Remove DTC
*****************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[WorkBreakdownStructure] WHERE WBSID = @WBSID) = @UpdateDT
		BEGIN
		
			DELETE FROM dbo.WBS_CLIN_BOE_XREF
			WHERE WBSID = @WBSID AND BOEID IS NULL

			DELETE FROM dbo.WorkBreakdownStructure
			FROM dbo.WorkBreakdownStructure 
			WHERE
				 WBSID = @WBSID
						
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The WBS with ID ' + CAST(@WBSID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
GO