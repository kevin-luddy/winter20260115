IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOE]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOE];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOE]
(
@WCBID int,
@BOEID int,
@WBSID int,
@CLINID int,
@BOEStateID int,
@BOEStartDate date,
@BOEEndDate date,
@AuthorID varchar (1000), 
@WorkspaceID int,
@UpdateDT datetime2,
@BOEDescription varchar (MAX),
@DataSource varchar(max),
@ETIUserID int,
@MetricDisclosureAcknowledge bit,
@NumAuthorReassigned int,
@IsMaterial bit,
@BOETitle varchar(100),
@SubcontractorAuthorID varchar (1000),
@CopyFromBOEID int = NULL,
@IsMultiClinWbs bit
)
AS
/******************************************************************************
**		 
**		Name:	[upsertBOE]
**		Desc:	Insert/Update BOE
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
**		8/19/10		dcanuso				RETURN was not working properly - corrected
**		9/13/10		dcanuso				Soft deletes removed
**		9/29/10		dcanuso				Developer requested Author be the ID
**										StateID is required.  Default is Initialization 
**										Since, it is possible to update the CLIN or 
**										the WBS, need to start passing in the XREF ID  
**										When you are on Manage BOE and a new WBS CLIN
**										combination is created, the developer will call
**										upsert WBS Element first to insert WBS/CLIN/BOE
**										Then call theis upsertBOE stored procedure
**										When that happens, both the @WCBID and @BOEID will be <0
**		11/4/10		dcanuso				Need to add ETIUserID to log changes in BOE State
**										Need to process BOE State Changes
**		11/9/10		dcanuso				Removal of [WBS_CLIN_BOE_XREF] from database
**										Columns Added to BOE table
**		11/11/10	dcanuso				BOE Description being added to the stored procedure		
**		11/16/10	dcanuso				BOE Logging being performed by DTO - Removed from SP (WI1018)
**		12/8/10		dcanuso				BOE Approval gets synched with Approvers
**		12/14/10	dcanuso				BOE State History is updated with State Changes as well
**										logging the person who made that change
**		3/1/11		dcanuso				No Changes in SP Needed however
**										NOTE:
**										BOE can now be created without CLINS
**										BOE Can be created with WBS, CLIN or WBS AND CLIN
**		3/7/11		dcanuso				WBS_CLIN_BOE_XREF is back
**										Removal of WBS and CLIN from BOE
**		3/14/11		dcanuso				WI2634  xref table must have either wbs OR clin for insert
**		3/29/11		dcanuso				@DataSource moved from Task Element to BOE
**		4/22/1		dcanuso				@MetricDisclosureAcknowledge Added
**		4/28/11		dcanuso				Adding None rather than NULL for LU - WI 3051
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		5/12/11		dcanuso				New parameter:@NumAuthorReassigned
**		6/22/11		dcanuso				New Parameter: @IsMaterial
**		7/25/11		dcanuso				Removing Approver as per WI request (4293)
**										Removed: @Approver varchar(250)
**										Changed Author to AuthorID
**										Changed Logic for handling Author
**		8/25/11		dcanuso				AutoCalculateDTS added
**		9/16/11		dcanuso				WI 5160 Logging Author and Approver changes
**		9/5/12		mbasquil			WI 10910 Added IsNull() checks to properly
**										handle null values		
**		3/5/13		dcanuso				WI 16730 moq text to varchar (max)
**		4/5/13		dcanuso				WI 17282 BOE Title added
**		5/28/13		dcanuso				Task #18628: DB Changes to support Multiple BOE Authors
**										Example: AuthorID = '9,10,11,' 
**										Multiple Subcontractors as well
**		7/16/14		dcanuso				Workspace and BOE Copy Metric
**		8/31/15		palider				Per BOE-J 206, increasing the author and subcontractor fields to allow for a larger number of NTIDs
**		10/14/15	mbasquil			BOEJ-342 Resource level WBS/CLIN
**      12/16/15    perry				Summary Boe changes
**      2/15/16		twilson3			Summary Boe changes
**		9/7/2016	twilson3			BOEJ-1333 Saving INL Form selections
**      1/17/17		twilson3			BOEJ-1604 Cleanup DB Project
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
**		4/25/2017	twilson3			BOEJ-2121 Project Map updates
**		7/6/2017	twilson3			BOEJ-2372 Recurring/Non-Recurring ClassOfCost
**		7/21/2017	momeara				BOEJ-2412 - RMS Feedback (round 3?)
**		10/2/2017	twilson3			BOEJ-2520 Cleanup DB, remove old ProjectMap columns
**		12/7/17		twilson3			BOEJ-1994 - Remove Summary BOE
**		08/12/25	ranzalon			PROPH-1877 - Clear Task Authors when unassigned from BOE
*******************************************************************************/

SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF @CLINID IS NULL AND @WBSID IS NULL
	BEGIN
		SET @ErrorMessage =   'A CLIN or WBS is required to create or update a BOE.'
		RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
		RETURN
	END

/*
Process Authors
*/
IF RIGHT(@AuthorID, 1) <> ','
	SET @AuthorID = @AuthorID + ','

DECLARE @Author TABLE (AuthorID INT, Processed bit default (0))
WHILE (SELECT CHARINDEX (',', @AuthorID) ) > 1
	BEGIN	
		INSERT INTO @Author (AuthorID)
		SELECT LEFT (@AuthorID, CHARINDEX (',', @AuthorID) -1)
		SET @AuthorID = RIGHT (@AuthorID, LEN (@AuthorID) - CHARINDEX (',', @AuthorID) )
	END


/*
Process Subcontractor Authors
*/
IF RIGHT(@SubcontractorAuthorID, 1) <> ','
	SET @SubcontractorAuthorID = @SubcontractorAuthorID + ','

DECLARE @SubcontractorAuthor TABLE (AuthorID INT, Processed bit default (0))
WHILE (SELECT CHARINDEX (',', @SubcontractorAuthorID) ) > 1
	BEGIN	
		INSERT INTO @SubcontractorAuthor (AuthorID)
		SELECT LEFT (@SubcontractorAuthorID, CHARINDEX (',', @SubcontractorAuthorID) -1)
		SET @SubcontractorAuthorID = RIGHT (@SubcontractorAuthorID, LEN (@SubcontractorAuthorID) - CHARINDEX (',', @SubcontractorAuthorID) )
	END

DECLARE @InsertedBOE AS Table (BOEID int)
DECLARE @CurrentBOEStateID int
SELECT @CurrentBOEStateID = BOEStateID FROM dbo.BOE WHERE BOEID = @BOEID

