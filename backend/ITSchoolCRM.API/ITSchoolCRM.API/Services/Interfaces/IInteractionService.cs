using ITSchoolCRM.API.DTOs.Interactions;
using ITSchoolCRM.API.DTOs.InteractionHistory;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IInteractionService
{
    Task<List<InteractionDto>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<List<InteractionDto>> GetByUniversityIdAsync(
        int universityId,
        CancellationToken cancellationToken);

    Task<InteractionDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<InteractionDto?> CreateAsync(
        CreateInteractionDto dto,
        CancellationToken cancellationToken);

    Task<InteractionDto?> UpdateAsync(
        int id,
        UpdateInteractionDto dto,
        CancellationToken cancellationToken);

    Task<bool> ChangeStatusAsync(
        int interactionId,
        ChangeInteractionStatusDto dto,
        CancellationToken cancellationToken);

    Task<bool> AddCommentAsync(
        int interactionId,
        AddInteractionCommentDto dto,
        CancellationToken cancellationToken);

    Task<List<InteractionStatusHistoryDto>> GetHistoryAsync(
        int interactionId,
        CancellationToken cancellationToken);
}