using System.Text.RegularExpressions;

namespace AtomUI.Desktop.Controls;

public class FormUrlValidator : AbstractFormValidator
{
    private static readonly Regex UrlRegex = new Regex(
        @"^(https?|ftp)://" +                              // 协议
        @"(([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,})" +              // 域名（支持多级）
        @"(:\d+)?" +                                        // 可选端口
        @"(/[^\s?#]*)?" +                                   // 可选路径
        @"(\?[^\s#]*)?" +                                   // 可选查询参数
        @"(#[^\s]*)?" +                                     // 可选片段
        @"$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));
    
    protected override async Task<bool> NotifyValidateAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        var isValid  = true;
        var strValue = value as string;
        if (string.IsNullOrWhiteSpace(strValue))
        {
            isValid = false;
        }
        else
        {
            try
            {
                isValid = UrlRegex.IsMatch(strValue);
            }
            catch (RegexMatchTimeoutException)
            {
                isValid = false;
            }
        }
        
        return await Task.FromResult(isValid);
    }
}