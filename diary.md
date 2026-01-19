# Development Diary

## 2026-01-19

### SAP Connection Field Fix for Workspace Copy

**Commit:** `27139a303` - Enable SAP Connection field when MOQ Template BOEs is Yes during workspace copy

**Issue:** The "SAP Connection Enabled" dropdown on the Create Workspace page was being disabled whenever a user was copying an existing workspace, regardless of whether "MOQ Template BOEs" was set to Yes or No.

**Fix:** Modified the `ng-disabled` condition in both workspace creation templates to only disable the SAP Connection field when MOQ Template BOEs is set to "No", allowing users to modify this setting during workspace copy when MOQ Template BOEs is "Yes".

**Files Changed:**
- `GenBOE/GenBOE.Web/Resources/CreateWorkspaceStep3RMS.html` (line 260)
- `GenBOE/GenBOE.Web/Resources/CreateWorkspaceStep3Space.html` (line 350)

**Before:**
```html
data-ng-disabled="data.WorkspaceToCopyID > 0 || !data.UsingTemplateBoe"
```

**After:**
```html
data-ng-disabled="!data.UsingTemplateBoe"
```

**Behavior Change:**
| Scenario | MOQ Template BOEs | SAP Connection Enabled |
|----------|-------------------|------------------------|
| New workspace | Yes | Enabled |
| New workspace | No | Disabled |
| Copy workspace | Yes | Enabled (was disabled) |
| Copy workspace | No | Disabled |
