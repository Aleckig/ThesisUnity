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

    [Header("Scene Lighting")]
    public Light directionalLight; // Reference to main directional light

    [Header("Lighting Settings")]
    [Range(0f, 1f)] public float clearSkyLightIntensity = 1.0f;
    [Range(0f, 1f)] public float cloudyLightIntensity = 0.7f;
    [Range(0f, 1f)] public float rainyLightIntensity = 0.5f;
    [Range(0f, 1f)] public float stormLightIntensity = 0.3f;
    [Range(0f, 1f)] public float foggyLightIntensity = 0.4f;
    [Range(0f, 1f)] public float nightLightIntensity = 0.1f;
    public Color clearSkyLightColor = new Color(1f, 0.95f, 0.85f); // Warm sunlight
    public Color cloudyLightColor = new Color(0.8f, 0.8f, 0.8f); // Grayish
    public Color rainyLightColor = new Color(0.7f, 0.7f, 0.8f); // Slightly blue
    public Color stormLightColor = new Color(0.4f, 0.4f, 0.5f); // Dark blue-gray
    public Color foggyLightColor = new Color(0.8f, 0.8f, 0.7f); // Yellowish gray
    public Color nightLightColor = new Color(0.2f, 0.2f, 0.4f); // Dark blue

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
    [Range(0f, 100f)] public float brokenCloudsEmission = 80f;
    [Range(0f, 200f)] public float overcastCloudsEmission = 200f;

    private void Start()
    {
        ValidateIconReferences();
        ValidateLightReference();
        DisableAllWeatherEffects();
    }

    private void ValidateIconReferences()
    {
        if (!sunIcon || !cloudIcon || !rainIcon || !snowIcon || !thunderIcon || !fogIcon)
        {
            Debug.LogWarning("Some weather textures are missing. Please assign them in the Inspector.");
        }
    }

    private void ValidateLightReference()
    {
        if (directionalLight == null)
        {
            Debug.LogWarning("Directional light reference is missing. Please assign it in the Inspector.");
            // Try to find the main directional light if not assigned
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    Debug.Log("Found directional light automatically: " + light.name);
                    break;
                }
            }
        }
    }

    public void UpdateWeatherEffects(string weatherDescription, float windSpeed, float windDegrees, float temperature, bool isForecast = false, int forecastDay = 0)
    {
        // Convert meteorological wind degrees to Unity direction
        float unityWindDegrees = (windDegrees + 180f) % 360f;
        Vector3 windDirection = Quaternion.Euler(0, -unityWindDegrees, 0) * Vector3.forward;
        
        weatherDescription = weatherDescription.ToLower();

        // Set the weather icon for either current weather or forecast
        SetWeatherIcon(weatherDescription, isForecast, forecastDay);

        // Only apply particle effects and lighting for current weather (not forecast)
        if (!isForecast)
        {
            DisableAllWeatherEffects();

            // Update directional light based on weather
            UpdateDirectionalLight(weatherDescription, temperature);

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

    private void UpdateDirectionalLight(string weatherDescription, float temperature)
    {
        if (directionalLight == null) return;

        // Adjust light intensity based on weather conditions
        float intensity = clearSkyLightIntensity;
        Color lightColor = clearSkyLightColor;

        // Determine time of day (this is simplified - you might want to get actual time data)
        bool isNight = IsNightTime();

        if (isNight)
        {
            intensity = nightLightIntensity;
            lightColor = nightLightColor;
        }
        else
        {
            if (weatherDescription.Contains("thunderstorm"))
            {
                intensity = stormLightIntensity;
                lightColor = stormLightColor;
            }
            else if (weatherDescription.Contains("rain") || weatherDescription.Contains("drizzle"))
            {
                intensity = rainyLightIntensity;
                lightColor = rainyLightColor;
            }
            else if (weatherDescription.Contains("snow"))
            {
                // Snow can be bright due to reflection
                intensity = cloudyLightIntensity;
                lightColor = cloudyLightColor;
            }
            else if (weatherDescription.Contains("fog") || weatherDescription.Contains("mist") || weatherDescription.Contains("haze"))
            {
                intensity = foggyLightIntensity;
                lightColor = foggyLightColor;
            }
            else if (weatherDescription.Contains("cloud"))
            {
                // Adjust intensity based on cloud coverage
                if (weatherDescription.Contains("few"))
                    intensity = clearSkyLightIntensity * 0.9f;
                else if (weatherDescription.Contains("scattered"))
                    intensity = clearSkyLightIntensity * 0.8f;
                else if (weatherDescription.Contains("broken"))
                    intensity = cloudyLightIntensity;
                else if (weatherDescription.Contains("overcast"))
                    intensity = cloudyLightIntensity * 0.8f;
                else
                    intensity = cloudyLightIntensity;
                
                lightColor = cloudyLightColor;
            }
        }

        // Apply the changes
        directionalLight.intensity = intensity;
        directionalLight.color = lightColor;

        // Adjust light direction based on time of day (optional)
        if (isNight)
        {
            // Moon position (opposite to the sun)
            directionalLight.transform.rotation = Quaternion.Euler(320, 30, 0);
        }
        else
        {
            // Sun position (typical daytime)
            directionalLight.transform.rotation = Quaternion.Euler(50, 30, 0);
        }
    }

    private bool IsNightTime()
    {
        // This is a placeholder. You could:
        // 1. Use the actual time from the API if available
        // 2. Use System.DateTime.Now for the local time
        // 3. Or use a simple day/night cycle in your game
        
        // For demonstration, let's use a simple check
        // Assuming this is connected to RealWorldWeather script
        // which might have time data from the API
        
        // Simple implementation (using current system time)
        int hour = System.DateTime.Now.Hour;
        return hour < 6 || hour > 18;
    }

    private void SetWeatherIcon(string weatherDescription, bool isForecast, int forecastDay)
    {
        // Same as original implementation
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
        // Same as original implementation
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
        // Same as original implementation
        if (description.Contains("light") || description.Contains("drizzle"))
            return lightRainEmission;
        else if (description.Contains("heavy") || description.Contains("extreme") || description.Contains("intensity"))
            return heavyRainEmission;
        else
            return moderateRainEmission;
    }

    float GetSnowIntensity(string description)
    {
        // Same as original implementation
        if (description.Contains("light"))
            return lightSnowEmission;
        else if (description.Contains("heavy"))
            return heavySnowEmission;
        else
            return moderateSnowEmission;
    }

    float GetCloudIntensity(string description)
    {
        // Same as original implementation
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

    // The following methods remain the same as the original implementation
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