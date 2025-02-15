using TMPro;
using UnityEngine;

public class WeatherEffectsManager : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem rainParticles;
    public ParticleSystem snowParticles;
    public ParticleSystem cloudParticles;
    public ParticleSystem fogParticles;
    public ParticleSystem thunderParticles;
    public ParticleSystem windParticles; // New Wind Particle System
    
    [Header("Rain Intensity Settings")]
    [Range(0f, 1000f)] public float lightRainEmission = 100f;
    [Range(0f, 1000f)] public float moderateRainEmission = 400f;
    [Range(0f, 1000f)] public float heavyRainEmission = 800f;

    [Header("Snow Intensity Settings")]
    [Range(0f, 1000f)] public float lightSnowEmission = 100f;
    [Range(0f, 1000f)] public float moderateSnowEmission = 300f;
    [Range(0f, 1000f)] public float heavySnowEmission = 600f;

    [Header("Cloud Coverage Settings")]
    [Range(0f, 100f)] public float fewCloudsEmission = 20f;
    [Range(0f, 100f)] public float scatteredCloudsEmission = 40f;
    [Range(0f, 100f)] public float brokenCloudsEmission = 60f;
    [Range(0f, 100f)] public float overcastCloudsEmission = 80f;

    [Header("UI Elements")]
    public TextMeshProUGUI currentWeatherText;
    public TextMeshProUGUI forecastDay1Text;
    public TextMeshProUGUI forecastDay2Text;
    public TextMeshProUGUI forecastDay3Text;

    private ParticleSystem.EmissionModule rainEmission;
    private ParticleSystem.EmissionModule snowEmission;
    private ParticleSystem.EmissionModule cloudEmission;
    private ParticleSystem.EmissionModule windEmission;
    private ParticleSystem.MainModule rainMain;
    private ParticleSystem.MainModule snowMain;
    private ParticleSystem.MainModule cloudMain;
    private ParticleSystem.MainModule windMain;

    void Start()
    {
        if (rainParticles)
        {
            rainMain = rainParticles.main;
            rainEmission = rainParticles.emission;
        }
        if (snowParticles)
        {
            snowMain = snowParticles.main;
            snowEmission = snowParticles.emission;
        }
        if (cloudParticles)
        {
            cloudMain = cloudParticles.main;
            cloudEmission = cloudParticles.emission;
        }
        if (windParticles)
        {
            windMain = windParticles.main;
            windEmission = windParticles.emission;
        }

        DisableAllWeatherEffects();
    }

    public void UpdateWeatherEffects(string weatherDescription, float windSpeed, float windDegrees, float temperature)
    {
        DisableAllWeatherEffects();
        
        Vector3 windDirection = Quaternion.Euler(0, windDegrees, 0) * Vector3.forward;
        weatherDescription = weatherDescription.ToLower();

        if (weatherDescription.Contains("rain") || weatherDescription.Contains("drizzle"))
        {
            float rainIntensity = GetRainIntensity(weatherDescription);
            EnableRain(rainIntensity, windSpeed, windDirection);
        }
        
        if (weatherDescription.Contains("snow"))
        {
            float snowIntensity = GetSnowIntensity(weatherDescription);
            EnableSnow(snowIntensity, windSpeed, windDirection);
        }
        
        if (weatherDescription.Contains("cloud") || weatherDescription.Contains("clear"))
        {
            float cloudIntensity = GetCloudIntensity(weatherDescription);
            EnableClouds(cloudIntensity, windSpeed, windDirection);
        }
        
        if (weatherDescription.Contains("thunderstorm"))
        {
            EnableThunder();
        }
        
        if (weatherDescription.Contains("fog") || weatherDescription.Contains("mist") || weatherDescription.Contains("haze"))
        {
            EnableFog();
        }

        EnableWind(windSpeed, windDirection);

        UpdateCurrentWeatherUI(weatherDescription, windSpeed, windDegrees, temperature);
    }

    float GetRainIntensity(string description)
    {
        if (description.Contains("light") || description.Contains("drizzle"))
            return lightRainEmission;
        else if (description.Contains("heavy") || description.Contains("extreme") || description.Contains("intensity"))
            return heavyRainEmission;
        else
            return moderateRainEmission;
    }

    float GetSnowIntensity(string description)
    {
        if (description.Contains("light"))
            return lightSnowEmission;
        else if (description.Contains("heavy"))
            return heavySnowEmission;
        else
            return moderateSnowEmission;
    }

    float GetCloudIntensity(string description)
    {
        if (description.Contains("clear"))
            return 0f;
        else if (description.Contains("few"))
            return fewCloudsEmission;
        else if (description.Contains("scattered"))
            return scatteredCloudsEmission;
        else if (description.Contains("broken"))
            return brokenCloudsEmission;
        else if (description.Contains("overcast"))
            return overcastCloudsEmission;
        else
            return scatteredCloudsEmission;
    }
    
    void EnableRain(float intensity, float windSpeed, Vector3 windDirection)
    {
        if (rainParticles)
        {
            rainParticles.gameObject.SetActive(true);
            rainEmission.rateOverTime = intensity;
            rainMain.startSpeed = 8f + windSpeed;
            rainParticles.transform.rotation = Quaternion.LookRotation(windDirection);
        }
    }
    
    void EnableSnow(float intensity, float windSpeed, Vector3 windDirection)
    {
        if (snowParticles)
        {
            snowParticles.gameObject.SetActive(true);
            snowEmission.rateOverTime = intensity;
            snowMain.startSpeed = 2f + windSpeed;
            snowParticles.transform.rotation = Quaternion.LookRotation(windDirection);
        }
    }
    
    void EnableClouds(float intensity, float windSpeed, Vector3 windDirection)
    {
        if (cloudParticles && intensity > 0)
        {
            cloudParticles.gameObject.SetActive(true);
            cloudEmission.rateOverTime = intensity;
            cloudMain.startSpeed = windSpeed;
            cloudParticles.transform.rotation = Quaternion.LookRotation(windDirection);
        }
    }

    void EnableThunder()
    {
        if (thunderParticles)
        {
            thunderParticles.gameObject.SetActive(true);
        }
    }
    
    void EnableFog()
    {
        if (fogParticles)
        {
            fogParticles.gameObject.SetActive(true);
        }
    }

    void EnableWind(float windSpeed, Vector3 windDirection)
    {
        if (windParticles)
        {
            windParticles.gameObject.SetActive(true);
            windEmission.rateOverTime = windSpeed;
            windMain.startSpeed = windSpeed;
            windParticles.transform.rotation = Quaternion.LookRotation(windDirection);
        }
    }
    
    void DisableAllWeatherEffects()
    {
        if (rainParticles) rainParticles.gameObject.SetActive(false);
        if (snowParticles) snowParticles.gameObject.SetActive(false);
        if (cloudParticles) cloudParticles.gameObject.SetActive(false);
        if (fogParticles) fogParticles.gameObject.SetActive(false);
        if (thunderParticles) thunderParticles.gameObject.SetActive(false);
        if (windParticles) windParticles.gameObject.SetActive(false);
    }

    public void UpdateCurrentWeatherUI(string description, float windSpeed, float windDegrees, float temperature)
    {
        if (currentWeatherText)
        {
            currentWeatherText.text = $"Current Weather:\n{description}\nTemp: {temperature:F1}°C\nWind: {windSpeed:F1} m/s at {windDegrees:F1}°";
        }
    }

    public void UpdateForecastUI(int dayOffset, string weatherDescription, float windSpeed, float temperature)
    {
        string forecastText = $"Day {dayOffset}:\n {weatherDescription}\nTemp: {temperature:F1}°C\nWind: {windSpeed:F1}m/s";
        switch (dayOffset)
        {
            case 1: if (forecastDay1Text) forecastDay1Text.text = forecastText; break;
            case 2: if (forecastDay2Text) forecastDay2Text.text = forecastText; break;
            case 3: if (forecastDay3Text) forecastDay3Text.text = forecastText; break;
        }
    }
}