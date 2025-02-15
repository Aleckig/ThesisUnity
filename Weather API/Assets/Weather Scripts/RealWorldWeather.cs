using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class RealWorldWeather : MonoBehaviour
{
    public string apiKey = "YOUR-API-KEY-GOES-HERE";
    public string city = "Paris";
    public float updateInterval = 600f;
    private float timer = 0f;

    [System.Serializable]
    public class ForecastData
    {
        public DateTime time;
        public float temperature;
        public float windSpeed;
        public float windDegrees;
        public string description;
    }

    void Start()
    {
        GetRealWeather();
        GetForecast();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            GetRealWeather();
            GetForecast();
            timer = 0f;
        }
    }

    // Fetch the real-time weather data
    public void GetRealWeather()
    {
        string uri = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=en";
        StartCoroutine(GetWeatherCoroutine(uri));
    }

    // Fetch the forecast data
    public void GetForecast()
    {
        string uri = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric&lang=en";
        StartCoroutine(GetForecastCoroutine(uri));
    }

    // Coroutine to get the current weather
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
                Debug.Log($"Weather API Response: {webRequest.downloadHandler.text}");
                ParseCurrentWeather(webRequest.downloadHandler.text); // Handle current weather response
            }
        }
    }

    // Coroutine to get the weather forecast
    IEnumerator GetForecastCoroutine(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Forecast request error: {webRequest.error}");
            }
            else
            {
                Debug.Log($"Forecast API Response: {webRequest.downloadHandler.text}");
                ParseForecastData(webRequest.downloadHandler.text); // Handle forecast response
            }
        }
    }

    // Parse the current weather data and update particle effects
    void ParseCurrentWeather(string json)
    {
        try
        {
            JObject jsonObject = JObject.Parse(json);
            string description = jsonObject["weather"]?[0]?["description"]?.Value<string>();
            float tempCelsius = jsonObject["main"]?["temp"]?.Value<float>() ?? 0f;
            float windSpeed = jsonObject["wind"]?["speed"]?.Value<float>() ?? 0f;
            float windDegrees = jsonObject["wind"]?["deg"]?.Value<float>() ?? 0f;

            // Display the current weather info in the console
            Debug.Log($"\nCurrent Weather in {city}:");
            Debug.Log($"Description: {description}");
            Debug.Log($"Temperature: {tempCelsius:F1}°C");
            Debug.Log($"Wind: {windSpeed:F1} m/s at {windDegrees:F1}°");

            // Update particle effects based on the weather description (current weather)
            FindFirstObjectByType<WeatherEffectsManager>()?.UpdateWeatherEffects(description, windSpeed, windDegrees);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing weather data: {e.Message}");
        }
    }

    // Parse and display the forecast data for the next 3 hours as debug text
    void ParseForecastData(string json)
    {
        try
        {
            JObject jsonObject = JObject.Parse(json);
            var forecastList = jsonObject["list"];

            // Only get the first 3 forecast entries (next 3 hours)
            for (int i = 0; i < 3; i++)
            {
                var item = forecastList[i];
                DateTime forecastTime = DateTime.Parse(item["dt_txt"]?.Value<string>());
                string forecastDescription = item["weather"]?[0]?["description"]?.Value<string>();
                float forecastTemp = item["main"]?["temp"]?.Value<float>() ?? 0f;
                float forecastWindSpeed = item["wind"]?["speed"]?.Value<float>() ?? 0f;
                float forecastWindDegrees = item["wind"]?["deg"]?.Value<float>() ?? 0f;

                // Log the forecast data to the console for the next 3 hours
                Debug.Log($"Forecast for {forecastTime}:");
                Debug.Log($"Description: {forecastDescription}");
                Debug.Log($"Temperature: {forecastTemp:F1}°C");
                Debug.Log($"Wind: {forecastWindSpeed:F1} m/s at {forecastWindDegrees:F1}°");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing forecast data: {e.Message}");
        }
    }
}
