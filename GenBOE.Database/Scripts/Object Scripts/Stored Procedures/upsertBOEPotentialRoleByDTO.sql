IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOEPotentialRoleByDTO]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOEPotentialRoleByDTO];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOEPotentialRoleByDTO]
(
	@BOEPotentialRoleID int,
	@ETIUserID int,
	@WorkspaceID int,
	@RoleID int,
	@UserRemoved smallint,
	@UpdateDT datetime2(7)
)
AS
/******************************************************************************
**		 
**		Name: upsertBOEPotentialRoleByDTO
**		Desc: Insert/Updates a record into the BOE Potential Role Permission table for DTO
**			
**		
**
**		Auth: Don Canuso
**		Date: 3/3/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		9/5/12		mbasquil			WI 10910 Added IsNull() checks to properly
**										handle null values	
**		11/20/13	dcanuso				Permission Story - Remove ETI Group ID
**		12/4/13		DCANUSO				wi 24910
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @InsertedBOEPotentialRole AS Table (BOEPotentialRoleID int)
DECLARE	@ErrorMessage varchar (500)

IF @BOEPotentialRoleID < 0
BEGIN
/*
genBoeSpace_main
ipecoor2
*/
--SELECT * FROM dbo.BOEPotentialRole BR
--inner join rolelu r on br.roleid =r.RoleID
--inner join ETIuser u on br.ETIUserID = u.ETIUserID
-- WHERE br.RoleID <> 9 AND u.NTID='ipecoor2'

IF @RoleID <> 9
BEGIN
IF EXISTS (SELECT 1 FROM dbo.BOEPotentialRole WHERE RoleID = 9 AND ETIUserID = @ETIUserID AND @RoleID <> 9)
BEGIN

	SET @ErrorMessage =   'You can not add any other roles to a user that has the Subcontractor Author role '
	RAISERROR (
		@ErrorMessage, -- Message text.
        11, -- Severity,/*Severity Changed to 11*/
		1 -- State,
		)
	RETURN
END
END

IF @RoleID = 9
BEGIN
IF EXISTS (SELECT 1 FROM dbo.BOEPotentialRole WHERE RoleID <> 9 AND ETIUserID = @ETIUserID AND @RoleID = 9)
BEGIN

	SET @ErrorMessage =   'You can not add Subcontractor Role if you have another role'
	RAISERROR (
		@ErrorMessage, -- Message text.
        11, -- Severity,/*Severity Changed to 11*/
		1 -- State,
		)
	RETURN
END
END




	IF EXISTS	(SELECT 1 FROM [dbo].[BOEPotentialRole]
						WHERE	ETIUserID = @ETIUserID AND
								/*IsNull(ETIGroupID, -9999) = IsNull(@ETIGroupID, -9999) AND*/
								IsNull(RoleID, -9999) = IsNull(@RoleID, -9999) AND
								WorkspaceID = @WorkspaceID
				)
			BEGIN
				SET @ErrorMessage =   'There is already a record for this BOE Potential Role'
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

		INSERT INTO [dbo].[BOEPotentialRole]
				   ([ETIUserID]
				   /*,[ETIGroupID]*/
				   ,[WorkspaceID]
				   ,[RoleID]
				   ,[UserRemoved]
				   ,[UpdateDT])
			 OUTPUT inserted.BOEPotentialRoleID INTO @InsertedBOEPotentialRole
			 VALUES
				   (
					@ETIUserID, 
/*					@ETIGroupID,*/
					@WorkspaceID,
					@RoleID, 
					IsNull(@UserRemoved,0), 
					@UpdateDT 
					)

	           
		  SELECT @BOEPotentialRoleID = BOEPotentialRoleID FROM @InsertedBOEPotentialRole
							
    END
END    
ELSE
	BEGIN
	IF (SELECT UpdateDT FROM dbo.BOEPotentialRole WHERE BOEPotentialRoleID = @BOEPotentialRoleID) = @UpdateDT
		BEGIN 
			SET @UpdateDT = GETDATE()
			
			UPDATE [dbo].[BOEPotentialRole]
			   SET [UserRemoved] = @UserRemoved
				  ,[UpdateDT] = @UpdateDT
			 WHERE BOEPotentialRoleID = @BOEPotentialRoleID
			
		END
		
	ELSE
			BEGIN

				SET @ErrorMessage =   'The BOE Potential Role with ID ' + CAST(@BOEPotentialRoleID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
END

IF @@ERROR = 0
	SELECT @ETIUserID as ETIUserID
GO