IF @BOEID  < 0  /*Insert Record*/
	BEGIN	
		SET @UpdateDT = GETDATE()	
		INSERT INTO [dbo].[BOE]
			   (
				[BOEStateID]
			   ,[BOEStartDate]
			   ,[BOEEndDate]
			   ,[UpdateDT]
			   ,[WorkspaceID]
			   ,[BOEDescription]
			   ,[DataSource]
			   ,[MetricDisclosureAcknowledge]
			   ,[NumAuthorReassigned]
			   ,[IsMaterial]
			   ,[BOETitle]
			   ,[IsMultiClinWbs]
			   )
		OUTPUT inserted.BOEID INTO @InsertedBOE            
		VALUES
			   (
			   @BOEStateID,
			   @BOEStartDate,
			   @BOEEndDate,
			   @UpdateDT,
			   @WorkspaceID,
			   @BOEDescription,
			   @DataSource,
			   IsNull(@MetricDisclosureAcknowledge,0),
			   @NumAuthorReassigned,
			   @IsMaterial,
			   IsNull(@BOETitle, ''),
			   @IsMultiClinWbs
			   )
		SELECT @BOEID = BOEID FROM @InsertedBOE
	
		/*
			BOE Logging when BOE State changes or when BOE is created
		*/
		IF @CurrentBOEStateID IS NULL /*This is a new BOE*/ 
			BEGIN
				/* Insert BOE Created State*/		
				INSERT INTO [dbo].[BOEStateHistory]
				([BOEID]
				,[FieldID]
				,[CurrentBOEStateID]
				,[UpdatedBOEStateID]
				,[ChangedByETIUserID]
				,[UpdateDT])
					VALUES
						(@BOEID
						,1 /*BOE Created*/
						,0 /* None*/
						,0 /* None*/
						,@ETIUserID
						,@UpdateDT)
				   
				/*Insert the New State*/				   
			   INSERT INTO [dbo].[BOEStateHistory]
			   ([BOEID]
			   ,[FieldID]
			   ,[CurrentBOEStateID]
			   ,[UpdatedBOEStateID]
			   ,[ChangedByETIUserID]
			   ,[UpdateDT])
				 VALUES
					   (@BOEID
					   ,2 /*BOE Status*/
					   ,0 /*None*/
					   ,@BOEStateID
					   ,@ETIUserID
					   ,DATEADD(SS, 1, @UpdateDT))				   
			END				   
	
		/*
			Only one Author is assigned - no need to look through list
			And since we are in creating a BOE, an author should not exist
			Now with WI 18628, multiple BOE Authors are allowed
		*/

		IF EXISTS (SELECT * FROM @Author)
			BEGIN
				INSERT INTO [dbo].[BOEUserRole]
				   ([ETIUserID]
				   ,[RoleID]
				   ,[BOEID]
				   ,[UpdateDT]
				   )
				 SELECT
				   AuthorID,
				   1, /*Author Role is 1*/
				   @BOEID,
				   @UpdateDT
				   FROM @Author

				INSERT INTO [dbo].[BOEUserRoleHistory]
						   ([UpdateDT]
						   ,[CurrentETIUserID]
						   ,[UpdatedETIUserID]
						   ,[RoleID]
						   ,[BOEID]
						   ,[FieldID]
						   ,[ChangedByETIUserID])
				 SELECT
					@UpdateDT,
					NULL,
					AuthorID,
					1, /*Author Role is 1*/
					@BOEID,
					7,/*Author*/
					@ETIUserID
				   FROM @Author
			END

		/*
			WI 18628, multiple Subcontractor Authors are allowed
		*/
		IF EXISTS (SELECT * FROM @SubcontractorAuthor)
			BEGIN 
				INSERT INTO [dbo].[BOEUserRole]
				   ([ETIUserID]
				   ,[RoleID]
				   ,[BOEID]
				   ,[UpdateDT]
				   )
				 SELECT
				   AuthorID,
				   9,	/*Subcontractor Author*/
				   @BOEID,
				   @UpdateDT
				   FROM @SubcontractorAuthor

				INSERT INTO [dbo].[BOEUserRoleHistory]
						   ([UpdateDT]
						   ,[CurrentETIUserID]
						   ,[UpdatedETIUserID]
						   ,[RoleID]
						   ,[BOEID]
						   ,[FieldID]
						   ,[ChangedByETIUserID])
				 SELECT
					@UpdateDT,
					NULL,
					AuthorID,
					9,	/*Subcontractor Author*/
					@BOEID,
					7,/*Author*/
					@ETIUserID
				   FROM @SubcontractorAuthor
			END	

		/*
			Finally, need create link for the WBSID, CLINID, and BOEID
			since this is an add
		*/
		IF @WCBID < 0
			BEGIN
				IF @CLINID IS NOT NULL OR @WBSID IS NOT NULL
					BEGIN
						IF NOT EXISTS (
										SELECT 1 FROM [dbo].[WBS_CLIN_BOE_XREF]
											WHERE	IsNull([WBSID], -9999) = IsNull(@WBSID, -9999) AND
													IsNull([CLINID], -9999) = IsNull(@CLINID, -9999) AND
													IsNull([BOEID], -9999) = IsNull(@BOEID, -9999)
										)				
						INSERT INTO [dbo].[WBS_CLIN_BOE_XREF] ([WBSID],[CLINID],[BOEID])
						SELECT @WBSID, @CLINID, @BOEID
					END
			END
		ELSE
			BEGIN
				IF NOT EXISTS (
									SELECT 1 FROM [dbo].[WBS_CLIN_BOE_XREF]
										WHERE	IsNull([WBSID], -9999) = IsNUll(@WBSID, -9999) AND
												IsNull([CLINID], -9999) = IsNull(@CLINID, -9999) AND
												IsNull([BOEID], -9999) = IsNull(@BOEID, -9999)
									)				
				UPDATE [dbo].[WBS_CLIN_BOE_XREF]
				SET 
					 [WBSID] = @WBSID
					,[CLINID] = @CLINID
					,[BOEID] = @BOEID
				WHERE 
					WCBID = @WCBID
			END
	END	
