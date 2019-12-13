using System.Collections.ObjectModel;
using GenBOE.Dtos;

namespace GenBOE.DataBridge.DTO
{
    public interface IWorkspaceHistoryDTODataLoader
    {
        Collection<WorkspaceHistoryDTO> GetWorkspaceHistory(int inWorkspaceID);
    }
}
