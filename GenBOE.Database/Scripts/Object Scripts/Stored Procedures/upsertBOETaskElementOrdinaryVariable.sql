IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOETaskElementOrdinaryVariable]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOETaskElementOrdinaryVariable];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOETaskElementOrdinaryVariable]
(
@OrdinaryVariableID int,
@OrdinaryVariableName varchar(50),
@OrdinaryVariableValue decimal (29,10),
@SortByID int,
@ValueTypeID int,
@BOETaskElementID int,
@IsPercentage bit,
@UpdateDT datetime2,
@DefaultSize varchar (200)
)
AS
/******************************************************************************
**		 
**		Name: [upsertBOETaskElementOrdinaryVariable]
**		Desc: Insert/Update Ordinary Variable data into BOE Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/4/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		3/17/11		dcanuso				New columns added to Workspace Variable Table
**		3/24/11		dcanuso				variable changed to decimal (29,10)
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		8/3/11		dcanuso				Developer Request to Add IsPercentage
**		8/31/11		dcanuso				User is now able to select the Resource
**										Types that will be used for Summing BOEs
**		9/13/11		dcanuso				Moving 8/31 code to new insert/delete SP
**		12/19/13	dcanuso				WI 24983 - @DefaultSize varchar (200) added
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedOrdinaryVariable AS Table (OrdinaryVariableID int)
DECLARE	@ErrorMessage varchar (500)


IF @OrdinaryVariableID  < 0  /*Insert Record*/
	BEGIN

		IF EXISTS	(SELECT 1 FROM [dbo].[OrdinaryVariable]
						WHERE	BOETaskElementID = @BOETaskElementID AND
								OrdinaryVariableName = @OrdinaryVariableName
					)
			BEGIN
				SET @ErrorMessage =   'There is already an Ordinary Variable with Ordinary Variable Name ' + @OrdinaryVariableName
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
				
				INSERT INTO [dbo].[OrdinaryVariable]
					   ([OrdinaryVariableName]
					   ,[OrdinaryVariableValue]
					   ,[SortByID]
					   ,[ValueTypeID]
					   ,[BOETaskElementID]
					   ,[IsPercentage]
					   ,[UpdateDT]
					   ,[DefaultSize]
					   )
				 OUTPUT inserted.OrdinaryVariableID INTO @InsertedOrdinaryVariable
					  VALUES
					   (@OrdinaryVariableName
					   ,@OrdinaryVariableValue
					   ,@SortByID
					   ,@ValueTypeID
					   ,@BOETaskElementID
					   ,@IsPercentage
					   ,@UpdateDT
					   ,@DefaultSize
					   )

            
				SELECT @OrdinaryVariableID = OrdinaryVariableID FROM @InsertedOrdinaryVariable
				
				
			END
	END			
ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[OrdinaryVariable] WHERE OrdinaryVariableID = @OrdinaryVariableID) = @UpdateDT
		BEGIN
		
		IF EXISTS	(SELECT 1 FROM [dbo].[OrdinaryVariable]
						WHERE	BOETaskElementID = @BOETaskElementID AND
								OrdinaryVariableName = @OrdinaryVariableName AND
								OrdinaryVariableID <> @OrdinaryVariableID
					)
			BEGIN
				SET @ErrorMessage =   'There is already an Ordinary Variable with Ordinary Variable Name ' + @OrdinaryVariableName
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
		ELSE
			BEGIN
			SET @UpdateDT = GetDate()
			
			UPDATE [dbo].[OrdinaryVariable]
				SET [OrdinaryVariableName] = @OrdinaryVariableName,
					[OrdinaryVariableValue] = @OrdinaryVariableValue,
					[SortByID] = @SortByID,
					[ValueTypeID] = @ValueTypeID,
					[IsPercentage] = @IsPercentage,
					[UpdateDT] = @UpdateDT,
					[DefaultSize] = @DefaultSize
			WHERE
				OrdinaryVariableID = @OrdinaryVariableID
				
			
			END
		END			
	ELSE
		BEGIN
		
			SET @ErrorMessage =  'The Ordinary Variable with Name ' + @OrdinaryVariableName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END


IF @@ERROR = 0
	SELECT @OrdinaryVariableID AS OrdinaryVariableID
GO