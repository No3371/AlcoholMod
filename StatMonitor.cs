using UnityEngine;

namespace AlcoholMod
{
	public class StatMonitor
	{
		public bool debug;
		public float hourlyBaseline;
		public float hourlyChange;
		public float offset;
		public float scale;
		public float value;

		public float GetRateOfChange()
		{
			float result;
			if (Math.Abs(hourlyChange) < 0.01f) result = 0;
			else if (hourlyBaseline > 0)
			{
				result = Mathf.Min(hourlyChange / hourlyBaseline, 1) + Mathf.Max(0, hourlyChange - hourlyBaseline) * scale;
			}
			else result = hourlyChange * scale;

			return result;
		}

		public void Update(float currentValue, float elapsedGameSeconds)
		{
			float delta = currentValue - value;
			hourlyChange = Mathf.Lerp(hourlyChange, 3600f * delta / elapsedGameSeconds, 0.05f);
			value = currentValue;
		}
	}
}
