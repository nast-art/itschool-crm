namespace ITSchoolCRM.API.DTOs.Interactions;

// Редактирование взаимодействия (кнопка «Редактировать», admin).
// Статус через этот метод НЕ меняется — только через ChangeStatusAsync.
public class UpdateInteractionDto
{
    public int? UniversityId { get; set; }

    public int? ProgramId { get; set; }

    public int? ProductId { get; set; }

    public int? ManagerId { get; set; }

    public int? UniversityContactId { get; set; }
}