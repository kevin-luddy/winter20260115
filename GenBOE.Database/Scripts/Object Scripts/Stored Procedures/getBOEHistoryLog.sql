IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEHistoryLog]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEHistoryLog];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getBOEHistoryLog]
(
@BOEID int
)
AS
/******************************************************************************
**          
**          Name: getBOEHistoryLog
**          Desc: Returns BOE Comment History along with Status Changes
**                
**          
**
**          Auth: Don Canuso
**          Date: 11/30/2010
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ---------------------------------------
**          9/19/11           dcanuso                       WI 5160 BOE User Role History Table Added
**                                                          Developer asked for changes in Output
**          7/26/13           dcanuso                       Adding Subcontractor Author
*******************************************************************************/
SET NOCOUNT ON 

SELECT 
IJ.FieldID,
IJ.[OldValue] AS [OldValue],
IJ.[NewValue] AS [NewValue],
IJ.[ETIUserID] AS [ETIUserID],
IJ.UpdateDT,
IJ.IsSubcontractor AS [IsSubcontractor] 
FROM
      (
      SELECT 
      [FieldID] AS [FieldID],
      [CurrentComment] AS [OldValue],
    [UpdatedComment] AS [NewValue],
    [ChangedByETIUserID] AS [ETIUserID],
    [UpdateDT] AS [UpdateDT],
    -1 AS [IsSubcontractor] /*NA in this section*/
      FROM [dbo].[BOECommentHistory]
      WHERE BOEID = @BOEID
      UNION
      SELECT
      SH.[FieldID] AS [FieldID],
      OS.[BOEState] AS [OldValue],
      NS.[BOEState] AS [NewValue],
      SH.[ChangedByETIUserID] AS [ETIUserID],
      SH.[UpdateDT] AS [UpdateDT],
      -1 AS [IsSubcontractor] /*NA in this section*/
      FROM [dbo].[BOEStateHistory] SH
            LEFT OUTER JOIN dbo.BOEStateLU OS ON SH.CurrentBOEStateID = OS.BOEStateID
            LEFT OUTER JOIN dbo.BOEStateLU NS ON SH.UpdatedBOEStateID = NS.BOEStateID 
      WHERE SH.BOEID = @BOEID
      UNION
      SELECT 
      5 AS [FieldID], /*Approver's Response*/
      '' AS [OldValue], /*No Old Value in Approver's Response*/
      [Approval] AS [NewValue],
    [ApprovalETIUserID] AS [ETIUserID],
    [UpdateDT] AS [UpdateDT],
    -1 AS [IsSubcontractor] /*NA in this section*/
      FROM [dbo].[BOEApprovalHistory]
      WHERE BOEID = @BOEID
      /*WI 5160*/
      UNION
      
      SELECT 
      BUR.[FieldID] AS [FieldID], 
      CAST(BUR.[CurrentETIUserID] AS varchar(10)) AS [OldValue], 
      CAST(BUR.[UpdatedETIUserID] AS varchar(10)) AS [NewValue],
    BUR.[ChangedByETIUserID] AS [ETIUserID],
    BUR.[UpdateDT] AS [UpdateDT],
    CASE 
            WHEN EXISTS (
                              SELECT * 
                              FROM dbo.BOEPotentialRole 
                              WHERE ETIUserID = BUR.UpdatedETIUserID AND
                              RoleID = 9
                              ) THEN 1
      ELSE 0
      END AS [IsSubcontractor]
      FROM [dbo].[BOEUserRoleHistory] BUR
      WHERE BOEID = @BOEID AND BUR.RoleID IN (1,9) /*Author/Subcontractor Changes*/ and FieldID = 7 /*Author*/
      
      UNION
      SELECT 
      [FieldID] AS [FieldID], 
      CAST([CurrentETIUserID] AS varchar(10)) AS [OldValue], 
      CAST([UpdatedETIUserID] AS varchar(10)) AS [NewValue],
    [ChangedByETIUserID] AS [ETIUserID],
    [UpdateDT] AS [UpdateDT],
    -1 AS [IsSubcontractor] /*NA in this section*/
      FROM [dbo].[BOEUserRoleHistory]
      WHERE BOEID = @BOEID AND RoleID = 3 /*Approver Changes*/ and FieldID = 8 /*Approver*/  
      ) IJ 
--INNER JOIN dbo.ETIuser U ON IJ.ETIUserID = U.ETIUserID
--INNER JOIN dbo.FieldLU F ON IJ.FieldID = F.FieldID
ORDER BY IJ.UpdateDT DESC

GO