namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;

    public interface IPerformingOrgListDTODataLoader
    {
        PerformingOrgListDTO GetPerfOrgList(int inPerfOrgListID);
        int SavePerformingOrgList(PerformingOrgListDTO inPerformingOrgList);
        void ClearPerformingOrgList(PerformingOrgListDTO inPerformingOrgList);
    }
}
