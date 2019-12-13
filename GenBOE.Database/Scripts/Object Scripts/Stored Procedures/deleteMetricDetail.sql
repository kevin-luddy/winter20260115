IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMetricDetail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMetricDetail];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteMetricDetail]
(
@MetricDetailID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteMetricDetail]
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
**		9/29/15		mbasquil			BOEJ-416 - updated delete on MetricDetail
**										to remove all orphaned records
*******************************************************************************/
SET NOCOUNT ON 
		BEGIN
			DELETE FROM dbo.BOETaskElementMetricDetailXREF 
				FROM dbo.BOETaskElementMetricDetailXREF X
					INNER JOIN [dbo].[MetricDetail] MD ON X.[MetricDetailID] = MD.[MetricDetailID]
			WHERE MD.[MetricDetailID] = @MetricDetailID

			/*BOEJ-416 Clean up all instances of MetricDetails that are no longer linked to a task in 
			* BOETaskElementMetricDetailXREF. Record cannot always be deleted because it may be in use
			* in other tasks.
			*/
			DELETE FROM [dbo].[MetricDetail]
			WHERE [MetricDetailID] NOT IN 
				(
				SELECT [MetricDetailID] FROM dbo.[BOETaskElementMetricDetailXREF]
				UNION
				SELECT [MetricDetailID] FROM version.[BOETaskElementMetricDetailXREF]
				)
		END

GO