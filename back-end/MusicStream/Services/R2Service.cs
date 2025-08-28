using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MusicStream.Services
{
    public class R2Service
    {
        private readonly IAmazonS3 _client;
        private readonly string _bucket;
        private readonly string _accountId;
        private readonly ILogger<R2Service> _logger;
        private readonly string _publicUrlBase; // Add this for configurable public URL

        public R2Service(IConfiguration config, ILogger<R2Service> logger)
        {
            _logger = logger;
            _accountId = config["CloudflareR2:AccountId"] ?? throw new ArgumentNullException(nameof(config), "CloudflareR2:AccountId is required");
            _bucket = config["CloudflareR2:BucketName"] ?? throw new ArgumentNullException(nameof(config), "CloudflareR2:BucketName is required");
            var accessKey = config["CloudflareR2:AccessKey"] ?? throw new ArgumentNullException(nameof(config), "CloudflareR2:AccessKey is required");
            var secretKey = config["CloudflareR2:SecretKey"] ?? throw new ArgumentNullException(nameof(config), "CloudflareR2:SecretKey is required");

            // Get public URL base from config, or use default
            // You MUST set this in appsettings.json after enabling public access in Cloudflare
            _publicUrlBase = config["CloudflareR2:PublicUrl"] ?? $"https://{_bucket}.{_accountId}.r2.cloudflarestorage.com";

            if (string.IsNullOrEmpty(_accountId) || string.IsNullOrEmpty(_bucket) ||
                string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Cloudflare R2 configuration is incomplete. Please check appsettings.json");
            }

            var s3config = new AmazonS3Config
            {
                ServiceURL = $"https://{_accountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true
            };

            _client = new AmazonS3Client(accessKey, secretKey, s3config);
            _logger.LogInformation("R2Service initialized with bucket: {Bucket}, account: {AccountId}, publicUrl: {PublicUrl}",
                _bucket, _accountId, _publicUrlBase);
        }

        public async Task<string> UploadMusicAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty", nameof(file));
            }

            try
            {
                // Sanitize filename
                var sanitizedFileName = Path.GetFileName(file.FileName).Replace(" ", "_");
                var key = $"{Guid.NewGuid()}_{sanitizedFileName}";

                _logger.LogInformation("Uploading file: {FileName}, Size: {Size} bytes, Key: {Key}",
                    file.FileName, file.Length, key);

                // Read entire file into memory to avoid streaming signature issues with R2
                byte[] fileBytes;
                using (var sourceStream = file.OpenReadStream())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await sourceStream.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }

                // Determine content type
                var contentType = file.ContentType;
                if (string.IsNullOrEmpty(contentType))
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    contentType = extension switch
                    {
                        ".mp3" => "audio/mpeg",
                        ".wav" => "audio/wav",
                        ".m4a" => "audio/mp4",
                        ".flac" => "audio/flac",
                        ".ogg" => "audio/ogg",
                        _ => "application/octet-stream"
                    };
                }

                using (var memoryStream = new MemoryStream(fileBytes))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = _bucket,
                        Key = key,
                        InputStream = memoryStream,
                        ContentType = contentType,
                        ServerSideEncryptionMethod = ServerSideEncryptionMethod.None
                    };

                    request.Headers.ContentLength = fileBytes.Length;
                    request.DisablePayloadSigning = true;

                    var response = await _client.PutObjectAsync(request);

                    _logger.LogInformation("File uploaded successfully. ETag: {ETag}, Key: {Key}",
                        response.ETag, key);

                    // Build public URL
                    var publicUrl = $"{_publicUrlBase.TrimEnd('/')}/{key}";
                    _logger.LogInformation("Public URL: {Url}", publicUrl);

                    return publicUrl;
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "AWS S3 Error uploading file: {FileName}. ErrorCode: {ErrorCode}, Message: {Message}",
                    file.FileName, ex.ErrorCode, ex.Message);
                throw new Exception($"Failed to upload file to R2: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading file: {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty", nameof(file));
            }

            try
            {
                // Sanitize filename
                var sanitizedFileName = Path.GetFileName(file.FileName).Replace(" ", "_");
                var key = $"images/{Guid.NewGuid()}_{sanitizedFileName}";

                _logger.LogInformation("Uploading image: {FileName}, Size: {Size} bytes, Key: {Key}",
                    file.FileName, file.Length, key);

                // Read entire file into memory
                byte[] fileBytes;
                using (var sourceStream = file.OpenReadStream())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await sourceStream.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }

                // Determine content type
                var contentType = file.ContentType;
                if (string.IsNullOrEmpty(contentType))
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    contentType = extension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        _ => "application/octet-stream"
                    };
                }

                using (var memoryStream = new MemoryStream(fileBytes))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = _bucket,
                        Key = key,
                        InputStream = memoryStream,
                        ContentType = contentType,
                        ServerSideEncryptionMethod = ServerSideEncryptionMethod.None
                    };

                    request.Headers.ContentLength = fileBytes.Length;
                    request.DisablePayloadSigning = true;

                    var response = await _client.PutObjectAsync(request);

                    _logger.LogInformation("Image uploaded successfully. ETag: {ETag}, Key: {Key}",
                        response.ETag, key);

                    // Build public URL
                    var publicUrl = $"{_publicUrlBase.TrimEnd('/')}/{key}";
                    _logger.LogInformation("Public URL: {Url}", publicUrl);

                    return publicUrl;
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "AWS S3 Error uploading image: {FileName}. ErrorCode: {ErrorCode}, Message: {Message}",
                    file.FileName, ex.ErrorCode, ex.Message);
                throw new Exception($"Failed to upload image to R2: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading image: {FileName}", file.FileName);
                throw;
            }
        }
    }
}