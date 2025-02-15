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

    // Update the weather effects based on weather description, wind speed, direction, and temperature
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
    }

    // Helper methods to determine intensity based on description
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

    // Enable rain particle system based on intensity, wind speed, and direction
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
    
    // Enable snow particle system based on intensity, wind speed, and direction
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
    
    // Enable cloud particle system based on intensity, wind speed, and direction
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

    // Enable thunder particle system
    void EnableThunder()
    {
        if (thunderParticles)
        {
            thunderParticles.gameObject.SetActive(true);
        }
    }
    
    // Enable fog particle system
    void EnableFog()
    {
        if (fogParticles)
        {
            fogParticles.gameObject.SetActive(true);
        }
    }

    // Enable wind particle system based on wind speed and direction
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

    // Disable all weather particle systems
    void DisableAllWeatherEffects()
    {
        if (rainParticles) rainParticles.gameObject.SetActive(false);
        if (snowParticles) snowParticles.gameObject.SetActive(false);
        if (cloudParticles) cloudParticles.gameObject.SetActive(false);
        if (fogParticles) fogParticles.gameObject.SetActive(false);
        if (thunderParticles) thunderParticles.gameObject.SetActive(false);
        if (windParticles) windParticles.gameObject.SetActive(false);
    }
}