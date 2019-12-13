namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class BoeFormClinContractTypeDTO
    {
        public int ClinId { get; set; }
        public int ContractType { get; set; }
    }
}
