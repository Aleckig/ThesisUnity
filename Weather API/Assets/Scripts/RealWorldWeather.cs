using System;
using System.Collections;
using System.Diagnostics;
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
    public TMP_InputField cityInputField;
    public TextMeshProUGUI currentWeatherText;
    public TextMeshProUGUI forecastDay1Text;
    public TextMeshProUGUI forecastDay2Text;
    public TextMeshProUGUI forecastDay3Text;
    public TextMeshProUGUI errorText;
    
    [Header("Performance Tracking")]
    public bool logPerformanceToConsole = true;
    public bool logDetailedPerformance = true;  // Set to true for more detailed logs
    
    // Performance tracking variables
    private Stopwatch apiCallStopwatch = new Stopwatch();
    private int apiCallCount = 0;
    private long totalApiTime = 0;
    private long maxApiTime = 0;

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
            UnityEngine.Debug.LogError("City Input Field not assigned!");
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
            UnityEngine.Debug.LogError(message);
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
        // Start frame tracking
        int startFrame = Time.frameCount;
        float startTime = Time.realtimeSinceStartup;
        
        // Start total API call timer
        apiCallStopwatch.Reset();
        apiCallStopwatch.Start();
        long networkStartTime = apiCallStopwatch.ElapsedMilliseconds;
        
        // Log API call start
        if (logPerformanceToConsole && logDetailedPerformance)
        {
            UnityEngine.Debug.Log($"[API Call Started] Weather API at frame {startFrame}");
        }
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();
            
            // Calculate network time
            long networkTime = apiCallStopwatch.ElapsedMilliseconds - networkStartTime;
            long parsingStartTime = apiCallStopwatch.ElapsedMilliseconds;

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                ShowError($"Weather API Error: {webRequest.error}");
                isLoading = false;
                
                // Log failed request performance
                apiCallStopwatch.Stop();
                LogPerformance("Weather API (Failed)", apiCallStopwatch.ElapsedMilliseconds, networkTime, 0, startFrame);
            }
            else
            {
                try
                {
                    ParseCurrentWeather(webRequest.downloadHandler.text);
                    
                    // Calculate parsing time
                    long parsingTime = apiCallStopwatch.ElapsedMilliseconds - parsingStartTime;
                    
                    // Stop timer and record performance data
                    apiCallStopwatch.Stop();
                    LogPerformance("Weather API", apiCallStopwatch.ElapsedMilliseconds, networkTime, parsingTime, startFrame);
                }
                catch (Exception e)
                {
                    ShowError($"Error processing weather data: {e.Message}");
                    
                    // Log error performance
                    apiCallStopwatch.Stop();
                    LogPerformance("Weather API (Error)", apiCallStopwatch.ElapsedMilliseconds, networkTime, 
                        apiCallStopwatch.ElapsedMilliseconds - parsingStartTime, startFrame);
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
        // Start frame tracking
        int startFrame = Time.frameCount;
        float startTime = Time.realtimeSinceStartup;
        
        // Start total API call timer
        apiCallStopwatch.Reset();
        apiCallStopwatch.Start();
        long networkStartTime = apiCallStopwatch.ElapsedMilliseconds;
        
        // Log API call start
        if (logPerformanceToConsole && logDetailedPerformance)
        {
            UnityEngine.Debug.Log($"[API Call Started] Forecast API at frame {startFrame}");
        }
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();
            
            // Calculate network time
            long networkTime = apiCallStopwatch.ElapsedMilliseconds - networkStartTime;
            long parsingStartTime = apiCallStopwatch.ElapsedMilliseconds;

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                ShowError($"Forecast API Error: {webRequest.error}");
                isLoading = false;
                
                // Log failed request performance
                apiCallStopwatch.Stop();
                LogPerformance("Forecast API (Failed)", apiCallStopwatch.ElapsedMilliseconds, networkTime, 0, startFrame);
            }
            else
            {
                try
                {
                    ParseForecastData(webRequest.downloadHandler.text);
                    
                    // Calculate parsing time
                    long parsingTime = apiCallStopwatch.ElapsedMilliseconds - parsingStartTime;
                    
                    // Stop timer and record performance data
                    apiCallStopwatch.Stop();
                    LogPerformance("Forecast API", apiCallStopwatch.ElapsedMilliseconds, networkTime, parsingTime, startFrame);
                }
                catch (Exception e)
                {
                    ShowError($"Error processing forecast data: {e.Message}");
                    
                    // Log error performance
                    apiCallStopwatch.Stop();
                    LogPerformance("Forecast API (Error)", apiCallStopwatch.ElapsedMilliseconds, networkTime, 
                        apiCallStopwatch.ElapsedMilliseconds - parsingStartTime, startFrame);
                }
                finally
                {
                    isLoading = false;
                }
            }
        }
    }
    
    // Log performance metrics to console
    private void LogPerformance(string endpoint, long totalTime, long networkTime, long parsingTime, int startFrame)
    {
        // Update statistics
        apiCallCount++;
        totalApiTime += totalTime;
        maxApiTime = Math.Max(maxApiTime, totalTime);
        
        int endFrame = Time.frameCount;
        int frameSpan = endFrame - startFrame;
        
        // Log to console if enabled
        if (logPerformanceToConsole)
        {
            // Basic performance log
            UnityEngine.Debug.Log($"[API Performance] {endpoint}: Total={totalTime}ms, Network={networkTime}ms, Parsing={parsingTime}ms, Frames={frameSpan}");
            
            // Detailed performance statistics if enabled
            if (logDetailedPerformance)
            {
                UnityEngine.Debug.Log($"[API Stats] Call #{apiCallCount}, Avg={totalApiTime/apiCallCount}ms, Max={maxApiTime}ms");
                
                // Frame rate impact analysis
                if (frameSpan > 0)
                {
                    float msPerFrame = totalTime / (float)frameSpan;
                    UnityEngine.Debug.Log($"[Frame Impact] Approx {msPerFrame:F2}ms per frame during API call");
                }
            }
            
            // Log warning if performance is poor (over 500ms)
            if (totalTime > 500)
            {
                UnityEngine.Debug.LogWarning($"[API Performance Warning] {endpoint} call took {totalTime}ms, spanning {frameSpan} frames");
            }
            
            // Log error if performance is terrible (over 2 seconds)
            if (totalTime > 2000)
            {
                UnityEngine.Debug.LogError($"[API Performance Critical] {endpoint} call took {totalTime}ms, which will cause significant frame rate drops");
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