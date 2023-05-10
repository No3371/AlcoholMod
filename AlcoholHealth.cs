using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AlcoholMod
{
	[MelonLoader.RegisterTypeInIl2Cpp]
	internal class AlcoholHealth : MonoBehaviour
	{
		public const float MIN_PERMILLE_FOR_BLUR = 0.5f;
		public const float MAX_PERMILLE_FOR_BLUR = 2.5f;
		public const float MAX_VALUE_FOR_BLUR = 0.992f;
		public const float MIN_PERMILLE_FOR_STAGGERING = 1f;

		public const float ALCOHOL_TO_PERMILLE = 18;
		public const float MIN_UPTAKE_SCALE = 0.1f;

		public const float PERMILLE_REDUCTION_PER_GAME_SECOND = 0.15f / 3600f;
		public const float THIRST_PER_PERMILLE_SECOND = 25f / 3600f;
		public const float FATIGUE_PER_PERMILLE_SECOND = 4.583f / 3600f;
		public const float FROSTBITE_TEMP_BONUS_PER_PERMILLE = 3;
		public const float BODY_TEMP_BONUS_PER_PERMILLE = -2;
        private AlcoholHealthData? data;
        internal AlcoholHealthData? Data
		{
			get
			{
				return data;
			}
			set
			{
				data = value;
				ResetStatMonitors();
			}
		}

        private StatMonitor thirstMonitor = new StatMonitor();
		private StatMonitor fatigueMonitor = new StatMonitor();

        void Awake()
		{
			ResetStatMonitors();
		}

		public AlcoholHealth(IntPtr intPtr) : base(intPtr) { }

		public void DrankAlcohol(float amount, float uptakeGameSeconds)
		{
			Hunger hunger = GameManager.GetHungerComponent();
			float hungerScale = Mathf.Clamp01(Math.Max(MIN_UPTAKE_SCALE, hunger.GetCalorieReserves() / hunger.m_MaxReserveCalories));
			float scaledUptakeGameSeconds = uptakeGameSeconds * hungerScale;
			scaledUptakeGameSeconds = Mathf.Max(scaledUptakeGameSeconds, 1);
            AlcoholUptake item = new AlcoholUptake(amount, scaledUptakeGameSeconds);
            Data.uptakes.Add(item);
			// MelonLogger.Msg(string.Format("Added uptake: DrankAlcohol: {0} / {1}", item.amountPerGameSecond, item.remainingGameSeconds));
		}

		public StatMonitor FatigueMonitor => AlcoholMod.Instance.HealthManager.fatigueMonitor;
		public StatMonitor ThirstMonitor => AlcoholMod.Instance.HealthManager.thirstMonitor;

		internal float GetAlcoholBlurValue()
		{
			return Mathf.Clamp01((AlcoholMod.Instance.HealthManager.Data.alcoholPermille - MIN_PERMILLE_FOR_BLUR) / (MAX_PERMILLE_FOR_BLUR - MIN_PERMILLE_FOR_BLUR)) * MAX_VALUE_FOR_BLUR;
		}

		internal void Update()
		{
			if (Data == null) return;
			float elapsedGameSeconds = Utils.NotNan(GameManager.GetTimeOfDayComponent().GetTODSeconds(Time.deltaTime), "Nan in ModHealthManager.Update");
			if (elapsedGameSeconds <= 0) return;

			Thirst? thirst  = GameManager.GetThirstComponent();
			Fatigue? fatigue  = GameManager.GetFatigueComponent();
			if (thirst == null || fatigue == null) return;
			UpdateStatMonitors(elapsedGameSeconds, thirst, fatigue);
			ProcessAlcohol(elapsedGameSeconds, thirst, fatigue);
		}


		[HideFromIl2Cpp]
		private void ProcessAlcohol(float elapsedGameSeconds, Thirst thirst, Fatigue fatigue)
		{
			for (int i = Data.uptakes.Count - 1; i >= 0; i--)
			{
				AlcoholUptake uptake = Data.uptakes[i];
				Data.alcoholPermille += Utils.NotNan(elapsedGameSeconds * uptake.amountPerGameSecond * ALCOHOL_TO_PERMILLE, "Nan in ProcessAlcohol addition");
				uptake.remainingGameSeconds -= elapsedGameSeconds;

				if (uptake.remainingGameSeconds <= 0) Data.uptakes.RemoveAt(i);
			}

			if (Data.alcoholPermille > 0)
			{
				Data.alcoholPermille -= Utils.NotNan(elapsedGameSeconds * PERMILLE_REDUCTION_PER_GAME_SECOND, "Nan in ProcessAlcohol subtraction");
			}

			thirst.AddThirst(elapsedGameSeconds * Data.alcoholPermille * THIRST_PER_PERMILLE_SECOND);
			fatigue.AddFatigue(elapsedGameSeconds * Data.alcoholPermille * FATIGUE_PER_PERMILLE_SECOND);
		}

		private void ResetStatMonitors()
		{
			thirstMonitor.value = GameManager.GetThirstComponent().m_CurrentThirst;
			thirstMonitor.hourlyBaseline = GameManager.GetThirstComponent().m_ThirstIncreasePerDay / 24 * GameManager.GetExperienceModeManagerComponent().GetThirstRateScale();
			thirstMonitor.hourlyChange = 0;
			thirstMonitor.offset = 1;
			thirstMonitor.scale = 0.2f;
			thirstMonitor.debug = true;

			fatigueMonitor.value = GameManager.GetFatigueComponent().m_CurrentFatigue;
			fatigueMonitor.hourlyBaseline = GameManager.GetFatigueComponent().m_FatigueIncreasePerHourStanding * GameManager.GetExperienceModeManagerComponent().GetFatigueRateScale();
			fatigueMonitor.hourlyBaseline = 0;
			fatigueMonitor.hourlyChange = 0;
			fatigueMonitor.offset = 0;
			fatigueMonitor.scale = 1;
			fatigueMonitor.scale = GameManager.GetExperienceModeManagerComponent().GetFatigueRateScale();
		}

		private void UpdateStatMonitors(float elapsedGameSeconds, Thirst thirst, Fatigue fatigue)
		{
			thirstMonitor.Update(thirst.m_CurrentThirst, elapsedGameSeconds);
			fatigueMonitor.Update(fatigue.m_CurrentFatigue, elapsedGameSeconds);
		}
	}
}
