namespace AtomUI.Controls;

public enum FileUploadErrorCode
{
    None = 0,           // 无错误
    NetworkError,       // 网络连接/超时问题
    ServerError,        // 服务器5xx错误
    ClientError,        // 服务器4xx错误（如认证失败、参数错误）
    FileError,          // 本地文件错误（找不到、无权限）
    ValidationFailed,   // 验证失败（如文件类型、大小不符）
    Cancelled,          // 操作被取消
    Unknown             // 未知错误
}

public enum FileUploadStatus
{
    Pending,
    Uploading,
    Success,
    Failed,
    Cancelled
}

public record FileUploadResult
{
    /// <summary>
    /// 上传的最终状态
    /// </summary>
    public FileUploadStatus Status { get; init; } = FileUploadStatus.Pending;
    
    /// <summary>
    /// 是否完全成功（Status == FileUploadStatus.Success 的便捷属性）
    /// </summary>
    public bool IsSuccess => Status == FileUploadStatus.Success;
    
    /// <summary>
    /// 分类的错误码
    /// </summary>
    public FileUploadErrorCode ErrorCode { get; init; } = FileUploadErrorCode.None;
    
    /// <summary>
    /// 可读的错误消息（适合显示给用户）
    /// </summary>
    public string? UserFriendlyMessage { get; init; }
    
    /// <summary>
    /// 详细的内部错误信息（用于调试和日志）
    /// </summary>
    public string? InternalErrorMessage { get; init; }
    
    /// <summary>
    /// 关联的异常（如果有）
    /// </summary>
    public Exception? Exception { get; init; }
    
    /// <summary>
    /// 文件在服务器上的最终访问地址（HTTP/HTTPS URL）
    /// </summary>
    public Uri? RemoteUrl { get; init; }
    
    /// <summary>
    /// 服务器返回的唯一文件标识（可能用于后续删除、查询等操作）
    /// </summary>
    public string? FileId { get; init; }
    
    /// <summary>
    /// 服务器返回的ETag或文件哈希（用于完整性校验）
    /// </summary>
    public string? FileHash { get; init; }

    // ---------- 性能与元数据 ----------
    /// <summary>
    /// 文件总大小（字节）
    /// </summary>
    public ulong FileSize { get; init; }
    
    /// <summary>
    /// 实际上传耗时
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }
    
    /// <summary>
    /// 平均上传速度（字节/秒）
    /// </summary>
    public double AverageSpeed => FileSize > 0 && ElapsedTime.TotalSeconds > 0 
        ? FileSize / ElapsedTime.TotalSeconds : 0;
    
    /// <summary>
    /// 用于断点续传的会话标识
    /// </summary>
    public string? ResumableSessionId { get; init; }
    
    /// <summary>
    /// 已成功上传的字节数（对于分片上传）
    /// </summary>
    public long BytesUploaded { get; init; }
    
    /// <summary>
    /// 服务器返回的自定义元数据（如：图片宽度、高度、时长等）
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
    
    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static FileUploadResult SuccessResult(
        Uri remoteUrl, 
        ulong fileSize, 
        TimeSpan elapsedTime,
        string? fileId = null,
        string? fileHash = null,
        Dictionary<string, object>? metadata = null)
    {
        return new FileUploadResult
        {
            Status = FileUploadStatus.Success,
            RemoteUrl = remoteUrl,
            FileSize = fileSize,
            ElapsedTime = elapsedTime,
            FileId = fileId,
            FileHash = fileHash,
            Metadata = metadata
        };
    }
    
    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static FileUploadResult FailureResult(
        FileUploadErrorCode errorCode,
        string userFriendlyMessage,
        string? internalMessage = null,
        Exception? exception = null)
    {
        return new FileUploadResult
        {
            Status = FileUploadStatus.Failed,
            ErrorCode = errorCode,
            UserFriendlyMessage = userFriendlyMessage,
            InternalErrorMessage = internalMessage,
            Exception = exception
        };
    }
    
    /// <summary>
    /// 创建取消结果
    /// </summary>
    public static FileUploadResult CancelledResult(string userFriendlyMessage)
    {
        return new FileUploadResult
        {
            Status = FileUploadStatus.Cancelled,
            ErrorCode = FileUploadErrorCode.Cancelled,
            UserFriendlyMessage = userFriendlyMessage
        };
    }
}