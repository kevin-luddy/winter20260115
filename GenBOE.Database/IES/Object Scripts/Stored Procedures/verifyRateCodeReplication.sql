IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[verifyRateCodeReplication]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[verifyRateCodeReplication];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[verifyRateCodeReplication]
(
	 @RevisionId int
)
AS
	/******************************************************************************
	**		 
	**		Name:	[verifyRateCodeReplication]
	**		Desc:	Verifies the Rate Codes saved in the Replication Table against the Revision Id passed in
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 5/29/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	
	SELECT [FROM] as RateCode, 'From' as ColumnName FROM [dbo].[RateCodeReplication] WHERE [FROM] NOT IN (SELECT RateCode FROM [dbo].[RateCode] WHERE RevisionID = @RevisionId)
	UNION
	SELECT [TO] as RateCode, 'To' as ColumnName FROM [dbo].[RateCodeReplication] WHERE [TO] NOT IN (SELECT RateCode FROM [dbo].[RateCode] WHERE RevisionID = @RevisionId)
	UNION
	SELECT [TO] as RateCode, 'Both To and From' as ColumnName FROM [dbo].[RateCodeReplication] WHERE [TO] IN (SELECT DISTINCT [FROM] FROM [dbo].[RateCodeReplication])
GO