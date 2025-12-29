namespace AtomUI.Desktop.Controls;

public enum TreeNodeLoadErrorCode
{
    None = 0,           // 无错误
    NetworkError,       // 网络连接/超时问题
    ServerError,        // 服务器5xx错误
    ClientError,        // 服务器4xx错误（如认证失败、参数错误）
    Cancelled,          // 操作被取消
    Unknown             // 未知错误
}