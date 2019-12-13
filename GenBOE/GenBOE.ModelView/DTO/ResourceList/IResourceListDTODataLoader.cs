namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;

    public interface IResourceListDTODataLoader
    {
        ResourceListDTO GetResourceList(int inResourceListID);
        int SaveResourceList(ResourceListDTO inResourceList);
        void ClearResourceList(ResourceListDTO inResourceList);
    }
}
