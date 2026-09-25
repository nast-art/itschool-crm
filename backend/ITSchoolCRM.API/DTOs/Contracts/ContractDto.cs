namespace ITSchoolCRM.API.DTOs.Contracts;

public class ContractDto
{
    public int Id { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? SignedAt { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Status { get; set; }
    public string? Comment { get; set; }
    public List<int> UniversityIds { get; set; } = new();
    public List<string> UniversityNames { get; set; } = new();
    public List<int> ProductIds { get; set; } = new();
    public List<string> ProductNames { get; set; } = new();
}