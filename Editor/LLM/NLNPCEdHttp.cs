using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class NLNPCEdHttp
{
    private class LLMRequest
    {
        public string model;
        public ResponseFormat response_format;
        public Message[] messages;
    }

    private class ResponseFormat
    {
        public string type;
    }
    
    private class Message
    {
        public string role;
        public string content;
    }
    
    public static async Task<(bool, string)> InvokeLLM(string systemPrompt, string userMessage, NLNPCSettings settings)
    {
        var requestBody = new LLMRequest
        {
            model = settings.model,
            response_format = new ResponseFormat { type = "json_object" },
            messages = new[]
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user", content = userMessage }
            }
        };

        string jsonBody = JsonConvert.SerializeObject(requestBody);
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);

        using (var request = new UnityWebRequest(settings.apiEndpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {settings.apiKey}");

            var asyncOp = request.SendWebRequest();

            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"LLM request failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
                return (false, null);
            }

            try
            {
                var responseJson = JObject.Parse(request.downloadHandler.text);
                string content = responseJson["choices"][0]["message"]["content"].Value<string>();
                return (true, content);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse LLM response: {e.Message}");
                Debug.LogError($"Raw Response: {request.downloadHandler.text}");
                return (false, null);
            }
        }
    }
} 