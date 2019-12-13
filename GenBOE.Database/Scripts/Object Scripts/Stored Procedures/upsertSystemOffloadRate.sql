IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertSystemOffloadRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertSystemOffloadRate];

GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertSystemOffloadRate]
(
@OffloadRateID int,
@Resource varchar(20),
@PerfOrg varchar(20),
@PercentToOffload decimal (4,3),
@Year int,
@SubcontractorResource varchar(20),
@HourlyRate decimal (7,2),
@UpdateDT datetime2(7)
)
AS
/******************************************************************************
**		 
**		Name: [upsertSystemOffloadRate]
**		Desc: Insert/Update System Offload Rate
**
**		Auth: RJ Anzalone
**		Date: 4/25/17
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE	@ErrorMessage varchar (500)

IF @OffloadRateID < 0 /*Insert Record*/
	BEGIN
		/*Combination of Resource, Perf Org, and Year must be unique*/
		IF EXISTS (SELECT 1 FROM [dbo].[SystemOffloadRate] WHERE [Resource] = @Resource AND [PerfOrg] = @PerfOrg AND [Year] = @YEAR)
		BEGIN
			SET @ErrorMessage = 'There is already a System Offload Rate for the combination of Resource ' + @Resource + ', Performing Organization ' + @PerfOrg + ', and Year ' + CAST (@Year AS varchar(4)) + '.'
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END
	
		SET @UpdateDT = GETDATE()
		
		INSERT INTO [dbo].[SystemOffloadRate]
			([Resource],
			[PerfOrg],
			[PercentToOffload],
			[Year],
			[SubcontractorResource],
			[HourlyRate],
			[UpdateDT])
		OUTPUT inserted.OffloadRateID INTO @Inserted
		VALUES
			(@Resource,
			@PerfOrg,
			@PercentToOffload,
			@Year,
			@SubcontractorResource,
			@HourlyRate,
			@UpdateDT)
		SELECT @OffloadRateID = ID FROM @Inserted
	END
ELSE /*Update Record*/
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[SystemOffloadRate] WHERE OffloadRateID = @OffloadRateID) = @UpdateDT
			BEGIN	
				SET @UpdateDT = GETDATE()
				
				UPDATE [dbo].[SystemOffloadRate]
				SET [Resource] = @Resource,
					[PerfOrg] = @PerfOrg,
					[PercentToOffload] = @PercentToOffload,
					[Year] = @Year,
					[SubcontractorResource] = @SubcontractorResource,
					[HourlyRate] = @HourlyRate,
					[UpdateDT] = @UpdateDT
				WHERE [OffloadRateID] = @OffloadRateID
			END
		ELSE
		BEGIN
			SET @ErrorMessage =   'The System Offload Rate with ID ' + CAST(@OffloadRateID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END
	END
IF @@ERROR = 0
	SELECT	@OffloadRateID AS OffloadRateID

GO