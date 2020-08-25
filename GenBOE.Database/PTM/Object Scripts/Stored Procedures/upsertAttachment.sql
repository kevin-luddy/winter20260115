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
	  @ProposalID [int],
	  @IsRevisionReference [bit]
)
AS
/******************************************************************************
**          
**          Name: [upsertAttachment]
**          Desc: Insert/Update Attachment
**          
**
**          Auth: twilson3
**          Date: 9/7/2017
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:         Description:
**			--------    --------        ---------------------------------------
**          7/1/2020	Dusan			BOEJ-4638 Post submittal attachments working with Revisioned Proposal
******************************************************************************/
	SET NOCOUNT ON 

	IF @AttachmentID < 0 -- Inserting a new attachment
		BEGIN
			DECLARE @Inserted AS Table (ID int)
			INSERT INTO [dbo].[Attachment] (UpdateDate, Name, Contents, UploadedBy)
				OUTPUT inserted.ID INTO @Inserted
				VALUES (GETDATE(), @Name, @Contents, @UploadedBy)
			SELECT @AttachmentID = ID FROM @Inserted
		END
	ELSE IF @IsRevisionReference = 0 -- updating an existing attachment
		BEGIN
			IF (SELECT UpdateDate FROM Attachment WHERE ID = @AttachmentID) = @UpdateDate
				UPDATE Attachment
					SET UpdateDate = GETDATE(), Name = @Name, Contents = @Contents, UploadedBy = @UploadedBy
					WHERE ID = @AttachmentID
			ELSE
				BEGIN
					DECLARE @ErrorMessage varchar (500)
					SET @ErrorMessage = 'The Attachment with ID ' + CAST(@AttachmentID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (@ErrorMessage, 11, 1)
					RETURN
				END
		END

	DELETE FROM ProposalsAttachments WHERE ProposalId = @ProposalID AND AttachmentId = @AttachmentID
	INSERT INTO ProposalsAttachments (ProposalId, AttachmentId, IsRevisionReference, AttachmentType) 
		VALUES (@ProposalID, @AttachmentID, @IsRevisionReference, @AttachmentType)

	IF @@ERROR = 0
		SELECT @AttachmentID as AttachmentID

GO