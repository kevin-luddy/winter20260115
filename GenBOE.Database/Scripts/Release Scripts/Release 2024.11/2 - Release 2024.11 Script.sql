/*
	## START ##

	8/12/2024 [ranzalon] - SLMX_POLM_PROPH-2253: Purge Old Data pt. 2
*/

-- Drop more unused SPs
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElementMetricDetail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElementMetricDetail];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOETaskElementMetricDetailXREF]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOETaskElementMetricDetailXREF];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMetricDetail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMetricDetail];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertMetricDetail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertMetricDetail];

GO

-- Drop unused Tables
IF OBJECT_ID('dbo.ETIGroup', 'U') IS NOT NULL 
  DROP TABLE dbo.ETIGroup; 

GO

IF OBJECT_ID('dbo.RealignedWorkspaceXREF', 'U') IS NOT NULL 
  DROP TABLE dbo.RealignedWorkspaceXREF; 

GO

IF OBJECT_ID('dbo.BOETaskElementMetricDetailXREF', 'U') IS NOT NULL 
  DROP TABLE dbo.BOETaskElementMetricDetailXREF; 

GO
  
IF OBJECT_ID('dbo.MetricDetail', 'U') IS NOT NULL 
  DROP TABLE dbo.MetricDetail; 

GO

IF OBJECT_ID('version.BOETaskElementMetricDetailXREF', 'U') IS NOT NULL 
  DROP TABLE version.BOETaskElementMetricDetailXREF; 

GO

IF OBJECT_ID('version.MetricDetail', 'U') IS NOT NULL 
  DROP TABLE version.MetricDetail; 

GO
  
IF OBJECT_ID('version.BOETaskElementgenDataMetricXREF', 'U') IS NOT NULL 
  DROP TABLE dbo.BOETaskElementgenDataMetricXREF; 

GO

IF OBJECT_ID('version.tempPerformingOrganizationMapping', 'U') IS NOT NULL 
  DROP TABLE dbo.tempPerformingOrganizationMapping;

GO

/*
	8/12/2024 [ranzalon] - SLMX_POLM_PROPH-2253: Purge Old Data pt. 2

	## END ##
*/