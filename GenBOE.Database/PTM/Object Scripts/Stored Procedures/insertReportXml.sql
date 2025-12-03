IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertReportXml]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertReportXml];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertReportXml]
(
@Nonce varchar(40),
@xml VARCHAR(MAX)
)
AS
/******************************************************************************
**		 
**		Name:	[insertReportXml]
**		Desc:	Insert Report XML
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 11/25/25
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

INSERT INTO [dbo].[ReportXmlData]
			([Nonce]
			,[Xml]
			,[UpdateDT]
			)
VALUES
		(
		@Nonce,
		@xml,
		GetDate()
		)

GO