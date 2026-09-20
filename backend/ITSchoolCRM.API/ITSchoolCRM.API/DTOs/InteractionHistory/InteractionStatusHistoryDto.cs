using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.InteractionHistory
{
    public class InteractionStatusHistoryDto
    {
    public int Id { get; set; }

    public int? InteractionId { get; set; }

    public int? FromStatusId { get; set; }

    public string? FromStatusName { get; set; }

    public int? ToStatusId { get; set; }

    public string? ToStatusName { get; set; }

    public int? ChangedBy { get; set; }

    public string? ChangedByName { get; set; }

    public string? Comment { get; set; }

    public DateTime? ChangedAt { get; set; }
    }
}