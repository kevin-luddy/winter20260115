/*

### DO NOT EXECUTE AS A PART OF ANY RELEASE ###





From: Turk, Dan (US) 
Sent: Monday, February 01, 2016 9:09 AM
To: Basquill, Mike (US) <mike.basquill@lmco.com>
Subject: RE: DB Copy Job

Ah, right. I never created that SP in the MSDB database on COSSQL23.  I just grabbed it from old vfassql23 after bringing it on line briefly. 
However, there should be a copy of that SP somewhere in your version control system. You might want to check for it….actually, there are 5 total.  
If they are not already in your version control, please add them from below

*/


USE [msdb]
GO

/****** Object:  StoredProcedure [dbo].[ExecuteGenBOECloneJob]    Script Date: 2/1/2016 9:08:17 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[ExecuteGenBOECloneJob]
(
@Job_Name varchar (2500)
)
WITH EXECUTE AS OWNER 
AS


IF @Job_Name NOT IN 
(
'DatabaseCloneSync2_GenBOE_ProdToGenBOE',
'DatabaseCloneSync2_GenBOE_ProdToGenBOE_DEVProdClone',
'DatabaseCloneSync2_GenBOE_ProdToProdClone',
'DatabaseCloneSync2_GenBOE_TestToDevMain',
'DatabaseCloneSync2_GenBOEMST_ProdToGenBOEMST_DEVProdClone',
'DatabaseCloneSync2_GenBOEMST_ProdToProdClone',
'DatabaseCloneSync2_GenBOEMST_TestToDevMain',

'DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace',
'DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace_DEVProdClone',
'DatabaseCloneSync2_GenBOESpace_ProdToProdClone',
'DatabaseCloneSync2_GenBOESpace_TestToDevMain',

'DatabaseCloneSync2_GenBOE_ProdToGenBOEPerf',
'DatabaseCloneSync2_GenBOESpace_ProdToGenBOEPerf',
'DatabaseCloneSync2_GenBOEMST_ProdToGenBOEPerf'

)
BEGIN
       SELECT 'Permissions have not been added for the job: ' + @Job_Name
       RETURN
END

/*
sample
EXECUTE msdb.dbo.sp_start_job @job_name = 'DatabaseCloneSync2_GenBOE_ProdToProdClone'
*/

/*Job Name Choises:
DatabaseCloneSync2_GenBOE_ProdToGenBOE
DatabaseCloneSync2_GenBOE_ProdToGenBOE_DEVProdClone
DatabaseCloneSync2_GenBOE_ProdToProdClone
DatabaseCloneSync2_GenBOE_TestToDevMain

DatabaseCloneSync2_GenBOEMST_ProdToGenBOEMST_DEVProdClone
DatabaseCloneSync2_GenBOEMST_ProdToProdClone
DatabaseCloneSync2_GenBOEMST_TestToDevMain

DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace
DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace_DEVProdClone
DatabaseCloneSync2_GenBOESpace_ProdToProdClone
DatabaseCloneSync2_GenBOESpace_TestToDevMain


I added these before they were created - these may need an edit

DatabaseCloneSync2_GenBOE_ProdToGenBOEPerf
DatabaseCloneSync2_GenBOESpace_ProdToGenBOEPerf
DatabaseCloneSync2_GenBOEMST_ProdToGenBOEPerf

*/



EXECUTE msdb.dbo.sp_start_job @job_name




GO

/****** Object:  StoredProcedure [dbo].[ExecuteGenBOEProductionToProductionCloneJob]    Script Date: 2/1/2016 9:08:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/****** Object:  StoredProcedure [dbo].[ExecuteGenBOEProductionToProductionCloneJob]    Script Date: 08/11/2014 12:18:59 ******/
CREATE PROCEDURE [dbo].[ExecuteGenBOEProductionToProductionCloneJob]
(
@Job_Name varchar (2500)
)
WITH EXECUTE AS OWNER 
AS


IF @Job_Name NOT IN 
(
'DatabaseCloneSync2_GenBOE_ProdToGenBOE',
'DatabaseCloneSync2_GenBOE_ProdToGenBOE_DEVProdClone',
'DatabaseCloneSync2_GenBOE_ProdToProdClone',
'DatabaseCloneSync2_GenBOE_TestToDevMain',
'DatabaseCloneSync2_GenBOEMST_ProdToGenBOEMST_DEVProdClone',
'DatabaseCloneSync2_GenBOEMST_ProdToProdClone',
'DatabaseCloneSync2_GenBOEMST_TestToDevMain',

'DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace',
'DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace_DEVProdClone',
'DatabaseCloneSync2_GenBOESpace_ProdToProdClone',
'DatabaseCloneSync2_GenBOESpace_TestToDevMain',

'DatabaseCloneSync2_GenBOE_ProdToGenBOEPerf',
'DatabaseCloneSync2_GenBOESpace_ProdToGenBOEPerf',
'DatabaseCloneSync2_GenBOEMST_ProdToGenBOEPerf'

)
BEGIN
       SELECT 'Permissions have not been added for the job: ' + @Job_Name
       RETURN
