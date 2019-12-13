IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertAttachment];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertAttachment]
(
	  @AttachmentID [int],
	  @UpdateDate [datetime2](7),
	  @Name [varchar](100),
	  @Contents [varbinary](max),
	  @UploadedBy  [varchar] (1000),
	  @AttachmentType [int],
	  @ProposalID [int]
)
AS
/******************************************************************************
**          
**          Name: [upsertAttachment]
**          Desc: Insert/Update Attachment
**                
**          
**
**          Auth: twilson3
**          Date: 9/7/2017
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ---------------------------------------
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF @AttachmentID  < 0  /*Insert Record*/
	BEGIN
		DECLARE @Inserted AS Table (ID int)
		SET @UpdateDate = GETDATE()

	INSERT INTO [dbo].[Attachment]
		([UpdateDate]
		,[Name]
		,[Contents]
		,[UploadedBy]
		,[AttachmentType]
		,[ProposalID]
		)
	OUTPUT inserted.ID INTO @Inserted
	VALUES
		(@UpdateDate
		,@Name
		,@Contents
		,@UploadedBy
		,@AttachmentType
		,@ProposalID
		)

		SELECT @AttachmentID = ID FROM @Inserted
	END
ELSE
	/*Update*/
	BEGIN
		IF (SELECT UpdateDate FROM [dbo].[Attachment] WHERE ID = @AttachmentID AND ProposalID = @ProposalID AND [AttachmentType] = @AttachmentType) = @UpdateDate
			BEGIN
				SET @UpdateDate = GETDATE()
							  
				UPDATE [dbo].[Attachment]
					SET  [UpdateDate] = @UpdateDate
						,[Name] = @Name
						,[Contents] = @Contents
						,[UploadedBy] = @UploadedBy
						WHERE ID = @AttachmentID;
			END
		ELSE
			BEGIN
				SET @ErrorMessage =   'The Attachment with ID ' + CAST(@AttachmentID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
						@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
				RETURN
			END
	END

IF @@ERROR = 0
	SELECT @AttachmentID as AttachmentID

GO