IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOETaskElementMetricDetailXREF]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOETaskElementMetricDetailXREF];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[insertBOETaskElementMetricDetailXREF]
(
@BTEMDID int,
@BOETaskElementID int,
@MetricDetailID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [insertBOETaskElementMetricDetailXREF]
**		Desc: Inserts and associated BOE Task Element with Metric
**			
**		
**
**		Auth: Don Canuso
**		Date: 7/30/15
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)

IF @BTEMDID < 0 
	BEGIN

	IF NOT EXISTS 
			(SELECT 1 FROM [dbo].[BOETaskElementMetricDetailXREF] 
				WHERE BOETaskElementID = @BOETaskElementID AND [MetricDetailID] = @MetricDetailID)
		BEGIN	
			SET @UpdateDT = GETDATE()
		
			INSERT INTO [dbo].[BOETaskElementMetricDetailXREF]
			   ([BOETaskElementID]
			   ,[MetricDetailID]
			   ,[UpdateDT])
			OUTPUT inserted.BTEMDID INTO @Inserted
			VALUES
			   (@BOETaskElementID
			   ,@MetricDetailID
			   ,@UpdateDT)
	
			 SELECT @BTEMDID = ID FROM @Inserted
		END
	ELSE
		BEGIN
			SELECT @BTEMDID = BTEMDID FROM [dbo].[BOETaskElementMetricDetailXREF] WHERE BOETaskElementID = @BOETaskElementID AND MetricDetailID = @MetricDetailID 
		END		

	IF @@ERROR = 0
		SELECT  @BTEMDID AS BTEMDID 
	END
GO