using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter,
    AllSesons, // use for dynamic events like pokemon encounter
}

public class TimeContext : MonoBehaviour
{
    public static TimeContext I { get; private set; }

    public Season currentSeason = Season.Spring;

    public int year = 0;            
    public int month = 1;
    public int day = 1;
    public int hour = 0;
    public int minute = 0;

    [Header("Time speed settings")]
    [SerializeField] public float gameMinutesPerRealSecond = 0.5f;

    // ---- SKIP ----
    private int _skipMinutesRemaining = 0;
    private float _skipMinutesPerSecond = 0f;
    private float _normalMinutesPerSecond = 0f;

    public bool IsSkipping => _skipMinutesRemaining > 0;

    private float _acc = 0f;
    private void Awake()
    {
        Debug.Log($"[TimeContext] Awake {GetInstanceID()} scene={gameObject.scene.name}");
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        float speed = IsSkipping ? _skipMinutesPerSecond : gameMinutesPerRealSecond;

        _acc += Time.deltaTime * speed;

        while (_acc >= 1f)
        {
            _acc -= 1f;

            minute++;
            addhours();

            if (IsSkipping)
            {
                _skipMinutesRemaining--;
                if (_skipMinutesRemaining <= 0)
                {
                    _skipMinutesRemaining = 0;
                    _skipMinutesPerSecond = 0f;

                    _acc = 0f;
                    break;
                }
            }
        }
    }

    public void addhours()
    {
        if(minute >= 60)
        {
            //debugTime();
            minute = 0;
            hour ++;
            if (hour >= 24)
            {
                addDays();
            }
        }
    }

    public void addDays()
    {
        if (hour >= 24)
        {
            day++;
            addMonths();
            currentSeason = changeSeason(day, month);
            hour = 0;
        }
    }

    public static int getDaysInMonth(int month, int year){
        switch(month){
            case 1: case 3: case 5: case 7: case 8: case 10: case 12:
                return 31;
            case 2:
                if (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0))
                    return 29;
                else
                    return 28;
            default:
                return 30;
        }
    }

    public static Season changeSeason(int day, int month)
    {
        // Spring: 20 Mar -> 20 Jun
        if ((month == 3 && day >= 20) || month == 4 || month == 5 || (month == 6 && day <= 20))
            return Season.Spring;

        // Summer: 21 Jun -> 22 Sep
        if ((month == 6 && day >= 21) || month == 7 || month == 8 || (month == 9 && day <= 22))
            return Season.Summer;

        // Autumn: 23 Sep -> 20 Dec
        if ((month == 9 && day >= 23) || month == 10 || month == 11 || (month == 12 && day <= 20))
            return Season.Autumn;

        // Winter: 21 Dec -> 19 Mar
        return Season.Winter;
    }

    public void addMonths()
    {
        if ( day > getDaysInMonth(this.month, this.year))
        {
            day = 1;
            month++;
            if (month > 12)
            {
                month = 1;
                year++;
            }
        }
    }

    public float GetTimeHoursSmooth()
    {
        float minutes = hour * 60f + minute + _acc;
        return minutes / 60f;
    }

    public void StartSkipHours(int hours, float durationSeconds = 1f)
    {
        int minutes = Mathf.Max(0, hours) * 60;
        if (minutes == 0) return;

        if (IsSkipping) return;

        _normalMinutesPerSecond = gameMinutesPerRealSecond;
        _skipMinutesRemaining = minutes;
        _skipMinutesPerSecond = minutes / Mathf.Max(0.01f, durationSeconds);

        _acc = 0f;
    }

    public void debugTime()
    {
        Debug.Log($"[TIME] Y:{year} M:{month} D:{day} H:{hour} Min:{minute} Season:{currentSeason}");
    }
}
