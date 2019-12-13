-- This file lists items where a workspace is referencing a custom field value that belongs to a different workspace (because CF belongs to a different WS)

-- issues via BOE
SELECT b.WorkspaceID AS WsWithIssue, cF.WorkspaceID AS WsThatOwnsCF, cf.CustomFieldName, cV.CustomFieldValueDescription, x.BCFVID
	FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
		INNER JOIN BOECustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
		INNER JOIN BOE b ON b.BOEID = x.BOEID
	WHERE cF.WorkspaceID <> b.WorkspaceID
	ORDER BY WsWithIssue, WsThatOwnsCF, cf.CustomFieldName;

-- issues via Task
SELECT b.WorkspaceID AS WsWithIssue, cF.WorkspaceID AS WsThatOwnsCF, cf.CustomFieldName, cV.CustomFieldValueDescription, x.BTECFVID
	FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
		INNER JOIN BOETaskElementCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
		INNER JOIN BOETaskElement bT ON bT.BOETaskElementID = x.BOETaskElementID
		INNER JOIN BOE b ON b.BOEID = bT.BOEID
	WHERE cF.WorkspaceID <> b.WorkspaceID
	ORDER BY WsWithIssue, WsThatOwnsCF, cf.CustomFieldName;

-- issues via Labor Elements
SELECT b.WorkspaceID AS WsWithIssue, cF.WorkspaceID AS WsThatOwnsCF, cf.CustomFieldName, cV.CustomFieldValueDescription, x.BLTCFVID
	FROM CustomField cF INNER JOIN CustomFieldValue cV ON cF.CustomFieldID = cV.CustomFieldID
		INNER JOIN BOELaborTypeCustomFieldValueXREF x ON cV.CustomFieldValueID = x.CustomFieldValueID
		INNER JOIN BOELaborType bL ON x.BOELaborTypeID = bL.BOELaborTypeID
		INNER JOIN BOETaskElement bT ON bT.BOETaskElementID = bL.BOETaskElementID
		INNER JOIN BOE b ON b.BOEID = bT.BOEID
	WHERE cF.WorkspaceID <> b.WorkspaceID
	ORDER BY WsWithIssue, WsThatOwnsCF, cf.CustomFieldName;
