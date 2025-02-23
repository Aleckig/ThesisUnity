using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Debug = UnityEngine.Debug; // Resolve ambiguity
using TMPro;

public class PerformanceLogger : MonoBehaviour
{
    public TMP_InputField cityInputField;
    public ParticleSystem rainParticles;
    public ParticleSystem snowParticles;
    public ParticleSystem cloudParticles;
    public ParticleSystem fogParticles;
    public ParticleSystem thunderParticles;
    public ParticleSystem windParticles;
    private string logFilePath;
    private float frameTimeTotal;
    private int frameCount;
    private Stopwatch apiTimer;

    void Start()
    {
        Debug.Log("Log file saved at: " + logFilePath);

        logFilePath = Application.persistentDataPath + "/PerformanceLog.txt";
        if (cityInputField != null)
        {
            cityInputField.onEndEdit.AddListener(OnCityChanged);
        }
        apiTimer = new Stopwatch();
    }

    void Update()
    {
        frameTimeTotal += Time.deltaTime;
        frameCount++;
    }

    void OnCityChanged(string cityName)
    {
        if (string.IsNullOrEmpty(cityName)) return;

        StartCoroutine(FetchWeatherData(cityName));
    }

    IEnumerator FetchWeatherData(string cityName)
    {
        apiTimer.Restart();
        UnityWebRequest request = UnityWebRequest.Get($"https://api.weather.com/v1/{cityName}");
        yield return request.SendWebRequest();
        apiTimer.Stop();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("API Error: " + request.error);
            yield return new WaitForSeconds(2);
            StartCoroutine(FetchWeatherData(cityName));
            yield break;
        }

        float avgFPS = frameCount / frameTimeTotal;
        float memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
        float cpuUsage = SystemInfo.processorCount;
        float gpuUsage = SystemInfo.graphicsMemorySize;
        int totalParticleCount = (rainParticles ? rainParticles.particleCount : 0) +
                                 (snowParticles ? snowParticles.particleCount : 0) +
                                 (cloudParticles ? cloudParticles.particleCount : 0) +
                                 (fogParticles ? fogParticles.particleCount : 0) +
                                 (thunderParticles ? thunderParticles.particleCount : 0) +
                                 (windParticles ? windParticles.particleCount : 0);
        string responseData = request.downloadHandler.text;

        string logEntry = $"City: {cityName}\n" +
                          $"API Response Time: {apiTimer.ElapsedMilliseconds} ms\n" +
                          $"API Response: {responseData.Substring(0, Mathf.Min(responseData.Length, 100))}...\n" +
                          $"Average FPS: {avgFPS:F2}\n" +
                          $"Memory Usage: {memoryUsage:F2} MB\n" +
                          $"CPU Cores: {cpuUsage}\n" +
                          $"GPU Memory: {gpuUsage} MB\n" +
                          $"Total Particle Count: {totalParticleCount}\n" +
                          "----------------------------------\n";

        Debug.Log(logEntry);
        File.AppendAllText(logFilePath, logEntry);
        
        frameTimeTotal = 0f;
        frameCount = 0;
    }
}