ELSE
	/*Update*/
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[BOE] WHERE BOEID = @BOEID) = @UpdateDT
			BEGIN			
				SET @UpdateDT = GETDATE()				
				/*
				Check for BOE Status Change
				If Changed, log
				*/
				IF @CurrentBOEStateID  IS NULL
					BEGIN 
					/*Insert the New State*/				   
					INSERT INTO [dbo].[BOEStateHistory]
				   ([BOEID]
				   ,[FieldID]
				   ,[CurrentBOEStateID]
				   ,[UpdatedBOEStateID]
				   ,[ChangedByETIUserID]
				   ,[UpdateDT])
					 VALUES
						   (@BOEID
						   ,2 /*BOE Status*/
						   ,@CurrentBOEStateID
						   ,@BOEStateID
						   ,@ETIUserID
						   ,@UpdateDT)
					
					END
				
				IF @CurrentBOEStateID  <> @BOEStateID 						
					BEGIN 
						/*Insert the New State*/				   
						INSERT INTO [dbo].[BOEStateHistory]
					   ([BOEID]
					   ,[FieldID]
					   ,[CurrentBOEStateID]
					   ,[UpdatedBOEStateID]
					   ,[ChangedByETIUserID]
					   ,[UpdateDT])
						 VALUES
							   (@BOEID
							   ,2 /*BOE Status*/
							   ,@CurrentBOEStateID
							   ,@BOEStateID
							   ,@ETIUserID
							   ,DATEADD(SS, 1, @UpdateDT))
					END
				
				UPDATE [dbo].[BOE]
				   SET 
					  [BOEStateID] = @BOEStateID,
					  [BOEStartDate] = @BOEStartDate,
					  [BOEEndDate] = @BOEEndDate,
					  [UpdateDT] = @UpdateDT,
					  [BOEDescription] = @BOEDescription,
					  [DataSource] = @DataSource,
					  [MetricDisclosureAcknowledge] = IsNull(@MetricDisclosureAcknowledge,0),
					  [NumAuthorReassigned] = @NumAuthorReassigned,
					  [IsMaterial] = @IsMaterial,
					  [BOETitle] = IsNull(@BOETitle, ''),
					  [IsMultiClinWbs] = @IsMultiClinWbs

				WHERE
					[BOEID] = @BOEID

				/*
					Process the Authors
					Only one Author is assigned
					When Approver logic was removed, logic for handling Author changed
					Now with WI 18628, multiple BOE Authors are allowed
				*/ 
				DECLARE @CurrentAuthorID int

				IF NOT EXISTS (SELECT * FROM @Author)
					/*Whole List is getting deleted*/
					BEGIN 
						INSERT INTO [dbo].[BOEUserRoleHistory]
								   ([UpdateDT]
								   ,[CurrentETIUserID]
								   ,[UpdatedETIUserID]
								   ,[RoleID]
								   ,[BOEID]
								   ,[FieldID]
								   ,[ChangedByETIUserID])
						SELECT 
								   @UpdateDT
								   ,ETIUserID
								   ,NULL
								   ,1 /*Author Role is 1*/
								   ,@BOEID
								   ,7/*Author*/
								   ,@ETIUserID
						FROM [dbo].[BOEUserRole] WHERE [RoleID] = 1/*Author*/ AND BOEID = @BOEID

						DELETE FROM [dbo].[BOEUserRole] WHERE [RoleID] = 1/*Author*/ AND BOEID = @BOEID		
					END

	IF EXISTS (SELECT * FROM @Author)
	BEGIN 
	/*Handle Users who are no longer in new list*/
		IF EXISTS 
			(
				SELECT [ETIUserID] 
				FROM [dbo].[BOEUserRole] 
				WHERE [RoleID] = 1/*Author*/ AND 
				BOEID = @BOEID AND 
				ETIUserID NOT IN (SELECT AuthorID FROM @Author)
			)
		BEGIN
 
			INSERT INTO [dbo].[BOEUserRoleHistory]
				([UpdateDT]
				,[CurrentETIUserID]
				,[UpdatedETIUserID]
				,[RoleID]
				,[BOEID]
				,[FieldID]
				,[ChangedByETIUserID])
			SELECT
				@UpdateDT
				,ETIUserID
				,NULL
				,1 /*Author Role is 1*/
				,@BOEID
				,7/*Author*/
				,@ETIUserID
			FROM [dbo].[BOEUserRole] 
			WHERE [RoleID] = 1/*Author*/ AND 
			BOEID = @BOEID AND 
			ETIUserID NOT IN (SELECT AuthorID FROM @Author)

			DELETE FROM [dbo].[BOEUserRole] 
			WHERE [RoleID] = 1/*Author*/ AND 
			BOEID = @BOEID AND 
			ETIUserID NOT IN (SELECT AuthorID FROM @Author)
		END

	WHILE EXISTS (SELECT 1 FROM @Author WHERE Processed = 0)
	BEGIN 
	SELECT TOP 1 @CurrentAuthorID = AuthorID FROM @Author WHERE Processed = 0

	IF NOT EXISTS (SELECT [ETIUserID] FROM [dbo].[BOEUserRole] WHERE [RoleID] = 1/*Author*/ AND BOEID = @BOEID AND ETIUserID = @CurrentAuthorID)
		BEGIN 
			/*New*/
			INSERT INTO [dbo].[BOEUserRole]
			   ([ETIUserID]
			   ,[RoleID]
			   ,[BOEID]
			   ,[UpdateDT]
			   )
			 SELECT
			   AuthorID,
			   1, /*Author Role is 1*/
			   @BOEID,
			   @UpdateDT
			   FROM @Author
			   WHERE AuthorID = @CurrentAuthorID


			INSERT INTO [dbo].[BOEUserRoleHistory]
					   ([UpdateDT]
					   ,[CurrentETIUserID]
					   ,[UpdatedETIUserID]
					   ,[RoleID]
					   ,[BOEID]
					   ,[FieldID]
					   ,[ChangedByETIUserID])
			 SELECT
				@UpdateDT,
				NULL,
				AuthorID,
				1, /*Author Role is 1*/
				@BOEID,
				7,/*Author*/
				@ETIUserID
			   FROM @Author
			   WHERE AuthorID = @CurrentAuthorID			   
		END	

