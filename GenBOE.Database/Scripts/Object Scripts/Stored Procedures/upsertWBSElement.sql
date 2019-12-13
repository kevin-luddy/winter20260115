IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWBSElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWBSElement];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertWBSElement]
(
@WBSID int,
@WBSNumber varchar(119),
@DisplayedWBSNumber varchar(50),
@WBSTitle varchar(255),
/*Comma separated list of CLIN IDs with a comma at the end*/
@CLINID varchar (1000),
@WorkspaceID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [upsertWBSElement]
**		Desc: Insert/Update data into Manage WBS
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
**		9/29/10		dcanuso				Manage WBS and Manage BOE are heavily tied.  
**										Changes to one creates changes in 
**										the other.  Stored procedure changes
**										required.  When you create a new WBS
**										CLIN combination, you automatically
**										create a BOE.
**		11/9/10		dcanuso				Removal of [WBS_CLIN_BOE_XREF] from database
**										Columns Added to BOE table
**		3/1/11		dcanuso				Task 2387:
**										make sure the step to automatically remove the
**										creation of the BOE is removed from the 
**										appropriate stored procedure (upsertwbselement)
**										make sure BOEs are not deleted 
**										Also WBS Number increased to allow for :
**										WBS # levels must be 5 characters or less.
**										WBS # can have a maximum of 10 levels.
**		3/7/11		dcanuso				[WBS_CLIN_BOE_XREF] is BACK!!!!
**										Some examples
**										Can have a BOE without an CLIN and WBS
**										Can have a CLIN and WBS without a BOE
**		3/16/11		dcanuso				Insert/Update/Delete into XREF are incorrect
**										Missing Parameter
**		4/7/11		dcanuso				A CLIN cannot be removed from the Manage WBS page 
**										if a BOE exists for it.  They will be able to remove
**										the CLIN from the BOE on the Manage BOEs page and 
**										that will remove the association on the Manage WBS page.
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		5/11/11		dcanuso				Added @DisplayedWBSNumber
**		10/26/11	dcanuso				Added @DTCElementID
**		11/11/11	dcanuso				Added DTCElementInUseFlag
**		12/1/11		dcanuso				Fixing Table and Column for Workspace DTC Element
**		8/6/12		dcanuso				Bug 10469 Rel 1.5 Workspace DTC Element In use Flag is 
**										not being set/unset in all cases
**										There seems to be 2 issues with the Workspace DTC element.
**										This goes along with developer bug 10093. 
**										First issue, the in use flag is not being set to 
**										true in all cases. 
**										If a wbs previously had no DTC element and one is added, 
**										that DTC element isn't being marked as in use. If a wbs
**										did have a DTC element and it's relinked to another 
**										DTC element, that new link is being correctly marked 
**										as in use. It seems to be only an issue when the WBS 
**										goes from no DTC to a DTC.
**										Second issue, if a WBS had a DTC element that was 
**										marked in use, and the WBS is saved with no DTC 
**										element, the previously linked DTC is not marked 
**										as not in use. It remains in use forever.
**		9/5/12		mbasquil			WI 10910 Added IsNull() checks to properly
**										handle null values	
**		4/25/2017	twilson3			BOEJ-2121 Project Map updates
**		12/7/2017	twilson3			BOEJ-2250 Remove DTC
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)


					
/*Set Up Variables*/
DECLARE @InsertedWBS AS Table (WBSID int)

DECLARE @NextCLIN int

DECLARE @CLIN TABLE
	(
		CLINID int,
		Processed bit DEFAULT (0)
	)


/*
Get the list of CLINs
*/
IF @CLINID IS NOT NULL 
	BEGIN
		WHILE @CLINID IS NOT NULL
			BEGIN
				INSERT INTO @CLIN (CLINID)
				VALUES
					   (
					   LEFT (@CLINID,(CHARINDEX(',',@CLINID)-1))
					   )

			   SELECT	@CLINID = RIGHT(@CLINID, (LEN (@CLINID) - CHARINDEX(',',@CLINID)))
					
				IF LEN(@CLINID) = 0 SET @CLINID = NULL
			END
	END


IF @WBSID < 0  /*Insert Record*/

	BEGIN
	
	SET @UpdateDT = GETDATE()
	
	INSERT INTO [dbo].[WorkBreakdownStructure]
           ([WBSNumber]
           ,[DisplayedWBSNumber]
           ,[WBSTitle]
           ,[WorkspaceID]
           ,[UpdateDT]
           )
     OUTPUT inserted.WBSID INTO @InsertedWBS
     VALUES
           (
           @WBSNumber,
           @DisplayedWBSNumber,
           @WBSTitle,
           @WorkspaceID,
           @UpdateDT
           )
           
      SELECT @WBSID = WBSID FROM @InsertedWBS
      
