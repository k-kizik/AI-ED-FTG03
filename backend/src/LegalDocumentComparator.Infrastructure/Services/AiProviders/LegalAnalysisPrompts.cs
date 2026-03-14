namespace LegalDocumentComparator.Infrastructure.Services.AiProviders;

public static class LegalAnalysisPrompts
{
    public static string GetSystemPrompt()
    {
        return @"You are a legal document analysis expert specialized in identifying and explaining changes in legal documents. 
Your role is to provide detailed, actionable insights for legal professionals including lawyers, paralegals, and contract managers.
Focus on the legal significance of changes, potential risks, and practical recommendations.";
    }

    public static string GetAnalysisPrompt(
        string changesText,
        string originalTextExcerpt,
        string newTextExcerpt,
        int totalChanges)
    {
        return $@"Analyze the following changes between two versions of a legal document.

CHANGES DETECTED ({totalChanges} total):
{changesText}

ORIGINAL TEXT EXCERPT:
{originalTextExcerpt}

NEW TEXT EXCERPT:
{newTextExcerpt}

Provide a detailed analysis in JSON format with this EXACT structure:
{{
  ""summary"": ""Brief summary of all changes (2-3 sentences)"",
  ""legalImplications"": ""Detailed legal implications and consequences of these changes (3-5 sentences)"",
  ""riskAssessment"": ""Low"",
  ""keyChanges"": [
    {{
      ""title"": ""Descriptive title of the change"",
      ""description"": ""What specifically changed"",
      ""impact"": ""How this affects the agreement or parties"",
      ""severity"": ""Critical"",
      ""recommendation"": ""Specific recommended action""
    }}
  ]
}}

CRITICAL REQUIREMENTS:
- riskAssessment MUST be exactly one of: Low, Medium, High, Critical
- Each keyChange severity MUST be exactly one of: Low, Medium, High, Critical
- Include 3-5 key changes, prioritized by importance
- Be specific and actionable in all descriptions
- Focus on legal significance, not just textual differences
- Provide concrete recommendations for legal professionals
- Respond ONLY with valid JSON, no markdown, no code blocks, no additional text before or after the JSON";
    }

    public static string FormatChangesText(List<Application.Common.Models.ChangeDetail> changes, int maxChangesToShow = 20)
    {
        var changesToDisplay = changes.Take(maxChangesToShow);
        var displayText = string.Join("\n", changesToDisplay.Select((c, i) =>
            $"{i + 1}. Page {c.PageNumber} - {c.Type}: '{TruncateText(c.OldText, 100)}' ? '{TruncateText(c.NewText, 100)}'"));

        if (changes.Count > maxChangesToShow)
        {
            displayText += $"\n... and {changes.Count - maxChangesToShow} more changes";
        }

        return displayText;
    }

    public static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }

    public static string GetJsonExtractionInstructions()
    {
        return "If the response contains markdown code blocks (```json), remove them and extract only the JSON content.";
    }
}
