/*
    This file finds the issue when BRCs are linked to the wrong Workspace's Resources inside BOELaborType table.
*/
select lt.boelabortypeid, lt.brcresourceid, b.workspaceid, w.workspaceid
  FROM [dbo].boelabortype lt 
  INNER JOIN boetaskelement t ON t.BOETaskElementID = lt.BOETaskElementID
  INNER JOIN BOE b on t.BOEID = b.BOEID
  INNER JOIN Resource r on r.ResourceID = lt.BRCResourceID
  INNER JOIN Workspace w on w.ResourceListID = r.ResourceListID
where w.workspaceid != b.workspaceid
order by lt.boelabortypeid asc