/*
	Now we have WBS and CLIN IDs.          
	You can have WBS and CLIN without a BOE
	You can also create a BOE with WBS and/or CLIN
	But creating a BOE is not in the scope of this SP now
*/	


	
	WHILE EXISTS(SELECT 1 FROM @CLIN WHERE Processed = 0)
		BEGIN
			SELECT TOP 1 @NextCLIN = CLINID FROM @CLIN WHERE Processed = 0		
			
			IF NOT EXISTS (
								SELECT 1 FROM [dbo].[WBS_CLIN_BOE_XREF]
									WHERE	IsNull([WBSID], -9999) = IsNull(@WBSID, -9999) AND
											IsNull([CLINID], -9999) = IsNull(@CLINID, -9999)
								)								
			INSERT INTO [dbo].[WBS_CLIN_BOE_XREF]
		   (
			[WBSID]
		   ,[CLINID]
		   )
		   SELECT 
		   @WBSID,
		   @NextCLIN

		   UPDATE @CLIN SET Processed = 1 WHERE CLINID = @NextCLIN
		   
		END

           
END

ELSE /*Update*/
	BEGIN
		IF (SELECT UpdateDT 
				FROM [dbo].[WorkBreakdownStructure] 
				WHERE	WBSID = @WBSID
		) = @UpdateDT
			/*Update Dates Match*/
			BEGIN
			
				SET @UpdateDT = GETDATE()
				
				UPDATE [dbo].[WorkBreakdownStructure]
					SET 
					[WBSNumber] = @WBSNumber,
					[DisplayedWBSNumber] = @DisplayedWBSNumber,
					[WBSTitle] = @WBSTitle,
					[UpdateDT] = @UpdateDT
				WHERE WBSID = @WBSID
					
	/*
		Now we have WBS and CLIN IDs.          
		You can have WBS and CLIN without a BOE
		You can also create a BOE with WBS and/or CLIN
		But creating a BOE is not in the scope of this SP now
		
		A CLIN cannot be removed from the Manage WBS page if a BOE exists for it.  
		They will be able to remove the CLIN from the BOE on the Manage BOEs page and 
		that will remove the association on the Manage WBS page.
	*/	



			/*
				Delete:
				If you are removing a WBS, then all WBS and CLIN combinations for that WBS will be deleted.  
				If you are removing one CLIN from the WBS, we need to update the WBS/CLIN Combinations
				*/
				
			
				DECLARE @NewXREF TABLE
					(
						WBSID int,
						CLINID int,
						Processed bit DEFAULT 0
					)


					
				IF EXISTS (SELECT 1 FROM @CLIN WHERE Processed = 0)
				BEGIN
					WHILE EXISTS(SELECT 1 FROM @CLIN WHERE Processed = 0)
						BEGIN

							SELECT TOP 1 @NextCLIN = CLINID FROM @CLIN WHERE Processed = 0		
										
											
							INSERT INTO @NewXREF
						   (
							[WBSID]
						   ,[CLINID]
						   )
						   SELECT 
						   @WBSID,
						   @NextCLIN

						   UPDATE @CLIN SET Processed = 1 WHERE CLINID = @NextCLIN
						   
						END



				/*
					DELETE
					If there are WBS/CLIN combinations in Current table
					that are not in the new list
					they need to be deleted
				*/
					DELETE FROM WBS_CLIN_BOE_XREF
					FROM WBS_CLIN_BOE_XREF X
						LEFT OUTER JOIN @NewXREF TX ON 
							X.WBSID = TX.WBSID AND
							X.CLINID = TX.CLINID
					WHERE
						X.WBSID IS NOT NULL AND 
						TX.WBSID IS NULL AND
						X.CLINID IS NOT NULL AND
						TX.CLINID IS NULL AND
						X.WBSID = @WBSID AND
						X.BOEID IS NULL /*Can only delete records if they are not associated
											with a BOE with this SP.  Otherwise,
											you would have to use the Manage BOE page*/
						
	

				/*
					UPDATE	
					There are no updates.
					Either an Insert or a Delete
				*/
				
				/*
					INSERT
					Insert new CLIN/WBS Combinations
				*/
				IF NOT EXISTS (
								SELECT 1 FROM [dbo].[WBS_CLIN_BOE_XREF]
									WHERE	IsNull([WBSID], -9999) = IsNull(@WBSID, -9999) AND
											IsNull([CLINID], -9999) = IsNull(@CLINID, -9999) 
								)				
				INSERT INTO [dbo].[WBS_CLIN_BOE_XREF]
			   ([WBSID]
			   ,[CLINID]
				)
			   SELECT
					TX.WBSID, TX.CLINID
				FROM @NewXREF TX
					LEFT OUTER JOIN dbo.WBS_CLIN_BOE_XREF X ON 
						X.WBSID = TX.WBSID AND
						X.CLINID = TX.CLINID
				WHERE
					X.WBSID IS NULL AND 
					TX.WBSID IS NOT NULL AND
					X.CLINID IS NULL AND
					TX.CLINID IS NOT NULL
			END					

			ELSE /*There are No CLINS in @CLINID Parameter - so delete them all*/
				BEGIN
				
					DELETE FROM dbo.WBS_CLIN_BOE_XREF 
					WHERE 
						WBSID = @WBSID AND 
						BOEID IS NULL /*Can only delete records if they are not associated
											with a BOE with this SP.  Otherwise,
											you would have to use the Manage BOE page*/
						
				END
			END

ELSE
			BEGIN

					SET @ErrorMessage =   'The WBS with Displayed WBS Number ' + @DisplayedWBSNumber + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
					RETURN
				
			END
END

IF @@ERROR = 0
	SELECT	@WBSID AS WBSID
GO