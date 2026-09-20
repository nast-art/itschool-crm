namespace ITSchoolCRM.API.DTOs.Licenses;

public class LicenseDto
{
    public int Id { get; set; }

    public DateTime? SignedAt { get; set; }

    public DateTime? ValidUntil { get; set; }

    public string? TransferStatus { get; set; }

    public string? Comment { get; set; }
}