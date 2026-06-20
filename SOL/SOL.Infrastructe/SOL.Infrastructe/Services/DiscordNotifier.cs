using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using SOL.Application.Common.Interfaces;

namespace Template.Infrastructe.Services;

public class DiscordNotifier : IDiscordNotifier
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public DiscordNotifier(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task SendExceptionAsync(Exception ex, HttpContext context)
    {
        var webhookUrl = _config["Discord:Webhook"];
        if (string.IsNullOrWhiteSpace(webhookUrl))
            return;

        var stack = FormatStackTrace(ex.StackTrace);
        var requestBody = await GetRequestDataAsync(context);
        var payload = new
        {
            username = "FPS Backend",
            embeds = new[]
            {
                new
                {
                    title = "🚨 Exception Occurred",
                    description = $"**{Escape(ex.Message)}**",
                    color = 15158332,
                    fields = new object[]
                    {
                        new
                        {
                            name = "Exception Type",
                            value = ex.GetType().FullName ?? "Unknown",
                            inline = true
                        },
                        new
                        {
                            name = "Timestamp (UTC)",
                            value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                            inline = true
                        },
                        new
                        {
                            name = "Request Path",
                            value = context.Request.Path.ToString(),
                            inline = false
                        },
                        new
                        {
                            name = "User",
                            value = GetUser(context),
                            inline = true
                        },
                        new
                        {
                            name = "IP",
                            value = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                            inline = true
                        },
                        new
                        {
                            name = "Stack Trace",
                            value = $"```{stack}```",
                            inline = false
                        },
                        new
                        {
                            name = "Request Body",
                            value = $"```{Escape(requestBody)}```",
                            inline = false
                        },
                    },
                    footer = new
                    {
                        text = "FPS Backend System"
                    }
                }
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _httpClient.PostAsync(webhookUrl, content, cts.Token);
        }
        catch
        {
            // intentionally swallow to avoid recursive failures
        }
    }

    private static string GetUser(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
            return context.User.Identity?.Name ?? "Authenticated";

        return "Anonymous";
    }

    private static string Escape(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "No message";

        // prevent breaking markdown
        return text.Replace("```", "`");
    }

    private static string FormatStackTrace(string? stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace))
            return "No stack trace available";

        const int maxLength = 1000;

        if (stackTrace.Length > maxLength)
            return stackTrace[..maxLength] + "...";

        return stackTrace;
    }
    private static async Task<string> GetRequestDataAsync(HttpContext context)
    {
        try
        {
            var sb = new StringBuilder();

            // ---------- Body ----------
            context.Request.EnableBuffering();
            context.Request.Body.Position = 0;

            using (var reader = new StreamReader(
                       context.Request.Body,
                       Encoding.UTF8,
                       detectEncodingFromByteOrderMarks: false,
                       leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                sb.AppendLine("Body:");
                sb.AppendLine(string.IsNullOrWhiteSpace(body) ? "Empty" : body);
            }

            // ---------- Query ----------
            sb.AppendLine("Query:");
            if (context.Request.Query.Any())
            {
                foreach (var q in context.Request.Query)
                    sb.AppendLine($"{q.Key}={q.Value}");
            }
            else
            {
                sb.AppendLine("Empty");
            }

            // ---------- Form ----------
            sb.AppendLine("Form:");
            if (context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();

                if (form.Any())
                {
                    foreach (var f in form)
                        sb.AppendLine($"{f.Key}={f.Value}");
                }
                else
                {
                    sb.AppendLine("Empty");
                }
            }
            else
            {
                sb.AppendLine("Not a form request");
            }

            // ---------- Trim length ----------
            const int maxLength = 1500;
            var result = sb.ToString();

            return result.Length > maxLength
                ? result[..maxLength] + "..."
                : result;
        }
        catch
        {
            return "Unable to read request data";
        }
    }
}