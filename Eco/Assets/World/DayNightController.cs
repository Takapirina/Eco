using System.Collections;
using UnityEngine;

public class DayNightController3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light sun; // Directional Light

    [Header("Day length (hours) by season")]
    [SerializeField] private float winterDayLength = 11f;
    [SerializeField] private float springDayLength = 14f;
    [SerializeField] private float summerDayLength = 17.5f;
    [SerializeField] private float autumnDayLength = 13f;

    [Header("Sun path")]
    [Tooltip("Yaw offset (asse Y) per orientare il percorso est->ovest nel tuo mondo")]
    [SerializeField] private float yaw = 30f;

    [Tooltip("Altezza del sole al sorgere/tramonto (gradi)")]
    [SerializeField] private float minElevation = 5f;

    [Tooltip("Altezza massima del sole a mezzogiorno (gradi)")]
    [SerializeField] private float maxElevation = 70f;

    [Tooltip("Angolo azimut a sorgere (gradi, attorno a Y)")]
    [SerializeField] private float azimuthAtSunrise = -90f;

    [Tooltip("Angolo azimut a tramonto (gradi, attorno a Y)")]
    [SerializeField] private float azimuthAtSunset = 90f;

    [Header("Intensity")]
    [SerializeField] private float dayMaxIntensity = 2f;
    [SerializeField] private float nightIntensity = 0.05f;
    [Tooltip("Minuti di transizione morbida (alba+tramonto). Evita stacchi netti.")]
    [SerializeField] private float twilightMinutes = 20f;

    [Header("Color over day")]
    [SerializeField] private bool driveColor = true;
    [SerializeField] private Gradient sunColorOverDay;

    [Header("Season tint")]
    [SerializeField] private bool driveSeasonTint = true;
    [SerializeField, Range(0f, 1f)] private float seasonTintStrength = 0.35f;

    [SerializeField] private Color winterTint = new Color(0.85f, 0.90f, 1.00f, 1f); // più freddo
    [SerializeField] private Color springTint = new Color(1.00f, 1.00f, 1.00f, 1f); // neutro
    [SerializeField] private Color summerTint = new Color(1.00f, 0.98f, 0.90f, 1f); // più caldo
    [SerializeField] private Color autumnTint = new Color(1.00f, 0.92f, 0.85f, 1f); // caldo morbido

    private void Reset()
    {
        if (sun == null)
        {
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    sun = l;
                    break;
                }
            }
        }
    }



    private void Update()
    {
        if (sun == null || TimeContext.I == null) return;
        if (Input.GetKeyDown(KeyCode.Q)) TimeContext.I.StartSkipHours(12, 1f); // 12 ore in 1 secondo


        // ---- TIME ----
        float timeHours = TimeContext.I.hour + TimeContext.I.minute / 60f;
        float day01 = timeHours / 24f;

        float dayLength = GetDayLengthBySeason(TimeContext.I.currentSeason);
        float sunrise = 12f - dayLength * 0.5f;
        float sunset  = 12f + dayLength * 0.5f;

        // ---- ROTATION ----
        bool isDay = timeHours >= sunrise && timeHours <= sunset;

        float elevation;
        float azimuth;

        if (isDay)
        {
            float dayT = Mathf.InverseLerp(sunrise, sunset, TimeContext.I.GetTimeHoursSmooth()); // 0..1
            float elevation01 = Mathf.Sin(dayT * Mathf.PI);            // 0..1..0 (naturale)

            elevation = Mathf.Lerp(minElevation, maxElevation, elevation01);
            azimuth   = Mathf.Lerp(azimuthAtSunrise, azimuthAtSunset, dayT);
        }
        else
        {
            // Sole sotto l'orizzonte
            elevation = minElevation - 20f;
            azimuth = (timeHours < sunrise) ? azimuthAtSunrise : azimuthAtSunset;
        }

        sun.transform.rotation = Quaternion.Euler(elevation, azimuth + yaw, 0f);
        

        // ---- INTENSITY ----
        sun.intensity = CalculateIntensityByHourAndSeason(
            nightIntensity: nightIntensity,
            dayMaxIntensity: dayMaxIntensity,
            twilightMinutes: twilightMinutes
        );

        // ---- COLOR ----
        if (driveColor && sunColorOverDay != null)
        {
            Color baseColor = sunColorOverDay.Evaluate(day01);

            if (driveSeasonTint)
            {
                Color tint = GetSeasonTint(TimeContext.I.currentSeason);
                // Lerp verso una versione "tintata" del colore base
                baseColor = Color.Lerp(baseColor, baseColor * tint, seasonTintStrength);
            }

            sun.color = baseColor;
        }
    }

    public float CalculateIntensityByHourAndSeason(
        float nightIntensity = 0.05f,
        float dayMaxIntensity = 1.2f,
        float twilightMinutes = 20f
    )
    {
        float timeHours = TimeContext.I.hour + TimeContext.I.minute / 60f;
        float dayLength = GetDayLengthBySeason(TimeContext.I.currentSeason);

        float sunrise = 12f - dayLength * 0.5f;
        float sunset  = 12f + dayLength * 0.5f;

        float twilightH = twilightMinutes / 60f;
        float sunriseSoft = sunrise - twilightH;
        float sunsetSoft  = sunset + twilightH;

        // Notte piena
        if (timeHours <= sunriseSoft || timeHours >= sunsetSoft)
            return nightIntensity;

        // 0..1 dentro il range "morbido"
        float t = Mathf.InverseLerp(sunriseSoft, sunsetSoft, timeHours);

        // Campana: sale fino a metà e poi scende
        float bell = Mathf.Sin(t * Mathf.PI);

        return Mathf.Lerp(nightIntensity, dayMaxIntensity, bell);
    }

    private float GetDayLengthBySeason(Season s)
    {
        return s switch
        {
            Season.Winter => winterDayLength,
            Season.Spring => springDayLength,
            Season.Summer => summerDayLength,
            Season.Autumn => autumnDayLength,
            _ => 12f
        };
    }

    private Color GetSeasonTint(Season s)
    {
        return s switch
        {
            Season.Winter => winterTint,
            Season.Spring => springTint,
            Season.Summer => summerTint,
            Season.Autumn => autumnTint,
            _ => Color.white
        };
    }
}