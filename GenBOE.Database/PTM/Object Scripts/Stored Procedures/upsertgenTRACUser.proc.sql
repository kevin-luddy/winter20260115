IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertgenTRACUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertgenTRACUser];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertgenTRACUser]
(
@UserID int,
@UpdateDT datetime2,
@NTID varchar(1000),
@DisplayName varchar(256),
@EmailAddress varchar(50),
@PhoneNumber varchar(20),
@FirstName varchar(25),
@LastName varchar(25),
@IsGroup bit,
@IsUsPerson bit,
@IsSubcontractor bit
)
AS
/*****************************************************************************
**		 
**		Name: upsertgenTRACUser
**		Desc: Insert/Update genTRAC User Information
**			
**		
**
**		Auth: Don Canuso
**		Date: 4/3/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not 
**										display technical details to the user
**		3/7/2017	Joe					BOEJ-1818 Increase size of the user's display name field
**		11/29/17	pattoncr			Remove domain.
**		6/6/19		Dusan				Added IsUsPerson and IsSubcontractor
**		11/20/19	ranzalon			BOEJ-4400 - fix update for sub/us
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE @ErrorMessage varchar (500)

IF @UserID < 0 
	IF EXISTS (SELECT 1 FROM dbo.genTRACUser WHERE NTID = @NTID)
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

		INSERT INTO [dbo].[genTRACUser]
           ([UpdateDT]
           ,[NTID]
           ,[DisplayName]
           ,[EmailAddress]
           ,[PhoneNumber]
           ,[FirstName]
           ,[LastName]
           ,[IsGroup]
		   ,[IsUsPerson]
		   ,[IsSubcontractor]
           )
		 OUTPUT inserted.UserID INTO @Inserted
         VALUES
           (@UpdateDT
           ,@NTID
           ,@DisplayName
           ,@EmailAddress
           ,@PhoneNumber
           ,@FirstName
           ,@LastName
           ,ISNULL(@IsGroup, 0)
		   ,@IsUsPerson
		   ,@IsSubcontractor
           )
           
		 SELECT @UserID = ID FROM @Inserted
									
		 END
ELSE
	BEGIN
	IF (SELECT UpdateDT FROM dbo.genTRACUser WHERE UserID = @UserID) = @UpdateDT
		BEGIN 
			IF EXISTS	(SELECT 1 
							FROM dbo.genTRACUser 
							WHERE	NTID = @NTID AND 
								UserID <> @UserID
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
					
					UPDATE [dbo].[genTRACUser]
					   SET [UpdateDT] = @UpdateDT
						  ,[NTID] = @NTID
						  ,[DisplayName] = @DisplayName
						  ,[EmailAddress] = @EmailAddress
						  ,[PhoneNumber] = @PhoneNumber
						  ,[FirstName] = @FirstName
						  ,[LastName] = @LastName
						  ,[IsGroup] = ISNULL(@IsGroup, 0)
						  ,[IsUsPerson] = @IsUsPerson
						  ,[IsSubcontractor] = @IsSubcontractor
					WHERE 
						[UserID] = @UserID
				END
		END		
	ELSE
			BEGIN
				SET @ErrorMessage =   'The User with ID ' + CAST(@UserID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
           			
	END


IF @@ERROR = 0
	SELECT @UserID as UserID

GO