UPDATE @Author SET Processed = 1 WHERE AuthorID = @CurrentAuthorID AND Processed = 0
END


END

/*
WI 18628, multiple Subcontractor Authors are allowed
*/ 
DECLARE @CurrentSubcontractorAuthorID int

IF NOT EXISTS (SELECT * FROM @SubcontractorAuthor)
/*Whole List is getting deleted*/
BEGIN 
	
	INSERT INTO [dbo].[BOEUserRoleHistory]
			   ([UpdateDT]
			   ,[CurrentETIUserID]
			   ,[UpdatedETIUserID]
			   ,[RoleID]
			   ,[BOEID]
			   ,[FieldID]
			   ,[ChangedByETIUserID])
	SELECT 
			   @UpdateDT
			   ,ETIUserID
			   ,NULL
			   ,9	/*Subcontractor Author*/
			   ,@BOEID
			   ,7/*Author*/
			   ,@ETIUserID
	FROM [dbo].[BOEUserRole] WHERE [RoleID] = 9	/*Subcontractor Author*/ AND BOEID = @BOEID

	DELETE FROM [dbo].[BOEUserRole] WHERE [RoleID] = 9	/*Subcontractor Author*/ AND BOEID = @BOEID
	
END

IF EXISTS (SELECT * FROM @SubcontractorAuthor)
BEGIN 
/*Handle Users who are no longer in new list*/
	IF EXISTS 
		(
			SELECT [ETIUserID] 
			FROM [dbo].[BOEUserRole] 
			WHERE [RoleID] = 9/*Subcontractor Author*/ AND 
			BOEID = @BOEID AND 
			ETIUserID NOT IN (SELECT AuthorID FROM @SubcontractorAuthor)
		)
	BEGIN
 
		INSERT INTO [dbo].[BOEUserRoleHistory]
			([UpdateDT]
			,[CurrentETIUserID]
			,[UpdatedETIUserID]
			,[RoleID]
			,[BOEID]
			,[FieldID]
			,[ChangedByETIUserID])
		SELECT
			@UpdateDT
			,ETIUserID
			,NULL
			,9 /*Subcontractor Author Role*/
			,@BOEID
			,7/*Author*/
			,@ETIUserID
		FROM [dbo].[BOEUserRole] 
		WHERE [RoleID] = 9/*SubcontractorAuthor*/ AND 
		BOEID = @BOEID AND 
		ETIUserID NOT IN (SELECT AuthorID FROM @SubcontractorAuthor)

		DELETE FROM [dbo].[BOEUserRole] 
		WHERE [RoleID] = 9/*SubcontractorAuthor*/ AND 
		BOEID = @BOEID AND 
		ETIUserID NOT IN (SELECT AuthorID FROM @SubcontractorAuthor)
	END

	WHILE EXISTS (SELECT 1 FROM @SubcontractorAuthor WHERE Processed = 0)
	BEGIN 
	SELECT TOP 1 @CurrentSubcontractorAuthorID = AuthorID FROM @SubcontractorAuthor WHERE Processed = 0
	
	IF NOT EXISTS (SELECT [ETIUserID] FROM [dbo].[BOEUserRole] WHERE [RoleID] = 9/*SubcontractorAuthor*/ AND BOEID = @BOEID AND ETIUserID = @CurrentSubcontractorAuthorID)
		BEGIN 
			/*New*/
			INSERT INTO [dbo].[BOEUserRole]
			   ([ETIUserID]
			   ,[RoleID]
			   ,[BOEID]
			   ,[UpdateDT]
			   )
			 SELECT
			   AuthorID,
			   9, /*SubcontractorAuthor Role*/
			   @BOEID,
			   @UpdateDT
			   FROM @SubcontractorAuthor
			   WHERE AuthorID = @CurrentSubcontractorAuthorID


			INSERT INTO [dbo].[BOEUserRoleHistory]
					   ([UpdateDT]
					   ,[CurrentETIUserID]
					   ,[UpdatedETIUserID]
					   ,[RoleID]
					   ,[BOEID]
					   ,[FieldID]
					   ,[ChangedByETIUserID])
			 SELECT
				@UpdateDT,
				NULL,
				AuthorID,
				9, /*SubcontractorAuthor Role*/
				@BOEID,
				7,/*Author*/
				@ETIUserID
			   FROM @SubcontractorAuthor
			   WHERE AuthorID = @CurrentSubcontractorAuthorID
		END	

	UPDATE @SubcontractorAuthor SET Processed = 1 WHERE AuthorID = @CurrentSubcontractorAuthorID AND Processed = 0
	END
