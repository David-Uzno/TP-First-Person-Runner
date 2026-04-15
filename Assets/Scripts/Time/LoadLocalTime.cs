using System;
using UnityEngine;

public enum TimeUnit
{
	Hours,
	Minutes,
	Seconds
}

public class LoadLocalTime : MonoBehaviour
{
	public float GetNormalizedTime()
	{
		DateTime localTime = DateTime.Now;

		float daySeconds = localTime.Hour * 3600f + localTime.Minute * 60f + localTime.Second + (localTime.Millisecond / 1000f);
		const float secondsPerDay = 24f * 3600f;
		return Mathf.Clamp01(daySeconds / secondsPerDay);
	}

	public int GetUnitValue(TimeUnit unit)
	{
		DateTime localTime = DateTime.Now;

        return unit switch
        {
            TimeUnit.Hours => localTime.Hour,
            TimeUnit.Minutes => localTime.Minute,
            TimeUnit.Seconds => localTime.Second,
            _ => 0,
        };
    }
}
