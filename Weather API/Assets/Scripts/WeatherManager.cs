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
        InitializeParticleSystems();
        ValidateIconReferences();
        DisableAllWeatherEffects();
    }

    private void InitializeParticleSystems()
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
    }
    

    private void ValidateIconReferences()
    {
        if (!sunIcon || !cloudIcon || !rainIcon || !snowIcon || !thunderIcon)
        {
            Debug.LogWarning("Some weather textures are missing. Please assign them in the Inspector.");
        }
    }

    public void UpdateWeatherEffects(string weatherDescription, float windSpeed, float windDegrees, float temperature, bool isForecast = false, int forecastDay = 0)
    {
        DisableAllWeatherEffects();
        
        Vector3 windDirection = Quaternion.Euler(0, windDegrees, 0) * Vector3.forward;
        weatherDescription = weatherDescription.ToLower();

        SetWeatherIcon(weatherDescription, isForecast, forecastDay);

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

   private void SetWeatherIcon(string weatherDescription, bool isForecast, int forecastDay)
    {
        // Select the correct texture
        Texture2D selectedTexture = null;
        if (weatherDescription.Contains("clear"))
            selectedTexture = sunIcon;
        else if (weatherDescription.Contains("cloud"))
            selectedTexture = cloudIcon;
        else if (weatherDescription.Contains("rain") || weatherDescription.Contains("drizzle"))
            selectedTexture = rainIcon;
        else if (weatherDescription.Contains("snow"))
            selectedTexture = snowIcon;
        else if (weatherDescription.Contains("thunderstorm"))
            selectedTexture = thunderIcon;

        if (selectedTexture == null)
        {
            Debug.LogError($"❌ No texture found for weather: {weatherDescription}");
            return;
        }

        Debug.Log($"🎨 Selected Texture: {selectedTexture.name}");

        // Remove the Sprite creation as it's not needed for RawImage
        if (isForecast)
        {
            RawImage targetIcon = null;
            string dayText = "";

            switch (forecastDay)
            {
                case 1:
                    targetIcon = forecastDay1Icon;
                    dayText = "Day 1";
                    break;
                case 2:
                    targetIcon = forecastDay2Icon;
                    dayText = "Day 2";
                    break;
                case 3:
                    targetIcon = forecastDay3Icon;
                    dayText = "Day 3";
                    break;
                default:
                    Debug.LogError($"❌ Invalid forecast day: {forecastDay}");
                    return;
            }

            if (targetIcon != null)
            {
                // Make sure the RawImage component is active and visible
                if (!targetIcon.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"⚠️ Forecast {dayText} GameObject is inactive!");
                    targetIcon.gameObject.SetActive(true);
                }

                targetIcon.texture = selectedTexture;
                
                // Ensure the RawImage has proper settings
                targetIcon.enabled = true;
                
                // Set proper size if needed
                targetIcon.SetNativeSize();

                Debug.Log($"📅 Forecast {dayText} icon set to {selectedTexture.name}");
            }
            else
            {
                Debug.LogError($"❌ Forecast {dayText} RawImage component is NULL!");
            }
        }
        else
        {
            if (currentWeatherIcon != null)
            {
                currentWeatherIcon.texture = selectedTexture;
                currentWeatherIcon.enabled = true;
                currentWeatherIcon.SetNativeSize();
                Debug.Log($"☀️ Current weather icon set to {selectedTexture.name}");
            }
            else
            {
                Debug.LogError("❌ currentWeatherIcon is NULL!");
            }
        }
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
}