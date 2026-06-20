using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SOL.Application.Common.Interfaces;

namespace Template.Infrastructe.Services;

/// <summary>
/// Firebase Cloud Messaging (FCM) Service Implementation using Firebase Admin SDK
/// </summary>
public class FcmService : IFcmService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FcmService> _logger;
    private readonly FirebaseMessaging _messaging;
    private readonly string _projectId;

    public FcmService(
        IConfiguration configuration,
        ILogger<FcmService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _projectId = _configuration["FCM:ProjectId"] ?? throw new ArgumentNullException("FCM:ProjectId");
        var serviceAccountPath = _configuration["FCM:ServiceAccountPath"] ?? throw new ArgumentNullException("FCM:ServiceAccountPath");

        // Initialize Firebase App if not already initialized
        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                var credential = GoogleCredential.FromFile(serviceAccountPath);
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = _projectId
                });

                _logger.LogInformation("Firebase Admin SDK initialized successfully for project: {ProjectId}", _projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Admin SDK");
                throw;
            }
        }

        _messaging = FirebaseMessaging.DefaultInstance;
    }

    /// <summary>
    /// Send notification to a single device
    /// </summary>
    public async Task<bool> SendNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string?>? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ChannelId = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var response = await _messaging.SendAsync(message, cancellationToken);

            _logger.LogInformation(
                "FCM notification sent successfully to device: {DeviceToken}. Message ID: {MessageId}",
                MaskToken(deviceToken),
                response);

            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(
                ex,
                "Firebase error sending notification to device: {DeviceToken}. Error Code: {ErrorCode}",
                MaskToken(deviceToken),
                ex.MessagingErrorCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending FCM notification to device: {DeviceToken}",
                MaskToken(deviceToken));
            return false;
        }
    }

    /// <summary>
    /// Send notification to multiple devices
    /// </summary>
    public async Task<FcmBatchResponse> SendNotificationToMultipleDevicesAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string?>? data = null,
        CancellationToken cancellationToken = default)
    {
        var response = new FcmBatchResponse();

        if (deviceTokens == null || !deviceTokens.Any())
        {
            _logger.LogWarning("No device tokens provided for batch notification");
            return response;
        }

        try
        {
            var message = new MulticastMessage
            {
                Tokens = deviceTokens,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ChannelId = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var batchResponse = await _messaging.SendEachForMulticastAsync(message, cancellationToken);

            response.SuccessCount = batchResponse.SuccessCount;
            response.FailureCount = batchResponse.FailureCount;

            // Identify failed tokens
            for (int i = 0; i < batchResponse.Responses.Count; i++)
            {
                if (!batchResponse.Responses[i].IsSuccess)
                {
                    response.FailedTokens.Add(deviceTokens[i]);
                    response.ErrorMessages.Add(batchResponse.Responses[i].Exception?.Message ?? "Unknown error");
                }
            }

            _logger.LogInformation(
                "FCM batch notification sent. Success: {SuccessCount}, Failure: {FailureCount}",
                response.SuccessCount,
                response.FailureCount);
        }
        catch (Exception ex)
        {
            response.FailureCount = deviceTokens.Count;
            response.FailedTokens = deviceTokens;

            _logger.LogError(
                ex,
                "Error sending FCM batch notification to {Count} devices",
                deviceTokens.Count);
        }

        return response;
    }

    /// <summary>
    /// Send notification to a topic
    /// </summary>
    public async Task<bool> SendNotificationToTopicAsync(
        string topic,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ChannelId = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var response = await _messaging.SendAsync(message, cancellationToken);

            _logger.LogInformation(
                "FCM notification sent successfully to topic: {Topic}. Message ID: {MessageId}",
                topic,
                response);

            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(
                ex,
                "Firebase error sending notification to topic: {Topic}. Error Code: {ErrorCode}",
                topic,
                ex.MessagingErrorCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending FCM notification to topic: {Topic}",
                topic);
            return false;
        }
    }

    /// <summary>
    /// Subscribe device token to a topic
    /// </summary>
    public async Task<bool> SubscribeToTopicAsync(
        string deviceToken, 
        string topic,
        CancellationToken cancellationToken = default)
    {
        return await ManageTopicSubscriptionAsync(
            new List<string> { deviceToken }, 
            topic, 
            true, 
            cancellationToken);
    }

    /// <summary>
    /// Subscribe multiple device tokens to a topic
    /// </summary>
    public async Task<FcmBatchResponse> SubscribeToTopicAsync(
        List<string> deviceTokens, 
        string topic,
        CancellationToken cancellationToken = default)
    {
        return await ManageTopicSubscriptionBatchAsync(
            deviceTokens, 
            topic, 
            true, 
            cancellationToken);
    }

    /// <summary>
    /// Unsubscribe device token from a topic
    /// </summary>
    public async Task<bool> UnsubscribeFromTopicAsync(
        string deviceToken, 
        string topic,
        CancellationToken cancellationToken = default)
    {
        return await ManageTopicSubscriptionAsync(
            new List<string> { deviceToken }, 
            topic, 
            false, 
            cancellationToken);
    }

    /// <summary>
    /// Unsubscribe multiple device tokens from a topic
    /// </summary>
    public async Task<FcmBatchResponse> UnsubscribeFromTopicAsync(
        List<string> deviceTokens,
        string topic,
        CancellationToken cancellationToken = default)
    {
        return await ManageTopicSubscriptionBatchAsync(
            deviceTokens,
            topic,
            false,
            cancellationToken);
    }

    /// <summary>
    /// Send data-only message (silent notification)
    /// </summary>
    public async Task<bool> SendDataMessageAsync(
        string deviceToken,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        ContentAvailable = true
                    }
                }
            };

            var response = await _messaging.SendAsync(message, cancellationToken);

            _logger.LogInformation(
                "FCM data message sent successfully to device: {DeviceToken}. Message ID: {MessageId}",
                MaskToken(deviceToken),
                response);

            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(
                ex,
                "Firebase error sending data message to device: {DeviceToken}. Error Code: {ErrorCode}",
                MaskToken(deviceToken),
                ex.MessagingErrorCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending FCM data message to device: {DeviceToken}",
                MaskToken(deviceToken));
            return false;
        }
    }

    /// <summary>
    /// Validate device token by attempting to send a test message
    /// </summary>
    public async Task<bool> ValidateDeviceTokenAsync(
        string deviceToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Data = new Dictionary<string, string>
                {
                    { "test", "validation" }
                }
            };

            // Use dry run to validate without actually sending
            await _messaging.SendAsync(message, dryRun: true, cancellationToken);

            _logger.LogInformation("Device token validated successfully: {DeviceToken}", MaskToken(deviceToken));
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogWarning(
                ex,
                "Invalid device token: {DeviceToken}. Error Code: {ErrorCode}",
                MaskToken(deviceToken),
                ex.MessagingErrorCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating device token: {DeviceToken}",
                MaskToken(deviceToken));
            return false;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Manage topic subscription for single or multiple tokens
    /// </summary>
    private async Task<bool> ManageTopicSubscriptionAsync(
        List<string> deviceTokens,
        string topic,
        bool subscribe,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var action = subscribe ? "subscribe" : "unsubscribe";

            TopicManagementResponse response;
            if (subscribe)
            {
                response = await _messaging.SubscribeToTopicAsync(deviceTokens, topic);
            }
            else
            {
                response = await _messaging.UnsubscribeFromTopicAsync(deviceTokens, topic);
            }

            if (response.SuccessCount > 0)
            {
                _logger.LogInformation(
                    "Successfully {Action}d {Count} device(s) to/from topic: {Topic}. Success: {SuccessCount}, Failure: {FailureCount}",
                    action,
                    deviceTokens.Count,
                    topic,
                    response.SuccessCount,
                    response.FailureCount);
                return response.FailureCount == 0;
            }

            _logger.LogError(
                "Failed to {Action} device(s) to/from topic: {Topic}. All {Count} attempts failed",
                action,
                topic,
                deviceTokens.Count);
            return false;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(
                ex,
                "Firebase error managing topic subscription for topic: {Topic}. Error Code: {ErrorCode}",
                topic,
                ex.MessagingErrorCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error managing topic subscription for topic: {Topic}",
                topic);
            return false;
        }
    }

    /// <summary>
    /// Manage topic subscription for multiple tokens with detailed response
    /// </summary>
    private async Task<FcmBatchResponse> ManageTopicSubscriptionBatchAsync(
        List<string> deviceTokens,
        string topic,
        bool subscribe,
        CancellationToken cancellationToken = default)
    {
        var response = new FcmBatchResponse();

        if (deviceTokens == null || !deviceTokens.Any())
        {
            _logger.LogWarning("No device tokens provided for topic subscription");
            return response;
        }

        try
        {
            var action = subscribe ? "subscribe" : "unsubscribe";

            TopicManagementResponse topicResponse;
            if (subscribe)
            {
                topicResponse = await _messaging.SubscribeToTopicAsync(deviceTokens, topic);
            }
            else
            {
                topicResponse = await _messaging.UnsubscribeFromTopicAsync(deviceTokens, topic);
            }

            response.SuccessCount = topicResponse.SuccessCount;
            response.FailureCount = topicResponse.FailureCount;

            // Identify failed tokens
            for (int i = 0; i < topicResponse.Errors.Count; i++)
            {
                var error = topicResponse.Errors[i];
                if (error != null)
                {
                    response.FailedTokens.Add(deviceTokens[error.Index]);
                    response.ErrorMessages.Add(error.Reason ?? "Unknown error");
                }
            }

            _logger.LogInformation(
                "Topic {Action} completed. Success: {SuccessCount}, Failure: {FailureCount}",
                action,
                response.SuccessCount,
                response.FailureCount);
        }
        catch (FirebaseMessagingException ex)
        {
            response.FailureCount = deviceTokens.Count;
            response.FailedTokens = deviceTokens;

            _logger.LogError(
                ex,
                "Firebase error managing topic subscription for {Count} devices. Error Code: {ErrorCode}",
                deviceTokens.Count,
                ex.MessagingErrorCode);
        }
        catch (Exception ex)
        {
            response.FailureCount = deviceTokens.Count;
            response.FailedTokens = deviceTokens;

            _logger.LogError(
                ex,
                "Error managing topic subscription for {Count} devices",
                deviceTokens.Count);
        }

        return response;
    }

    /// <summary>
    /// Mask device token for logging (show only first and last 4 characters)
    /// </summary>
    private string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length <= 8)
            return "****";

        return $"{token.Substring(0, 4)}...{token.Substring(token.Length - 4)}";
    }

    #endregion
}