END

/*
sample
EXECUTE msdb.dbo.sp_start_job @job_name = 'DatabaseCloneSync2_GenBOE_ProdToProdClone'
*/

/*Job Name Choises:
DatabaseCloneSync2_GenBOE_ProdToGenBOE
DatabaseCloneSync2_GenBOE_ProdToGenBOE_DEVProdClone
DatabaseCloneSync2_GenBOE_ProdToProdClone
DatabaseCloneSync2_GenBOE_TestToDevMain

DatabaseCloneSync2_GenBOEMST_ProdToGenBOEMST_DEVProdClone
DatabaseCloneSync2_GenBOEMST_ProdToProdClone
DatabaseCloneSync2_GenBOEMST_TestToDevMain

DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace
DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace_DEVProdClone
DatabaseCloneSync2_GenBOESpace_ProdToProdClone
DatabaseCloneSync2_GenBOESpace_TestToDevMain


I added these before they were created - these may need an edit

DatabaseCloneSync2_GenBOE_ProdToGenBOEPerf
DatabaseCloneSync2_GenBOESpace_ProdToGenBOEPerf
DatabaseCloneSync2_GenBOEMST_ProdToGenBOEPerf

*/



EXECUTE msdb.dbo.sp_start_job @job_name




GO

/****** Object:  StoredProcedure [dbo].[getStatusGenBOECloneJob]    Script Date: 2/1/2016 9:08:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[getStatusGenBOECloneJob]
(
@Job_Name varchar (2500)
)

WITH EXECUTE AS OWNER 
AS



IF @Job_Name NOT IN 
(
'DatabaseCloneSync2_GenBOE_ProdToGenBOE',
'DatabaseCloneSync2_GenBOE_ProdToGenBOE_DEVProdClone',
'DatabaseCloneSync2_GenBOE_ProdToProdClone',
'DatabaseCloneSync2_GenBOE_TestToDevMain',
'DatabaseCloneSync2_GenBOEMST_ProdToGenBOEMST_DEVProdClone',
'DatabaseCloneSync2_GenBOEMST_ProdToProdClone',
'DatabaseCloneSync2_GenBOEMST_TestToDevMain',

'DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace',
'DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace_DEVProdClone',
'DatabaseCloneSync2_GenBOESpace_ProdToProdClone',
'DatabaseCloneSync2_GenBOESpace_TestToDevMain',

'DatabaseCloneSync2_GenBOE_ProdToGenBOEPerf',
'DatabaseCloneSync2_GenBOESpace_ProdToGenBOEPerf',
'DatabaseCloneSync2_GenBOEMST_ProdToGenBOEPerf'

)
BEGIN
       SELECT 'Permissions have not been added for the job: ' + @Job_Name
       RETURN
END


/*Job Name Choises:
DatabaseCloneSync2_GenBOE_ProdToGenBOE
DatabaseCloneSync2_GenBOE_ProdToGenBOE_DEVProdClone
DatabaseCloneSync2_GenBOE_ProdToProdClone
DatabaseCloneSync2_GenBOE_TestToDevMain

DatabaseCloneSync2_GenBOEMST_ProdToGenBOEMST_DEVProdClone
DatabaseCloneSync2_GenBOEMST_ProdToProdClone
DatabaseCloneSync2_GenBOEMST_TestToDevMain

DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace
DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace_DEVProdClone
DatabaseCloneSync2_GenBOESpace_ProdToProdClone
DatabaseCloneSync2_GenBOESpace_TestToDevMain


I added these before they were created - these may need an edit

DatabaseCloneSync2_GenBOE_ProdToGenBOEPerf
DatabaseCloneSync2_GenBOESpace_ProdToGenBOEPerf
DatabaseCloneSync2_GenBOEMST_ProdToGenBOEPerf

*/



SELECT 
     JV.name AS JobName,
     last_outcome_message as message,
     CASE last_run_outcome
              WHEN 0 THEN 'Failed'
              WHEN 1 THEN 'Succeeded'
              WHEN 2 THEN 'Retry'
              WHEN 3 THEN 'Canceled'
       END AS run_status,
    LEFT (last_run_date, 4) + '/' + SUBSTRING (CAST(last_run_date AS varchar (25)), 5, 2) + '/' + RIGHT (last_run_date,2) as run_date,
       CASE 
              WHEN LEN (last_run_time) = 6 THEN LEFT (last_run_time, 2)
              WHEN LEN (last_run_time) = 5 THEN LEFT (last_run_time, 1)
       END + ':' + LEFT (RIGHT (last_run_time, 4), 2) + ':' + RIGHT (last_run_time, 2)


  FROM msdb.dbo.sysjobs_view  JV
       INNER JOIN msdb.dbo.sysjobservers JS ON JV.job_id = JS.job_id
