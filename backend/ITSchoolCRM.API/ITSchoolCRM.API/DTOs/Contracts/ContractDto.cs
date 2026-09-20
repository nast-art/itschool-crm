namespace ITSchoolCRM.API.DTOs.Contracts;

public class ContractDto
{
    public int Id { get; set; }

    public string? ContractNumber { get; set; }

    public DateTime? SignedAt { get; set; }

    public string? Status { get; set; }

    public string? Comment { get; set; }
}