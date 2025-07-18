using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


public class LLMResponse : MonoBehaviour
{
    static public LLMResponse Instance;
    private List<CardsData> _currentCards;
    public string apiResponse;

    //predefined context and instructions
    private readonly string predefinedContext = "You are a personal advisor, tasked to give advice without follow-up questions";
    private string instructions;

    
    void Awake()
    {
        LLMResponse.Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        _currentCards = SaveChosenCards.LoadChosenCards(true);
        instructions = "Your responses should be clear and three sentences or 50 tokens max. You should keep the following strengths in mind: " + _currentCards[0].ChosenOption + " and " + _currentCards[1].ChosenOption;
        // Initializes advisor with context and instructions on first load
        Debug.Log("Initializing personal advisor");
        StartCoroutine(HandleLLMResponse(instructions));

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator HandleLLMResponse(string fullPrompt)
    {
        // Wait for the result of the asynchronous HTTP request
        yield return StartCoroutine(GetLLMResponse(fullPrompt, (response) => apiResponse = response));
        
        Debug.Log($"AI Response: {apiResponse}");

        yield return new WaitForSeconds(7f);
    }
    
    private IEnumerator GetLLMResponse(string fullPrompt, System.Action<string> onResponse)
    {
        // Start the HTTP request as a separate Task and wait for it to complete
        Task<string> task = RequestAIResponseAsync(fullPrompt);

        // Yield until the Task is finished
        while (!task.IsCompleted)
        {
            yield return null;
        }

        // Once the task is completed, pass the result to the callback
        if (task.IsCompletedSuccessfully)
        {
            onResponse?.Invoke(task.Result);
        }
        else
        {
            Debug.Log(task.Result);
            onResponse?.Invoke("Error: Failed to get AI response.");
        }
    }

    // Async method that makes the HTTP request
    private async Task<string> RequestAIResponseAsync(string fullPrompt)
    {
        string apiKey = ""; //Put your API key here.
        string apiUrl = "https://api.openai.com/v1/chat/completions"; //Default API URL for the newest models. For older models the url may be different

        var payload = new
        {
            model = "gpt-3.5-turbo", // You can use other models based on your requirements
            messages = new List<object>{
                new { role = "system", content = predefinedContext + "\n" +  instructions},
                new { role = "user", content = fullPrompt }
            },
            max_tokens = 200,
            temperature = 0.7
        };

        var jsonPayload = JsonConvert.SerializeObject(payload);

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Make the API request asynchronously
            var response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);
                if (responseObject != null && responseObject["choices"] is Newtonsoft.Json.Linq.JArray choices)
                {
                    Debug.Log("hello");
                    return choices[0]["message"]["content"].ToString().Trim();
                }
                else
                {
                    return "Error parsing AI response.";
                }
            }
            else
            {
                return "Failed to get a response from the AI. " + response;
            }
        }
    }
}
