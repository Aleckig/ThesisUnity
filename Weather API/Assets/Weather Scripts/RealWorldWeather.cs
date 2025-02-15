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
    public float updateInterval = 600f; // Time in seconds (600 = 10 minutes)
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
        // Initial weather requests
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

    public void GetRealWeather()
    {
        string uri = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}";
        StartCoroutine(GetWeatherCoroutine(uri));
    }

    public void GetForecast()
    {
        string uri = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}";
        StartCoroutine(GetForecastCoroutine(uri));
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
                ParseCurrentWeather(webRequest.downloadHandler.text);
            }
        }
    }

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
                ParseForecast(webRequest.downloadHandler.text);
            }
        }
    }

    string GetWindDirection(float degrees)
    {
        string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
        int index = Mathf.RoundToInt(degrees / 45f);
        return directions[index % 8];
    }

    void ParseCurrentWeather(string json)
    {
        try
        {
            JObject jsonObject = JObject.Parse(json);
           
            string description = jsonObject["weather"]?[0]?["description"]?.Value<string>();
            float tempKelvin = jsonObject["main"]?["temp"]?.Value<float>() ?? 0f;
            float windSpeed = jsonObject["wind"]?["speed"]?.Value<float>() ?? 0f;
            float windDegrees = jsonObject["wind"]?["deg"]?.Value<float>() ?? 0f;

            float tempCelsius = tempKelvin - 273.15f;
            float tempFahrenheit = tempCelsius * 9f/5f + 32f;
            string windDirection = GetWindDirection(windDegrees);

            Debug.Log($"\nCurrent Weather in {city}:");
            Debug.Log($"Description: {description}");
            Debug.Log($"Temperature: {tempCelsius:F1}°C ({tempFahrenheit:F1}°F)");
            Debug.Log($"Wind: {windSpeed:F1} m/s from {windDirection} ({windDegrees:F1}°)");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing current weather data: {e.Message}");
        }
    }

    void ParseForecast(string json)
    {
        try
        {
            JObject jsonObject = JObject.Parse(json);
            var forecastList = jsonObject["list"];
            List<ForecastData> forecasts = new List<ForecastData>();

            Debug.Log($"\nForecast for {city}:");
            
            // Get next 4 forecasts (12 hours ahead in 3-hour steps)
            for (int i = 0; i < 4; i++)
            {
                if (forecastList?[i] != null)
                {
                    var forecast = forecastList[i];
                    
                    // Parse timestamp
                    long timestamp = forecast["dt"]?.Value<long>() ?? 0;
                    DateTime forecastTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToLocalTime();
                    
                    // Parse weather data
                    float tempKelvin = forecast["main"]?["temp"]?.Value<float>() ?? 0f;
                    float windSpeed = forecast["wind"]?["speed"]?.Value<float>() ?? 0f;
                    float windDegrees = forecast["wind"]?["deg"]?.Value<float>() ?? 0f;
                    string description = forecast["weather"]?[0]?["description"]?.Value<string>();
                    
                    // Convert temperature
                    float tempCelsius = tempKelvin - 273.15f;
                    float tempFahrenheit = tempCelsius * 9f/5f + 32f;
                    string windDirection = GetWindDirection(windDegrees);

                    Debug.Log($"\nTime: {forecastTime:HH:mm, dd MMM}");
                    Debug.Log($"Description: {description}");
                    Debug.Log($"Temperature: {tempCelsius:F1}°C ({tempFahrenheit:F1}°F)");
                    Debug.Log($"Wind: {windSpeed:F1} m/s from {windDirection} ({windDegrees:F1}°)");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing forecast data: {e.Message}");
        }
    }
}