using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeatherEffectsManager : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem rainParticles;
    public ParticleSystem snowParticles;
    public ParticleSystem cloudParticles;
    public ParticleSystem fogParticles;
    public ParticleSystem thunderParticles;
    public ParticleSystem windParticles;

    [Header("Weather UI Elements")]
    public RawImage currentWeatherIcon;
    public RawImage forecastDay1Icon;
    public RawImage forecastDay2Icon;
    public RawImage forecastDay3Icon;

    [Header("Weather Icons (Drag Textures Here)")]
    [SerializeField] private Texture2D sunIcon;
    [SerializeField] private Texture2D cloudIcon;
    [SerializeField] private Texture2D rainIcon;
    [SerializeField] private Texture2D snowIcon;
    [SerializeField] private Texture2D thunderIcon;
    [SerializeField] private Texture2D fogIcon;

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

    private void Start()
    {
        ValidateIconReferences();
        DisableAllWeatherEffects();
    }

    private void ValidateIconReferences()
    {
        if (!sunIcon || !cloudIcon || !rainIcon || !snowIcon || !thunderIcon || !fogIcon)
        {
            Debug.LogWarning("Some weather textures are missing. Please assign them in the Inspector.");
        }
    }

    public void UpdateWeatherEffects(string weatherDescription, float windSpeed, float windDegrees, float temperature, bool isForecast = false, int forecastDay = 0)
    {
        // Convert meteorological wind degrees to Unity direction
        // Meteorological wind degrees: 0/360° = North (wind FROM North), 90° = East, 180° = South, 270° = West
        // We need to:
        // 1. Add 180° to get the direction the wind is blowing TOWARDS
        // 2. Convert to Unity's coordinate system
        float unityWindDegrees = (windDegrees + 180f) % 360f;
        Vector3 windDirection = Quaternion.Euler(0, -unityWindDegrees, 0) * Vector3.forward;
        
        weatherDescription = weatherDescription.ToLower();

        // Set the weather icon for either current weather or forecast
        SetWeatherIcon(weatherDescription, isForecast, forecastDay);

        // Only apply particle effects for current weather (not forecast)
        if (!isForecast)
        {
            DisableAllWeatherEffects();

            // Prioritize snow first before rain or cloud
            if (weatherDescription.Contains("snow"))
            {
                EnableSnow(GetSnowIntensity(weatherDescription), windSpeed, windDirection);
            }
            // Then check if rain is in the description
            else if (weatherDescription.Contains("rain") || weatherDescription.Contains("drizzle"))
            {
                EnableRain(GetRainIntensity(weatherDescription), windSpeed, windDirection);
            }
            // Only enable cloud particles if no snow or rain is active
            else if (weatherDescription.Contains("cloud") || weatherDescription.Contains("clear"))
            {
                EnableClouds(GetCloudIntensity(weatherDescription), windSpeed, windDirection);
            }

            // Check for thunderstorm
            if (weatherDescription.Contains("thunderstorm"))
                EnableThunder();

            // Check for fog conditions
            if (weatherDescription.Contains("fog") || weatherDescription.Contains("mist") || weatherDescription.Contains("haze"))
                EnableFog();

            // Enable wind effects
            EnableWind(windSpeed, windDirection);
        }
    }

    private void SetWeatherIcon(string weatherDescription, bool isForecast, int forecastDay)
    {
        Texture2D selectedTexture = DetermineWeatherTexture(weatherDescription);

        if (selectedTexture == null)
        {
            Debug.LogWarning($"No texture found for weather: {weatherDescription}");
            return;
        }

        if (isForecast)
        {
            RawImage targetIcon = null;
            switch (forecastDay)
            {
                case 1:
                    targetIcon = forecastDay1Icon;
                    break;
                case 2:
                    targetIcon = forecastDay2Icon;
                    break;
                case 3:
                    targetIcon = forecastDay3Icon;
                    break;
            }

            if (targetIcon != null)
            {
                targetIcon.texture = selectedTexture;
                targetIcon.enabled = true;
                targetIcon.SetNativeSize();
            }
        }
        else
        {
            if (currentWeatherIcon != null)
            {
                currentWeatherIcon.texture = selectedTexture;
                currentWeatherIcon.enabled = true;
                currentWeatherIcon.SetNativeSize();
            }
        }
    }

    private Texture2D DetermineWeatherTexture(string weatherDescription)
    {
        if (weatherDescription.Contains("clear"))
            return sunIcon;
        if (weatherDescription.Contains("cloud"))
            return cloudIcon;
        if (weatherDescription.Contains("rain") || weatherDescription.Contains("drizzle"))
            return rainIcon;
        if (weatherDescription.Contains("snow"))
            return snowIcon;
        if (weatherDescription.Contains("thunderstorm"))
            return thunderIcon;
        if (weatherDescription.Contains("fog") || weatherDescription.Contains("mist") || weatherDescription.Contains("haze"))
            return fogIcon;
            
        return cloudIcon; // default fallback
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
            var emission = rainParticles.emission;
            emission.rateOverTime = intensity;
            var main = rainParticles.main;
            main.startSpeed = 8f + windSpeed;

            // Apply wind direction to rain
            rainParticles.transform.rotation = Quaternion.LookRotation(windDirection);
            
            // Add slight downward tilt
            Vector3 currentRotation = rainParticles.transform.rotation.eulerAngles;
            rainParticles.transform.rotation = Quaternion.Euler(15f, currentRotation.y, currentRotation.z);
        }
    }

    void EnableSnow(float intensity, float windSpeed, Vector3 windDirection)
    {
        if (snowParticles)
        {
            snowParticles.gameObject.SetActive(true);
            var emission = snowParticles.emission;
            emission.rateOverTime = intensity;
            var main = snowParticles.main;
            main.startSpeed = 2f + windSpeed;

            // Apply wind direction to snow
            snowParticles.transform.rotation = Quaternion.LookRotation(windDirection);
            
            // Add slight downward tilt
            Vector3 currentRotation = snowParticles.transform.rotation.eulerAngles;
            snowParticles.transform.rotation = Quaternion.Euler(15f, currentRotation.y, currentRotation.z);
        }
    }

    void EnableClouds(float intensity, float windSpeed, Vector3 windDirection)
    {
        if (cloudParticles && intensity > 0)
        {
            cloudParticles.gameObject.SetActive(true);
            var emission = cloudParticles.emission;
            emission.rateOverTime = intensity;
            
            // Apply wind direction to clouds
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
            var main = windParticles.main;
            main.startSpeed = windSpeed;
            
            // Apply wind direction
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
}