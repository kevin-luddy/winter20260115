IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertOutputFormatTemplate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertOutputFormatTemplate];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertOutputFormatTemplate]
(
@TemplateID int,
@Template varchar(100),
@TemplateDescription varchar(500),
@TemplateFile varbinary(max),
@UpdateDT datetime2,
@ParentTemplateID int
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
**		Date: 3/30/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		10/28/13	dcanuso				WI 23578 Parent added to Template
**		11/1/13		dcanuso				Template Name constraint (unique) being
**										handled in the DB and adding a IsActive = 1 here
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE	@ErrorMessage varchar (500)

IF @TemplateID < 0  /*Insert Record*/
	BEGIN
	
	IF EXISTS	(SELECT 1 FROM [dbo].[OutputFormatTemplate]
				WHERE	Template = @Template AND IsActive = 1
			)
	BEGIN
		SET @ErrorMessage =   'There is already a Template with TemplateName ' + @Template
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
           )
     OUTPUT inserted.TemplateID INTO @Inserted
     VALUES
           (@Template,
            @TemplateDescription,
            @TemplateFile,
            @UpdateDT,
            1 /*Active on New*/,
            @ParentTemplateID
            )
           
      SELECT @TemplateID = ID FROM @Inserted
		
	END
	
	END

IF @@ERROR = 0
	SELECT	@TemplateID AS TemplateID
GO