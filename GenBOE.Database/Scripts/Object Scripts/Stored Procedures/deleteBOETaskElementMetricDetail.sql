IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElementMetricDetail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElementMetricDetail];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteBOETaskElementMetricDetail]
(
@BTEMDID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElementMetricDetail]
**		Desc: Delete the association between BOE Task Element and Metric
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
	IF (SELECT UpdateDT FROM dbo.BOETaskElementMetricDetailXREF WHERE BTEMDID = @BTEMDID ) = @UpdateDT
		BEGIN
			DELETE FROM dbo.BOETaskElementMetricDetailXREF WHERE BTEMDID = @BTEMDID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Task Element Metric Detail with ID ' + CAST(@BTEMDID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END

GO