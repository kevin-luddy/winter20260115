IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getReportXML]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getReportXML];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getReportXML]
(
	@Nonce varchar(40)
)
AS
/******************************************************************************
**		 
**		Name: getReportXML
**		Desc: Returns XML used for reports
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 11/25/25
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @RawXml VARCHAR(MAX);
SET @RawXml = 
(
    SELECT top(1) [Xml] 
    FROM [ReportXmlData] 
    WHERE Nonce = @Nonce
) 

DELETE FROM [ReportXmlData] WHERE Nonce = @Nonce

SELECT @RawXml as Xml

GO
