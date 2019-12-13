IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertOutputFormatTemplate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertOutputFormatTemplate];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertOutputFormatTemplate]
(
@TemplateID int,
@Template varchar(100),
@TemplateDescription varchar(500),
@TemplateFile varbinary(max),
@UpdateDT datetime2,
@ParentTemplateID int,
@IsAvailableToAllWorkspaces bit
)
AS
/******************************************************************************
**		 
**		Name: [upsertOutputFormatTemplate]
**		Desc: Insert/Update Output Format Template in table
**
**		
**
**		Auth: Don Canuso
**		Date: 9/10/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		10/28/13	dcanuso				WI 23578 Parent added to Template
**		5/1/2018	ranzalon			BOEJ-3416 - Available to all Workspaces
**		8/29/2019	ranzalon			Fixed missing else for updated records
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE	@ErrorMessage varchar (500)

IF @TemplateID < 0  /*Insert Record*/
	BEGIN
	
	IF EXISTS	(SELECT 1 FROM [dbo].[OutputFormatTemplate]
				WHERE	Template = @Template
			)
	BEGIN
		SET @ErrorMessage =   'There is already a Template named ' + @Template
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
	
	INSERT INTO [dbo].[OutputFormatTemplate]
           ([Template]
           ,[TemplateDescription]
           ,[TemplateFile]
           ,[UpdateDT]
           ,[IsActive]
           ,[ParentTemplateID]
		   ,[IsAvailableToAllWorkspaces]
           )
     OUTPUT inserted.TemplateID INTO @Inserted
     VALUES
           (@Template,
            @TemplateDescription,
            @TemplateFile,
            @UpdateDT,
            1 /* Active on Insert*/,
            @ParentTemplateID,
			@IsAvailableToAllWorkspaces
            )
           
      SELECT @TemplateID = ID FROM @Inserted
		
	END
	
END
/*Update*/
ELSE IF @TemplateID > 0 AND
  (SELECT UpdateDT FROM [dbo].[OutputFormatTemplate] WHERE TemplateID = @TemplateID) = @UpdateDT
BEGIN	
		SET @UpdateDT = GETDATE()
		
		UPDATE [dbo].[OutputFormatTemplate]
			SET 
				[UpdateDT] = @UpdateDT
			   ,[Template] = @Template
		       ,[TemplateDescription] = @TemplateDescription
		       ,[TemplateFile] = @TemplateFile
		       ,[IsActive] = 1 /* If you are updating it - it must be active*/
		       ,[ParentTemplateID] = @ParentTemplateID
			   ,[IsAvailableToAllWorkspaces] = @IsAvailableToAllWorkspaces
		WHERE TemplateID = @TemplateID
END
ELSE
	BEGIN
				SET @ErrorMessage =   'The Output Format Template with ID ' + CAST(@TemplateID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN 
	END

IF @@ERROR = 0
	SELECT	@TemplateID AS TemplateID
GO