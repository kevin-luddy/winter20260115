IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertMetricDetail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertMetricDetail];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertMetricDetail]
(
	@MetricDetailID [int],
	@SourceSystemID [int],
	@ProgramName [varchar](50),
	@ScopeName [varchar](354) ,
	@MeasureData [decimal](38, 18) ,
	@MeasureName [varchar](200),
	@DataSource [varchar](100) ,
	@MeasureFunction [varchar](40) ,
	@Equation [varchar](1000) ,
	@Comment [varchar](2000) ,
	@StartDate [datetime] ,
	@EndDate [datetime] ,
	@ContractNumber [varchar](2000) ,
	@WorkPackages [varchar](max) ,
	@BaseMeasure1Name [varchar](100) ,
	@BaseMeasure1Data [decimal](38, 18) ,
	@ProgramID [int],
	@MeasureFunctionID [varchar](2) ,
	@DataSourceID [int],
	@MeasureID [int],
	@MeasureQualifier [varchar](1000) ,
	@BaseMeasure2Name [varchar](100) ,
	@BaseMeasure2Data [decimal](38, 18) ,
	@BaseMeasure3Name [varchar](100) ,
	@BaseMeasure3Data [decimal](38, 18) ,
	@BaseMeasure4Name [varchar](100) ,
	@BaseMeasure4Data [decimal](38, 18) ,
	@BaseMeasure5Name [varchar](100) ,
	@BaseMeasure5Data [decimal](38, 18) ,
	@BusinessArea [varchar](50),
	@LineOfBusiness [varchar](50) ,
	@MeasureGroupName [varchar](100),
	@MeasureCategoryName [varchar](100),
	@MeasureDescription [varchar](2000) ,
	@MeasureLink [varchar](2000) ,
	@MeasureValidationDate [datetime] ,
	@MeasureValidatedBy [varchar](50) ,
	@ProgramDescription [varchar](1000) 
)
AS
/******************************************************************************
**		 
**		Name:	[insertMetricDetail]
**		Desc:	Insert Metric Detail
**			
**		
**
**		Auth: Don Canuso
**		Date: 7/30/15
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)
DECLARE @Inserted AS Table (ID int)
	INSERT INTO [dbo].[MetricDetail]
           ([SourceSystemID]
           ,[ProgramName]
           ,[ScopeName]
           ,[MeasureData]
           ,[MeasureName]
           ,[DataSource]
           ,[MeasureFunction]
           ,[Equation]
           ,[Comment]
           ,[StartDate]
           ,[EndDate]
           ,[ContractNumber]
           ,[WorkPackages]
           ,[BaseMeasure1Name]
           ,[BaseMeasure1Data]
           ,[ProgramID]
           ,[MeasureFunctionID]
           ,[DataSourceID]
           ,[MeasureID]
           ,[MeasureQualifier]
           ,[BaseMeasure2Name]
           ,[BaseMeasure2Data]
           ,[BaseMeasure3Name]
           ,[BaseMeasure3Data]
           ,[BaseMeasure4Name]
           ,[BaseMeasure4Data]
           ,[BaseMeasure5Name]
           ,[BaseMeasure5Data]
           ,[BusinessArea]
           ,[LineOfBusiness]
           ,[MeasureGroupName]
           ,[MeasureCategoryName]
           ,[MeasureDescription]
           ,[MeasureLink]
           ,[MeasureValidationDate]
           ,[MeasureValidatedBy]
           ,[ProgramDescription])
    OUTPUT inserted.[MetricDetailID] INTO @Inserted     
     VALUES
           (@SourceSystemID
           ,@ProgramName
           ,@ScopeName
           ,@MeasureData
           ,@MeasureName
           ,@DataSource
           ,@MeasureFunction
           ,@Equation
           ,@Comment
           ,@StartDate
           ,@EndDate
           ,@ContractNumber
           ,@WorkPackages
           ,@BaseMeasure1Name
           ,@BaseMeasure1Data
           ,@ProgramID
           ,@MeasureFunctionID
           ,@DataSourceID
           ,@MeasureID
           ,@MeasureQualifier
           ,@BaseMeasure2Name
           ,@BaseMeasure2Data
           ,@BaseMeasure3Name
           ,@BaseMeasure3Data
           ,@BaseMeasure4Name
           ,@BaseMeasure4Data
           ,@BaseMeasure5Name
           ,@BaseMeasure5Data
           ,@BusinessArea
           ,@LineOfBusiness
           ,@MeasureGroupName
           ,@MeasureCategoryName
           ,@MeasureDescription
           ,@MeasureLink
           ,@MeasureValidationDate
           ,@MeasureValidatedBy
           ,@ProgramDescription
		   )
		SELECT [ID] AS [MetricDetailID] FROM @Inserted
GO