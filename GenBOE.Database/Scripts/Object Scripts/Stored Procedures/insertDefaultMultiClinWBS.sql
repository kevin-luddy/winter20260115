IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertDefaultMultiClinWBS]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertDefaultMultiClinWBS];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertDefaultMultiClinWBS]
(
@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: [insertDefaultMultiClinWBS]
**		Desc: Inserts default Multi CLIN and WBS elements in to a workspace. 
**			
**		
**
**		Auth: Matthew Kotwicki
**		Date: 10/14/2015
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		12/7/2017	twilson3			BOEJ-2250 Remove DTC
*******************************************************************************/

SET NOCOUNT ON 
DECLARE @UpdateDT datetime2  = GetDate()

If NOT EXISTS( select WBSID from WorkBreakdownStructure
			   where WorkspaceID = @WorkspaceID
			   and WBSNumber = 'MULTI')
									BEGIN
									INSERT into WorkBreakdownStructure (UpdateDT,WBSNumber, DisplayedWBSNumber, WBSTitle, WorkspaceID)
									VALUES (@UpdateDT, 'MULTI', 'MULTI', 'MULTI', @WorkspaceID);
									END
If NOT EXISTS( select CLINID from CLIN
               where WorkspaceID = @WorkspaceID
			   and CLINNumber = 'MULTI')
									BEGIN
									insert into CLIN (UpdateDT,CLINNumber, ClinTitle, CLINStartDate, CLINEndDate, WorkspaceID, DisplayedCLINNumber)
									VALUES (@UpdateDT, 'MULTI', 'MULTI', NULL,NULL,  @WorkspaceID, 'MULTI');
								    END
GO