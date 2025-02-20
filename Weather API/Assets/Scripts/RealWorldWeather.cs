using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class RealWorldWeather : MonoBehaviour
{
    public string apiKey = "API-KEY";
    public string city = "Paris";
    public float updateInterval = 600f;
    private float timer = 0f;

    [Header("UI Elements")]
    public TextMeshProUGUI currentWeatherText;
    public TextMeshProUGUI forecastDay1Text;
    public TextMeshProUGUI forecastDay2Text;
    public TextMeshProUGUI forecastDay3Text;

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

            string windDirection = GetWindDirection(windDegrees);

            // Display the current weather info in the UI, including the city name and wind direction
            if (currentWeatherText)
            {
                currentWeatherText.text = $"City: {city}\nCurrent Weather:\n{description}\nTemp: {tempCelsius:F1}°C\nWind: {windSpeed:F1} m/s, {windDirection}";
            }

            // Update particle effects based on the weather description (current weather)
            FindFirstObjectByType<WeatherEffectsManager>()?.UpdateWeatherEffects(description, windSpeed, windDegrees, tempCelsius);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing weather data: {e.Message}");
        }
    }

    // Parse and display the forecast data for the next 3 days
    void ParseForecastData(string json)
    {
        try
        {
            JObject jsonObject = JObject.Parse(json);
            var forecastList = jsonObject["list"];

            // Only get the forecast for the next 3 days
            for (int i = 0; i < 3; i++)
            {
                var item = forecastList[i];
                DateTime forecastTime = DateTime.Parse(item["dt_txt"]?.Value<string>());
                string forecastDescription = item["weather"]?[0]?["description"]?.Value<string>();
                float forecastTemp = item["main"]?["temp"]?.Value<float>() ?? 0f;
                float forecastWindSpeed = item["wind"]?["speed"]?.Value<float>() ?? 0f;
                float forecastWindDegrees = item["wind"]?["deg"]?.Value<float>() ?? 0f;

                string windDirection = GetWindDirection(forecastWindDegrees);

                string forecastText = $"Day {i + 1}:\n{forecastDescription}\nTemp: {forecastTemp:F1}°C\nWind: {forecastWindSpeed:F1} m/s, {windDirection}";

                // Update UI for each forecast day
                if (i == 0 && forecastDay1Text) forecastDay1Text.text = forecastText;
                if (i == 1 && forecastDay2Text) forecastDay2Text.text = forecastText;
                if (i == 2 && forecastDay3Text) forecastDay3Text.text = forecastText;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing forecast data: {e.Message}");
        }
    }

    // Convert wind degrees to cardinal directions (e.g., 0° = North, 90° = East)
    string GetWindDirection(float degrees)
    {
        if (degrees >= 0 && degrees < 45) return "North";
        if (degrees >= 45 && degrees < 135) return "East";
        if (degrees >= 135 && degrees < 225) return "South";
        if (degrees >= 225 && degrees < 315) return "West";
        return "North"; // Default case
    }
}