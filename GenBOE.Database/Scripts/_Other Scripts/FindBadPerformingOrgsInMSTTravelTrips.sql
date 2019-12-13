select tt.msttraveltripid, tt.[PerformingOrganizationID], b.workspaceid, w.workspaceid
  FROM [dbo].[MSTTravelTrip] tt
  INNER JOIN TravelTripTaskElement t ON t.TravelTripTaskElementID = tt.TravelTripTaskElementID
  INNER JOIN BOE b on t.BOEID = b.BOEID
  INNER JOIN PerformingOrganization po on po.performingorganizationid = tt.[PerformingOrganizationID]
  INNER JOIN Workspace w on w.PerformingOrganizationListID = po.PerformingOrganizationListID
where w.workspaceid != b.workspaceid
order by tt.MSTTravelTripID asc
