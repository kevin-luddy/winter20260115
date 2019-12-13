/* SQL Queries for the Bulk Archive Manual Test. */

use GenTRAC
GO

PRINT 'Find count of "In Progress and Completed" Proposals with Dates, Product Line, and LOB specified.'
select count(*) As [Total In-Progress And Completed Proposals] from Proposal where DateCreated >= '8/15/2013' and DateCreated <= '8/16/2013' and ProductLineId = '2' and
	LineOfBusinessID ='10' and (ProposalStatusID = 1 or ProposalStatusID = 2); 

PRINT 'Find count of "Archived" Proposals with Dates, Product Line, and LOB specified.'
select count(*) As [Total Archived Proposals] from Proposal where DateCreated >= '5/5/2013' and DateCreated <= '8/31/2013' and ProductLineId = '2' and
	LineOfBusinessID ='10' and ProposalStatusID = 3;

PRINT 'Find count of "Archived Proposals" for *ALL* Product Lines and LOBs between 2 dates.'
select count(*) As [Total Archived Proposals] from Proposal where DateCreated >= '8/15/2013' and DateCreated <= '8/16/2013' and ProposalStatusID = 3;

PRINT 'Find count of "Archived Proposals" for the "Defense" Product Line and all of its LOBs for all dates.'
select count(*) As [Total Archived Proposals] from Proposal where ProductLineId = '2' and ProposalStatusID = 3;

PRINT 'Find count of "In Progress" or "Complete" Proposals for the "Civil" Product Line and *ALL* of its LOBs between 2 dates.'
select count(*)  As [Total In-Progress And Completed Proposals] from Proposal where DateCreated >= '8/15/2013' and DateCreated <= '8/16/2013' 
	and ProductLineId = '1' and (ProposalStatusID = 1 or ProposalStatusID = 2); 

/* Reference */

/* Proposal Status: 
*  1 - In Progress, 2 - Completed, 3 - Archived
*/

/* Product Line:
*  1 - Civil, 2 - Defense, 3 - National
*/

-- Find LOB Name/ID pairings when it's necessary to look up a LOB ID 
select distinct [LineOfBusinessID], [LineOfBusinessName] from LineOfBusiness;

-- Find Product Line Name/ID pairings when it's necessary to look up a ProductLine ID 
select distinct [ProductLineID], [ProductLineName] from ProductLine;

GO