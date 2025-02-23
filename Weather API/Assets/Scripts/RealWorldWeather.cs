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
    private bool isLoading = false;

    [Header("UI Elements")]
    public TMP_InputField cityInputField; // Add reference to input field
    public TextMeshProUGUI currentWeatherText;
    public TextMeshProUGUI forecastDay1Text;
    public TextMeshProUGUI forecastDay2Text;
    public TextMeshProUGUI forecastDay3Text;
    public TextMeshProUGUI errorText;

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
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }

        // Setup input field
        if (cityInputField != null)
        {
            cityInputField.text = city; // Set initial city
            cityInputField.onSubmit.AddListener(OnCityInputSubmit);
        }
        else
        {
            Debug.LogError("City Input Field not assigned!");
        }

        GetRealWeather();
        GetForecast();
    }

    void Update()
    {
        if (!isLoading)
        {
            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                GetRealWeather();
                GetForecast();
                timer = 0f;
            }
        }

        // Check for Enter key when input field is focused
        if (cityInputField != null && cityInputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            OnCityInputSubmit(cityInputField.text);
        }
    }

    // Method to handle input field submission
    private void OnCityInputSubmit(string newCity)
    {
        if (string.IsNullOrEmpty(newCity))
        {
            ShowError("Please enter a valid city name.");
            return;
        }

        UpdateCity(newCity);
        cityInputField.DeactivateInputField(); // Remove focus from input field
    }

    public void UpdateCity(string newCity)
    {
        if (string.IsNullOrEmpty(newCity))
        {
            ShowError("Please enter a valid city name.");
            return;
        }

        city = newCity;
        timer = updateInterval; // Reset timer to trigger immediate update
        
        // Show loading state
        isLoading = true;
        if (currentWeatherText) 
            currentWeatherText.text = $"Loading weather for {city}...";
        
        if (forecastDay1Text) forecastDay1Text.text = "Loading...";
        if (forecastDay2Text) forecastDay2Text.text = "Loading...";
        if (forecastDay3Text) forecastDay3Text.text = "Loading...";
        
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }

        GetRealWeather();
        GetForecast();
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
            StartCoroutine(HideErrorAfterDelay(5f));
        }
        else
        {
            Debug.LogError(message);
        }
    }

    private IEnumerator HideErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    public void GetRealWeather()
    {
        string uri = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=en";
        StartCoroutine(GetWeatherCoroutine(uri));
    }

    public void GetForecast()
    {
        string uri = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric&lang=en";
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
                ShowError($"Weather API Error: {webRequest.error}");
                isLoading = false;
            }
            else
            {
                try
                {
                    ParseCurrentWeather(webRequest.downloadHandler.text);
                }
                catch (Exception e)
                {
                    ShowError($"Error processing weather data: {e.Message}");
                }
                finally
                {
                    isLoading = false;
                }
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
                ShowError($"Forecast API Error: {webRequest.error}");
                isLoading = false;
            }
            else
            {
                try
                {
                    ParseForecastData(webRequest.downloadHandler.text);
                }
                catch (Exception e)
                {
                    ShowError($"Error processing forecast data: {e.Message}");
                }
                finally
                {
                    isLoading = false;
                }
            }
        }
    }

    void ParseCurrentWeather(string json)
    {
        try
        {
            JObject jsonObject = JObject.Parse(json);
            
            // Check for API error response
            if (jsonObject["cod"]?.Value<int>() != 200)
            {
                string errorMessage = jsonObject["message"]?.Value<string>() ?? "Unknown error";
                ShowError($"Weather API Error: {errorMessage}");
                return;
            }

            string description = jsonObject["weather"]?[0]?["description"]?.Value<string>();
            string iconCode = jsonObject["weather"]?[0]?["icon"]?.Value<string>();
            float tempCelsius = jsonObject["main"]?["temp"]?.Value<float>() ?? 0f;
            float windSpeed = jsonObject["wind"]?["speed"]?.Value<float>() ?? 0f;
            float windDegrees = jsonObject["wind"]?["deg"]?.Value<float>() ?? 0f;

            string windDirection = GetWindDirection(windDegrees);

            if (currentWeatherText)
            {
                currentWeatherText.text = $"City: {city}\nCurrent Weather:\n{description}\nTemp: {tempCelsius:F1}°C\nWind: {windSpeed:F1} m/s, {windDirection}";
            }

            WeatherEffectsManager weatherEffectsManager = FindFirstObjectByType<WeatherEffectsManager>();
            if (weatherEffectsManager != null)
            {
                weatherEffectsManager.UpdateWeatherEffects(description, windSpeed, windDegrees, tempCelsius);
            }
        }
        catch (Exception e)
        {
            ShowError($"Error parsing weather data: {e.Message}");
        }
    }

    void ParseForecastData(string json)
    {
        try
        {
            JObject jsonObject = JObject.Parse(json);
            
            // Check for API error response
            if (jsonObject["cod"]?.Value<string>() != "200")
            {
                string errorMessage = jsonObject["message"]?.Value<string>() ?? "Unknown error";
                ShowError($"Forecast API Error: {errorMessage}");
                return;
            }

            var forecastList = jsonObject["list"];

            if (forecastList == null)
            {
                ShowError("No forecast data available");
                return;
            }

            int forecastDaysFound = 0;
            WeatherEffectsManager weatherEffectsManager = FindFirstObjectByType<WeatherEffectsManager>();

            foreach (var item in forecastList)
            {
                DateTime forecastTime = DateTime.Parse(item["dt_txt"]?.Value<string>());
                
                if (forecastTime.Hour == 12)
                {
                    string forecastDescription = item["weather"]?[0]?["description"]?.Value<string>();
                    float forecastTemp = item["main"]?["temp"]?.Value<float>() ?? 0f;
                    float forecastWindSpeed = item["wind"]?["speed"]?.Value<float>() ?? 0f;
                    float forecastWindDegrees = item["wind"]?["deg"]?.Value<float>() ?? 0f;

                    string windDirection = GetWindDirection(forecastWindDegrees);
                    string forecastText = $"Day {forecastDaysFound + 1}:\n{forecastDescription}\nTemp: {forecastTemp:F1}°C\nWind: {forecastWindSpeed:F1} m/s, {windDirection}";

                    switch (forecastDaysFound)
                    {
                        case 0:
                            if (forecastDay1Text) forecastDay1Text.text = forecastText;
                            break;
                        case 1:
                            if (forecastDay2Text) forecastDay2Text.text = forecastText;
                            break;
                        case 2:
                            if (forecastDay3Text) forecastDay3Text.text = forecastText;
                            break;
                    }

                    if (weatherEffectsManager != null)
                    {
                        weatherEffectsManager.UpdateWeatherEffects(
                            forecastDescription,
                            forecastWindSpeed,
                            forecastWindDegrees,
                            forecastTemp,
                            true,
                            forecastDaysFound + 1
                        );
                    }

                    forecastDaysFound++;
                    if (forecastDaysFound >= 3) break;
                }
            }

            if (forecastDaysFound == 0)
            {
                ShowError("No forecast data available for the next days");
            }
        }
        catch (Exception e)
        {
            ShowError($"Error parsing forecast data: {e.Message}");
        }
    }

    string GetWindDirection(float degrees)
    {
        if (degrees >= 0 && degrees < 45) return "North";
        if (degrees >= 45 && degrees < 135) return "East";
        if (degrees >= 135 && degrees < 225) return "South";
        if (degrees >= 225 && degrees < 315) return "West";
        return "North";
    }
}