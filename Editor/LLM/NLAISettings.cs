using UnityEngine;

[CreateAssetMenu(fileName = "New NLAI Settings", menuName = "NLAI/NLAI Settings", order = 0)]
public class NLAISettings : ScriptableObject
{
    [Tooltip("The API key for the LLM service.")]
    public string apiKey;

    [Tooltip("The endpoint URL for the LLM service.")]
    public string apiEndpoint = "https://api.openai.com/v1/chat/completions";
    
    [Tooltip("The model to use, e.g., 'gpt-4-turbo'")]
    public string model = "gpt-4-turbo";
} 