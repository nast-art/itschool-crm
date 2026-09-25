using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Storage
{
    /// <summary>
    /// Абстракция файлового хранилища вложений.
    ///
    /// ТЕКУЩАЯ РЕАЛИЗАЦИЯ: DiskFileStorage (локальный диск).
    /// ПЕРЕЕЗД НА S3: добавляется S3FileStorage, регистрация
    /// в Program.cs меняется на одну строку — сервисы,
    /// контроллеры, кэш и схема БД не меняются.
    ///
    /// СОГЛАШЕНИЕ О КЛЮЧАХ: ключ — это относительный путь,
    /// уникальный в пределах хранилища:
    ///   диск:  "uploads/attachments/{interactionId}/{statusId}/{guid}_{name}"
    ///   S3:    тот же формат — ключи S3 допускают такие строки
    ///          без изменений, что даёт БЕСШОВНУЮ МИГРАЦИЮ:
    ///          существующие storage_path из БД остаются валидными
    ///          ключами бакета (S3 не требует ведущего слэша).
    ///
    /// РЕГИСТРАЦИЯ: AddSingleton — реализация без состояния,
    /// держит только конфигурацию (корневой путь / клиент S3).
    /// </summary>
    public interface IFileStorage
    {
        /// <summary>
        /// Сохраняет файл под заданным ключом.
        /// Возвращает ключ — его значение записывается
        /// в attachment.storage_path.
        /// </summary>
        Task<string> SaveAsync(
            Stream content,
            string key,
            string contentType,
            CancellationToken cancellationToken);

        /// <summary>
        /// Открывает файл на чтение. null — ключ не найден.
        /// Вызывающий код обязан dispose возвращённый поток
        /// (using / await using в контроллере).
        /// </summary>
        Task<Stream?> OpenReadAsync(
            string key,
            CancellationToken cancellationToken);

        /// <summary>
        /// Удаляет файл по ключу. Отсутствие файла — НЕ ошибка
        /// (запись в БД удаляется в любом случае; «осиротевший»
        /// файл на стороне хранилища не должен блокировать
        /// операцию удаления метаданных).
        /// </summary>
        Task DeleteAsync(
            string key,
            CancellationToken cancellationToken);

        /// <summary>
        /// Проверка доступности хранилища (для health-check).
        /// </summary>
        Task<bool> IsHealthyAsync(
            CancellationToken cancellationToken);
    }
}