END

/* Clear Task Author from all tasks in BOE where user was unassigned from Author/Subcontractor Author */
UPDATE dbo.[BOETaskElement]
SET AuthorUserId = NULL, UpdateDT = @UpdateDT
WHERE BOEID = @BOEID AND AuthorUserId IS NOT NULL 
	AND AuthorUserId NOT IN (SELECT AuthorID FROM @Author) 
	AND AuthorUserId NOT IN (SELECT AuthorID FROM @SubcontractorAuthor)

/*
	BOEJ-1333, multiple Custom form selections are allowed
	Delete the current list and insert the new ones	(standard Kill-Fill)		
*/

	/*
	Finally, need create link for the WBSID, CLINID, and BOEID
	since this is an add
	*/
	IF @WCBID < 0
		BEGIN 
		IF @CLINID IS NOT NULL OR @WBSID IS NOT NULL
			BEGIN
				IF NOT EXISTS (
								SELECT 1 FROM [dbo].[WBS_CLIN_BOE_XREF]
									WHERE	IsNull([WBSID], -9999) = IsNull(@WBSID, -9999) AND
											IsNull([CLINID], -9999) = IsNull(@CLINID, -9999) AND
											IsNull([BOEID], -9999) = IsNull(@BOEID, -9999)
								)				
				INSERT INTO [dbo].[WBS_CLIN_BOE_XREF] ([WBSID],[CLINID],[BOEID])
				SELECT @WBSID, @CLINID, @BOEID
			END
		END
	ELSE
		BEGIN 
			IF NOT EXISTS (
								SELECT 1 FROM [dbo].[WBS_CLIN_BOE_XREF]
									WHERE	IsNull([WBSID], -9999) = IsNull(@WBSID, -9999) AND
											IsNull([CLINID], -9999) = IsNull(@CLINID, -9999) AND
											IsNull([BOEID], -9999) = IsNull(@BOEID, -9999)
								)				
			UPDATE [dbo].[WBS_CLIN_BOE_XREF]
			SET 
				 [WBSID] = @WBSID
				,[CLINID] = @CLINID
				,[BOEID] = @BOEID
			WHERE 
				WCBID = @WCBID
		END
