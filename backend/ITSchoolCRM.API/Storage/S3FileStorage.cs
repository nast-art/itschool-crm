using Amazon.S3;
using Amazon.S3.Model;

namespace ITSchoolCRM.API.Storage;

/// <summary>
/// Хранение вложений в S3-совместимом объектном хранилище
/// (рекомендация заказчика 16.09.2026 13:10).
///
/// КЛЮЧИ: тот же формат, что у DiskFileStorage
/// ("uploads/attachments/{interactionId}/{statusId}/{guid}_{name}") —
/// существующие storage_path из БД остаются валидными ключами
/// бакета без миграции данных (S3 допускает такие ключи;
/// ведущий слэш не требуется).
///
/// ПЕРЕКЛЮЧЕНИЕ: одна строка в Program.cs
/// (AddSingleton<IFileStorage, S3FileStorage>()).
/// </summary>
public sealed class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _client;
    private readonly string _bucket;

    public S3FileStorage(IConfiguration configuration)
    {
        var section = configuration.GetSection("Storage:S3");

        _bucket = section["Bucket"]
            ?? throw new InvalidOperationException(
                "Не задан Storage:S3:Bucket.");

        var config = new AmazonS3Config
        {
            ServiceURL = section["ServiceUrl"],   // для S3-совместимых (MinIO, VK Cloud, Yandex)
            AuthenticationRegion = section["Region"],
            ForcePathStyle = section.GetValue<bool>("ForcePathStyle"), // true для MinIO
        };

        _client = new AmazonS3Client(
            section["AccessKey"],
            section["SecretKey"],
            config);
    }

    public async Task<string> SaveAsync(
        Stream content,
        string key,
        string contentType,
        CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
        };

        await _client.PutObjectAsync(
            request,
            cancellationToken);

        return key;
    }

    public async Task<Stream?> OpenReadAsync(
        string key,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetObjectAsync(
                _bucket,
                key,
                cancellationToken);

            // ResponseStream живёт, пока жив response —
            // оборачиваем, чтобы dispose потока закрывал и ответ
            return new S3ResponseStream(response);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(
        string key,
        CancellationToken cancellationToken)
    {
        // NoSuchKey — не ошибка: метаданные из БД удаляются
        // в любом случае
        try
        {
            await _client.DeleteObjectAsync(
                _bucket,
                key,
                cancellationToken);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
        }
    }

    public async Task<bool> IsHealthyAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            // Лёгкая операция: проверяем доступность бакета
            await _client.ListObjectsV2Async(
                new ListObjectsV2Request
                {
                    BucketName = _bucket,
                    MaxKeys = 1,
                },
                cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Обертка над ResponseStream: dispose потока освобождает
    /// и HTTP-ответ S3 (иначе — утечка соединений).
    /// Dispose ИДЕМПОТЕНТЕН: ASP.NET вызывает DisposeAsync -> Close ->
    /// Dispose по цепочке, повторные вызовы — норма, не ошибка.
    /// </summary>
    private sealed class S3ResponseStream : Stream
    {
        private readonly GetObjectResponse _response;
        private bool _disposed;

        public S3ResponseStream(GetObjectResponse response)
        {
            _response = response;
        }

        public override bool CanRead => !_disposed;
        public override bool CanSeek => !_disposed && _response.ResponseStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _response.ContentLength;

        public override long Position
        {
            get => _response.ResponseStream.Position;
            set => _response.ResponseStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
            => _response.ResponseStream.Read(buffer, offset, count);

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
            => _response.ResponseStream.ReadAsync(buffer, offset, count, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin)
            => _response.ResponseStream.Seek(offset, origin);

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override void Flush()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;

                // Null-условный доступ: ResponseStream у AWSSDK
                // иногда уже освобождён к моменту нашего dispose
                _response.ResponseStream?.Dispose();
                _response.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}