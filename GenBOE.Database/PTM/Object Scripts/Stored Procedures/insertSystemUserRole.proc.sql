IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertSystemUserRole]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertSystemUserRole];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertSystemUserRole]
(
	@UserID int,
	@RoleID int,
	@LineOfBusinessID varchar(100)
)
AS
/******************************************************************************
**		 
**		Name: insertSystemUserRole
**		Desc: Inserts a record into the System User Role table
**			
**		
**
**		Auth: Don Canuso
**		Date: 4/23/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/20/14	dcanuso				New Proposal Set Up role - Renamed to 
**										reuse Product Line Viewer to Role XREF
**		1/12/2017	brunworg			BOEJ-1688 Update PTM SPs to not display technical details to the user
**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE @ErrorMessage varchar (500)

IF EXISTS	(SELECT 1 FROM [dbo].[SystemUserRole] WHERE	
					 UserID = @UserID AND
				 	 RoleID = @RoleID
			)
			BEGIN
				SET @ErrorMessage =   'This System User Role already exists.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
ELSE
	BEGIN		
		
		DECLARE @UpdateDT datetime2 = GETDATE(),
				@SystemUserRoleID int
			
		INSERT INTO [dbo].[SystemUserRole]
				   ([UserID]
				   ,[RoleID]
				   ,[UpdateDT])		 
		OUTPUT inserted.SystemUserRoleID INTO @Inserted
			 VALUES
				   (
					@UserID, 
					@RoleID, 
					@UpdateDT
					)		 
		SELECT @SystemUserRoleID = ID FROM @Inserted
		
		
		
		IF @LineOfBusinessID IS NOT NULL AND 
			@RoleID IN 
				(
					SELECT RoleID FROM dbo.RoleLU 
					WHERE Role IN ('Viewer', 'Proposal Setup Administrator')
				)
				
			BEGIN
			/*
			Process LineOfBusiness
			*/
			IF RIGHT(@LineOfBusinessID, 1) <> ','
				SET @LineOfBusinessID = @LineOfBusinessID + ','

			DECLARE @LineOfBusiness TABLE (LineOfBusinessID INT)


			WHILE (SELECT CHARINDEX (',', @LineOfBusinessID) ) > 1
			BEGIN
	
			INSERT INTO @LineOfBusiness
			SELECT LEFT (@LineOfBusinessID, CHARINDEX (',', @LineOfBusinessID) -1)
			SET @LineOfBusinessID = RIGHT (@LineOfBusinessID, LEN (@LineOfBusinessID) - CHARINDEX (',', @LineOfBusinessID) )
			;


			WITH [Target] AS 
				(
					SELECT 
						SystemUserRoleID AS SystemUserRoleID,
						LineOfBusinessID AS LineOfBusinessID
					FROM [dbo].[LineOfBusinessRoleXREF]
					WHERE SystemUserRoleID = @SystemUserRoleID
				)
			MERGE INTO [Target]
			USING	(
					SELECT DISTINCT 
						@SystemUserRoleID AS SystemUserRoleID, 
						LineOfBusinessID AS LineOfBusinessID
					FROM @LineOfBusiness
					)  AS [Source] ON
					[Target].[SystemUserRoleID] = [Source].[SystemUserRoleID] AND
					[Target].[LineOfBusinessID] = [Source].[LineOfBusinessID]
			WHEN NOT MATCHED BY SOURCE 
			THEN 
			DELETE	

			WHEN NOT MATCHED BY TARGET THEN
			INSERT (SystemUserRoleID, LineOfBusinessID)
			VALUES ([Source].[SystemUserRoleID], [Source].[LineOfBusinessID])
			;	

	
END
			
			END
	END

IF @@ERROR = 0
	SELECT @SystemUserRoleID as SystemUserRoleID

GO