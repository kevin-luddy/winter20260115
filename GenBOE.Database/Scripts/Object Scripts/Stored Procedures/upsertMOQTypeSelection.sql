IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMOQTypeSelection]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMOQTypeSelection];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertMOQTypeSelection]
(
	@MOQTypeSelectionId int,
	@TaskId int,
	@MOQTypeSelection int,
	@UpdateDT datetime2,
	@Order int,
	@CERName varchar(255),
	@CERLocation varchar(255),
	@HoursDescription varchar(max),
	@SubjectMatterExpert varchar(max),
	@HoursLogicAndAssumptions varchar(max),
	@DurationLogicAndAssumptions varchar(max),
	@EstimateTasks varchar(max),
	@Rationale varchar(max),
	@SkillMix varchar(max)
)
AS
/******************************************************************************
**		 
**		Name: [upsertMOQTypeSelection]
**		Desc: Insert/Update data into MOQTypeSelection
**			
**		
**
**		Auth: ranzalon
**		Date: 9/11/2020
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @MOQTypeSelection AS Table (MOQTypeSelectionId int)

IF @MOQTypeSelectionId  < 0  /*Insert Record*/
	BEGIN
	SET @UpdateDT = GETDATE()
	INSERT INTO [dbo].[MOQTypeSelection]
           (
				[TaskId],
				[MOQTypeSelection],
				[UpdateDT],
				[Order],
				[CERName],
				[CERLocation],
				[HoursDescription],
				[SubjectMatterExpert],
				[HoursLogicAndAssumptions],
				[DurationLogicAndAssumptions],
				[EstimateTasks],
				[Rationale],
				[SkillMix]
		   )
     OUTPUT inserted.MOQTypeSelectionId INTO @MOQTypeSelection
     VALUES
           (
				@TaskId,
				@MOQTypeSelection,
				@UpdateDT,
				@Order,
				@CERName,
				@CERLocation,
				@HoursDescription,
				@SubjectMatterExpert,
				@HoursLogicAndAssumptions,
				@DurationLogicAndAssumptions,
				@EstimateTasks,
				@Rationale,
				@SkillMix
            ) 
            
	SELECT @MOQTypeSelectionId = MOQTypeSelectionId FROM @MOQTypeSelection

END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[MOQTypeSelection] WHERE MOQTypeSelectionId = @MOQTypeSelectionId) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			 
			UPDATE [dbo].[MOQTypeSelection]
				SET 
					[TaskId] = @TaskId,
					[MOQTypeSelection] = @MOQTypeSelection,
					[UpdateDT] = @UpdateDT,
					[Order] = @Order,
					[CERName] = @CERName,
					[CERLocation] = @CERLocation,
					[HoursDescription] = @HoursDescription,
					[SubjectMatterExpert] = @SubjectMatterExpert,
					[HoursLogicAndAssumptions] = @HoursLogicAndAssumptions,
					[DurationLogicAndAssumptions] = @DurationLogicAndAssumptions,
					[EstimateTasks] = @EstimateTasks,
					[Rationale] = @Rationale,
					[SkillMix] = @SkillMix
			WHERE
				[MOQTypeSelectionId] = @MOQTypeSelectionId

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The MOQ Type Selection with ID ' + CAST(@MOQTypeSelectionId AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @MOQTypeSelectionId AS MOQTypeSelectionId
GO