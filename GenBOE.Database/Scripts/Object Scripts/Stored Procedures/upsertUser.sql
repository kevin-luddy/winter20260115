IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertUser];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertUser]
(
@ETIUserID int,
@NTID varchar(1000),
@DisplayName varchar(256),
@EmailAddress varchar(50),
@PhoneNumber varchar(20),
@FirstName varchar(25),
@LastName varchar(25),
@UpdateDT datetime2,
@IsUsPerson bit,
@IsSubcontractor bit
)
AS
/******************************************************************************
**		 
**		Name: upsertUser
**		Desc: We add users via the Workspace Permissions section
**			
**		
**
**		Auth: Don Canuso
**		Date: June 2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/18/10		dcanuso				Deleted and Update Date added to ETI User Table, 
**											Group funcationality added, Update Date Added	
**		9/02/10		dcanuso				We are currently only adding users via 
**											Workspace Permissions  - Removing Group input as this
**											will be handled by Workspace Permissions
**		9/13/10		dcanuso				Soft deletes removed
**		2/17/11		dcanuso				FirstName and LastName added
**		5/9/11		dcanuso				Task 3364:ETIUser table - 
**										needs to be able to have duplicate ntid made unique by the domain
**										Checking for dup in SP
**		9/5/12		mbasquil			WI 10910 Added IsNull() checks to properly
**										handle null values	
**		11/21/13	dcanuso				User NTID Increased
**		3/7/17		Joe					Increased size of Display Name
**		11/28/17	pattoncr			Remove domain.
**		10/29/19	ranzalon			Add IsUsPerson, IsSubcontractor
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedETIUser AS Table (ETIUserID int)
DECLARE @ErrorMessage varchar (500)

IF @ETIUserID < 0 
	IF EXISTS (SELECT 1 FROM dbo.ETIuser 
				WHERE NTID = @NTID
			   )
				BEGIN

				SET @ErrorMessage =   'A user with this NTID already exists'
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

			INSERT INTO [dbo].[ETIuser]
				   ([NTID]
				   ,[DisplayName]
				   ,[EmailAddress]
				   ,[PhoneNumber]
				   ,[FirstName]
				   ,[LastName]
				   ,[UpdateDT]
				   ,[IsUsPerson]
				   ,[IsSubcontractor])
			 OUTPUT inserted.ETIUserID INTO @InsertedETIUser
			 VALUES
				   (@NTID
				   ,@DisplayName
				   ,@EmailAddress
				   ,@PhoneNumber
				   ,@FirstName
				   ,@LastName
				   ,@UpdateDT
				   ,@IsUsPerson
				   ,@IsSubcontractor
				   )
		           
			  SELECT @ETIUserID = ETIUserID FROM @InsertedETIUser
									
		 END
ELSE
	BEGIN
	IF (SELECT UpdateDT FROM dbo.ETIuser WHERE ETIUserID = @ETIUserID) = @UpdateDT
		BEGIN 
			IF EXISTS (SELECT 1 FROM dbo.ETIuser 
						WHERE NTID = @NTID AND 
							  ETIUserID <> @ETIUserID
					   )
				BEGIN
					SET @ErrorMessage =   'A user with this NTID already exists'
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
					UPDATE [dbo].[ETIuser]
					   SET	
							[DisplayName] = @DisplayName,
							[EmailAddress] = @EmailAddress,
							[PhoneNumber] = @PhoneNumber,
							[FirstName] = @FirstName,
							[LastName] = @LastName,
							[UpdateDT] = @UpdateDT,
							[IsUsPerson] = @IsUsPerson,
							[IsSubcontractor] = @IsSubcontractor
					WHERE 
						[ETIUserID] = @ETIUserID
				END
		END		
	ELSE
			BEGIN
				SET @ErrorMessage =   'The user with NTID ' + @NTID + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
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