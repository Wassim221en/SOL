namespace SOL.Application.Common.Interfaces;

public class FcmBatchResponse
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> FailedTokens { get; set; } = new();
    public List<string> ErrorMessages { get; set; } = new();
}

public interface IFcmService
{
    Task<bool> SendNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string?>? data = null,
        CancellationToken cancellationToken = default);

    Task<FcmBatchResponse> SendNotificationToMultipleDevicesAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string?>? data = null,
        CancellationToken cancellationToken = default);

    Task<bool> SendNotificationToTopicAsync(
        string topic,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);

    Task<bool> SubscribeToTopicAsync(string deviceToken, string topic, CancellationToken cancellationToken = default);
    Task<FcmBatchResponse> SubscribeToTopicAsync(List<string> deviceTokens, string topic, CancellationToken cancellationToken = default);
    Task<bool> UnsubscribeFromTopicAsync(string deviceToken, string topic, CancellationToken cancellationToken = default);
    Task<FcmBatchResponse> UnsubscribeFromTopicAsync(List<string> deviceTokens, string topic, CancellationToken cancellationToken = default);

    Task<bool> SendDataMessageAsync(string deviceToken, Dictionary<string, string> data, CancellationToken cancellationToken = default);
    Task<bool> ValidateDeviceTokenAsync(string deviceToken, CancellationToken cancellationToken = default);
}
