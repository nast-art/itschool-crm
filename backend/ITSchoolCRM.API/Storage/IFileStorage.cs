using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Storage;

/// <summary>
/// Абстракция файлового хранилища вложений.
/// </summary>
/// <remarks>
/// ТЕКУЩАЯ РЕАЛИЗАЦИЯ: Yandex Cloud S3. Замена реализации —
/// одна строка в Program.cs, сервисы, контроллеры, кэш и схема БД не меняются.
///
/// СОГЛАШЕНИЕ О КЛЮЧАХ: ключ — это относительный путь, уникальный в пределах
/// бакета:
///   "uploads/attachments/{interactionId}/{statusId}/{guid}_{name}"
/// Такой формат допускается ключами S3 без изменений; при смене провайдера
/// существующие storage_path из БД остаются валидными ключами бакета
/// (S3 не требует ведущего слэша).
///
/// РЕГИСТРАЦИЯ: AddSingleton — реализация без состояния, держит только
/// конфигурацию (клиент S3).
/// </remarks>
public interface IFileStorage
{
    /// <summary>
    /// Сохраняет файл под заданным ключом.
    /// Возвращает ключ — его значение записывается в attachment.storage_path.
    /// </summary>
    Task<string> SaveAsync(Stream content, string key, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Открывает файл на чтение. null — ключ не найден.
    /// Вызывающий код обязан dispose возвращённый поток
    /// (using / await using в контроллере).
    /// </summary>
    Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Удаляет файл по ключу. Отсутствие файла — НЕ ошибка
    /// (запись в БД удаляется в любом случае; «осиротевший» файл
    /// на стороне хранилища не должен блокировать удаление метаданных).
    /// </summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Проверка доступности хранилища (для health-check).
    /// </summary>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken);
}