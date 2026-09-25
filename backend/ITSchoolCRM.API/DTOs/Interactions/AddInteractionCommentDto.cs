namespace ITSchoolCRM.API.DTOs.InteractionHistory;

// Комментарий без смены статуса: падает в interaction_status_history
// с from_status_id = to_status_id = текущий статус.
public class AddInteractionCommentDto
{
    public string Comment { get; set; } = string.Empty;

    // Необязательная привязка к этапу (для отображения в ленте)
    public int? StatusId { get; set; }
}