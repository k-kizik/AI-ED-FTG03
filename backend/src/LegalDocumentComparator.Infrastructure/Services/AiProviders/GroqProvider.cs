using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LegalDocumentComparator.Infrastructure.Services.AiProviders;

public class GroqProvider : IAiProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GroqProvider> _logger;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;

    public GroqProvider(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GroqProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiKey = configuration["AI:Groq:ApiKey"] ?? string.Empty;
        _model = configuration["AI:Groq:Model"] ?? "llama-3.1-70b-versatile";
        _baseUrl = configuration["AI:Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1";
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrWhiteSpace(_apiKey);
    }

    public string GetProviderName()
    {
        return "Groq Cloud";
    }

    public async Task<ComparisonAnalysis> AnalyzeDocumentChangesAsync(
        string originalText,
        string newText,
        List<ChangeDetail> changes,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable())
        {
            throw new InvalidOperationException("Groq API key is not configured");
        }

        var changesText = LegalAnalysisPrompts.FormatChangesText(changes);
        var originalExcerpt = LegalAnalysisPrompts.TruncateText(originalText, 100_000);
        var newExcerpt = LegalAnalysisPrompts.TruncateText(newText, 100_000);

        var systemPrompt = LegalAnalysisPrompts.GetSystemPrompt();
        var userPrompt = LegalAnalysisPrompts.GetAnalysisPrompt(
            changesText,
            originalExcerpt,
            newExcerpt,
            changes.Count);

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var groqRequest = new GroqChatRequest
            {
                Model = _model,
                Messages = new[]
                {
                    new GroqMessage { Role = "system", Content = systemPrompt },
                    new GroqMessage { Role = "user", Content = userPrompt }
                },
                Temperature = 0.3,
                MaxTokens = 32768,
                TopP = 1
            };

            var response = await httpClient.PostAsJsonAsync(
                $"{_baseUrl}/chat/completions",
                groqRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Groq API request failed: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return CreateFallbackAnalysis(changes);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var groqResponse = JsonSerializer.Deserialize<GroqChatResponse>(responseContent);

            if (groqResponse?.Choices == null || groqResponse.Choices.Length == 0)
            {
                _logger.LogWarning("Groq returned empty response");
                return CreateFallbackAnalysis(changes);
            }

            var aiResponseText = groqResponse.Choices[0].Message.Content;

            var cleanedJson = ExtractJsonFromResponse(aiResponseText);

            try
            {
                var analysis = JsonSerializer.Deserialize<ComparisonAnalysis>(
                    cleanedJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (analysis != null && ValidateAnalysis(analysis))
                {
                    _logger.LogInformation("Successfully analyzed document changes using Groq ({Model})", _model);
                    return analysis;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse Groq response as JSON: {Response}",
                    cleanedJson.Substring(0, Math.Min(500, cleanedJson.Length)));
            }

            return CreateFallbackAnalysis(changes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq API");
            return CreateFallbackAnalysis(changes);
        }
    }

    private string ExtractJsonFromResponse(string response)
    {
        response = response.Trim();

        if (response.StartsWith("```json"))
        {
            response = response.Substring(7);
        }
        else if (response.StartsWith("```"))
        {
            response = response.Substring(3);
        }

        if (response.EndsWith("```"))
        {
            response = response.Substring(0, response.Length - 3);
        }

        return response.Trim();
    }

    private bool ValidateAnalysis(ComparisonAnalysis analysis)
    {
        return !string.IsNullOrWhiteSpace(analysis.Summary) &&
               !string.IsNullOrWhiteSpace(analysis.RiskAssessment);
    }

    private ComparisonAnalysis CreateFallbackAnalysis(List<ChangeDetail> changes)
    {
        return new ComparisonAnalysis
        {
            Summary = $"Document comparison detected {changes.Count} changes across {changes.Select(c => c.PageNumber).Distinct().Count()} pages.",
            LegalImplications = "AI analysis unavailable. Manual review recommended for a complete legal analysis of the changes.",
            RiskAssessment = changes.Count > 10 ? "High" : changes.Count > 5 ? "Medium" : "Low",
            KeyChanges = changes.Take(5).Select((c, i) => new KeyChange
            {
                Title = $"Change #{i + 1} on Page {c.PageNumber}",
                Description = $"{c.Type}: {LegalAnalysisPrompts.TruncateText(c.OldText, 50)} ? {LegalAnalysisPrompts.TruncateText(c.NewText, 50)}",
                Impact = "Requires legal review",
                Severity = c.Severity.ToString(),
                Recommendation = "Review this change carefully with legal counsel"
            }).ToList()
        };
    }

    private class GroqChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = null!;

        [JsonPropertyName("messages")]
        public GroqMessage[] Messages { get; set; } = null!;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("top_p")]
        public double TopP { get; set; }
    }

    private class GroqMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = null!;

        [JsonPropertyName("content")]
        public string Content { get; set; } = null!;
    }

    private class GroqChatResponse
    {
        [JsonPropertyName("choices")]
        public GroqChoice[]? Choices { get; set; }
    }

    private class GroqChoice
    {
        [JsonPropertyName("message")]
        public GroqMessage Message { get; set; } = null!;
    }
}
