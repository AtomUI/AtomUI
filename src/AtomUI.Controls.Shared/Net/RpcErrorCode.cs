namespace AtomUI.Controls;

public enum RpcErrorCode
{
    Success = 0,
    Unknown,
    
    InvalidRequest, // 参数校验失败等
    Unauthorized,   // 身份认证失败
    Forbidden,      // 权限不足
    NotFound,       // 请求的资源或端点不存在
    
    NetworkFailure, // 网络不通（连接拒绝、DNS失败等）
    Timeout,        // 请求超时
    
    ServerInternal, // 服务器内部错误
    ServiceUnavailable, // 服务不可用（如熔断、维护）
    
    Cancelled       // 操作被取消
}