WHERE 
       JV.[name] = @Job_Name
       



GO

/****** Object:  StoredProcedure [dbo].[getStatusGenBOEProductionToProductionCloneJob]    Script Date: 2/1/2016 9:08:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/****** Object:  StoredProcedure [dbo].[getStatusGenBOEProductionToProductionCloneJob]    Script Date: 08/11/2014 12:56:28 ******/
CREATE PROCEDURE [dbo].[getStatusGenBOEProductionToProductionCloneJob]
(
@Job_Name varchar (2500)
)

WITH EXECUTE AS OWNER 
AS



IF @Job_Name NOT IN 
(
'DatabaseCloneSync2_GenBOE_ProdToGenBOE',
'DatabaseCloneSync2_GenBOE_ProdToGenBOE_DEVProdClone',
'DatabaseCloneSync2_GenBOE_ProdToProdClone',
'DatabaseCloneSync2_GenBOE_TestToDevMain',
'DatabaseCloneSync2_GenBOEMST_ProdToGenBOEMST_DEVProdClone',
'DatabaseCloneSync2_GenBOEMST_ProdToProdClone',
'DatabaseCloneSync2_GenBOEMST_TestToDevMain',

'DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace',
'DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace_DEVProdClone',
'DatabaseCloneSync2_GenBOESpace_ProdToProdClone',
'DatabaseCloneSync2_GenBOESpace_TestToDevMain',

'DatabaseCloneSync2_GenBOE_ProdToGenBOEPerf',
'DatabaseCloneSync2_GenBOESpace_ProdToGenBOEPerf',
'DatabaseCloneSync2_GenBOEMST_ProdToGenBOEPerf'

)
BEGIN
       SELECT 'Permissions have not been added for the job: ' + @Job_Name
       RETURN
END


/*Job Name Choises:
DatabaseCloneSync2_GenBOE_ProdToGenBOE
DatabaseCloneSync2_GenBOE_ProdToGenBOE_DEVProdClone
DatabaseCloneSync2_GenBOE_ProdToProdClone
DatabaseCloneSync2_GenBOE_TestToDevMain

DatabaseCloneSync2_GenBOEMST_ProdToGenBOEMST_DEVProdClone
DatabaseCloneSync2_GenBOEMST_ProdToProdClone
DatabaseCloneSync2_GenBOEMST_TestToDevMain

DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace
DatabaseCloneSync2_GenBOESpace_ProdToGenBOESpace_DEVProdClone
DatabaseCloneSync2_GenBOESpace_ProdToProdClone
DatabaseCloneSync2_GenBOESpace_TestToDevMain


I added these before they were created - these may need an edit

DatabaseCloneSync2_GenBOE_ProdToGenBOEPerf
DatabaseCloneSync2_GenBOESpace_ProdToGenBOEPerf
DatabaseCloneSync2_GenBOEMST_ProdToGenBOEPerf

*/



SELECT 
     JV.name AS JobName,
     last_outcome_message as message,
     CASE last_run_outcome
              WHEN 0 THEN 'Failed'
              WHEN 1 THEN 'Succeeded'
              WHEN 2 THEN 'Retry'
              WHEN 3 THEN 'Canceled'
       END AS run_status,
    LEFT (last_run_date, 4) + '/' + SUBSTRING (CAST(last_run_date AS varchar (25)), 5, 2) + '/' + RIGHT (last_run_date,2) as run_date,
       CASE 
              WHEN LEN (last_run_time) = 6 THEN LEFT (last_run_time, 2)
              WHEN LEN (last_run_time) = 5 THEN LEFT (last_run_time, 1)
       END + ':' + LEFT (RIGHT (last_run_time, 4), 2) + ':' + RIGHT (last_run_time, 2)


  FROM msdb.dbo.sysjobs_view  JV
       INNER JOIN msdb.dbo.sysjobservers JS ON JV.job_id = JS.job_id
WHERE 
       JV.[name] = @Job_Name
       



GO

/****** Object:  StoredProcedure [dbo].[JimBasilio_RunGenBoeJob_RestoreProdToProdClone]    Script Date: 2/1/2016 9:08:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[JimBasilio_RunGenBoeJob_RestoreProdToProdClone]
with execute as owner
as
exec sp_start_job @job_name = 'DatabaseCloneSync2_GenBOE_ProdToProdClone'

GO