END		
ELSE
			BEGIN
				
				SET @ErrorMessage =   'The BOE with ID ' + CAST(@BOEID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
           			
END

/*copy boe metric*/
IF @CopyFromBOEID IS NOT NULL
BEGIN

DECLARE @CreateDate datetime2(7) = GetDate()

INSERT INTO [dbo].[BOECopySource]
           ([BOEID]
           ,[UpdateDT]
           ,[BOEStateID]
           ,[BOEStartDate]
           ,[BOEEndDate]
           ,[BOEDescription]
           ,[DataSource]
           ,[WorkspaceID]
           ,[MetricDisclosureAcknowledge]
           ,[NumAuthorReassigned]
           ,[IsMaterial]
           ,[BOETitle]
		   ,[WorkspaceName]
		   ,[WorkspaceShortName]
		   ,[CreateDate]
		   )
SELECT B.[BOEID]
      ,B.[UpdateDT]
      ,B.[BOEStateID]
      ,B.[BOEStartDate]
      ,B.[BOEEndDate]
      ,B.[BOEDescription]
      ,B.[DataSource]
      ,B.[WorkspaceID]
      ,B.[MetricDisclosureAcknowledge]
      ,B.[NumAuthorReassigned]
      ,B.[IsMaterial]
      ,B.[BOETitle]
	  ,W.[WorkspaceName]
	  ,W.[WorkspaceShortName]
	  ,@CreateDate
  FROM [dbo].[BOE] B
	INNER JOIN [dbo].[Workspace] W ON B.WorkspaceID = W.WorkspaceID
  WHERE B.BOEID = @CopyFromBOEID

INSERT INTO [dbo].[BOECopyTarget]
           ([BOEID]
           ,[UpdateDT]
           ,[BOEStateID]
           ,[BOEStartDate]
           ,[BOEEndDate]
           ,[BOEDescription]
           ,[DataSource]
           ,[WorkspaceID]
           ,[MetricDisclosureAcknowledge]
           ,[NumAuthorReassigned]
           ,[IsMaterial]
           ,[BOETitle]
		   ,[WorkspaceName]
		   ,[WorkspaceShortName]
		   ,[CreateDate]
		   )
SELECT B.[BOEID]
      ,B.[UpdateDT]
      ,B.[BOEStateID]
      ,B.[BOEStartDate]
      ,B.[BOEEndDate]
      ,B.[BOEDescription]
      ,B.[DataSource]
      ,B.[WorkspaceID]
      ,B.[MetricDisclosureAcknowledge]
      ,B.[NumAuthorReassigned]
      ,B.[IsMaterial]
      ,B.[BOETitle]
	  ,W.[WorkspaceName]
	  ,W.[WorkspaceShortName]
	  ,@CreateDate
  FROM [dbo].[BOE] B
	INNER JOIN [dbo].[Workspace] W ON B.WorkspaceID = W.WorkspaceID
 WHERE B.BOEID = @BOEID


INSERT INTO [dbo].[BOECopyMetric]
           ([SourceBOEID]
           ,[TargetBOEID]
           ,[CreateDate])
     VALUES
           (@CopyFromBOEID
           ,@BOEID
           ,@CreateDate)



END

IF @@ERROR = 0
	SELECT	@BOEID AS BOEID
GO