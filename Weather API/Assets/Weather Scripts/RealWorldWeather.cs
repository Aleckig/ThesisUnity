using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class RealWorldWeather : MonoBehaviour
{
    public string apiKey = "YOUR-API-KEY-GOES-HERE";
    public string city = "Paris";

    void Start()
    {
        GetRealWeather();
    }

    public void GetRealWeather()
    {
        string uri = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}";
        StartCoroutine(GetWeatherCoroutine(uri));
    }

    IEnumerator GetWeatherCoroutine(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Web request error: {webRequest.error}");
            }
            else
            {
                ParseJson(webRequest.downloadHandler.text);
            }
        }
    }

    void ParseJson(string json)
    {
        try
        {
            JObject jsonObject = JObject.Parse(json);
            
            // Get weather data
            string description = jsonObject["weather"]?[0]?["description"]?.Value<string>();
            float tempKelvin = jsonObject["main"]?["temp"]?.Value<float>() ?? 0f;
            float windSpeed = jsonObject["wind"]?["speed"]?.Value<float>() ?? 0f;

            // Convert temperature
            float tempCelsius = tempKelvin - 273.15f;
            float tempFahrenheit = tempCelsius * 9f/5f + 32f;

            // Print to console
            Debug.Log($"Weather in {city}:");
            Debug.Log($"Description: {description}");
            Debug.Log($"Temperature: {tempCelsius:F1}°C ({tempFahrenheit:F1}°F)");
            Debug.Log($"Wind Speed: {windSpeed:F1} m/s");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing weather data: {e.Message}");
        }
    }
}