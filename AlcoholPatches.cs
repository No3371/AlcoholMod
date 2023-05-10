using HarmonyLib;
using Il2Cpp;
using Il2CppVLB;
using MelonLoader;
using ModComponent.API;
using ModComponent.API.Components;
using System;
using UnityEngine;

namespace AlcoholMod
{
	internal static class AlcoholPatches
	{
		[HarmonyPatch(typeof(GearItem), nameof(GearItem.ApplyBuffs))]
		internal static class AlcoholComponentHook
		{
			public static void Postfix(GearItem __instance, float normalizedValue)
			{
				if (AlcoholMod.Instance?.HealthManager == null) return;
				ModFoodComponent modFood = __instance.GetComponent<ModFoodComponent>();
				if (modFood != null)
				{
					var amountTotal = modFood.WeightKG * modFood.AlcoholPercentage * 0.01f;
					float amountConsumed = amountTotal * normalizedValue;
					AlcoholMod.Instance.HealthManager.DrankAlcohol(amountConsumed, modFood.AlcoholUptakeMinutes * 60);
				}
			}
		}

		
		[HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.RestoreGlobalData))]
		internal static class LoadModSavedData
		{
			public static void Postfix()
			{
				MelonLogger.Msg("--------------------SaveGameSystem.RestoreGlobalData");
				AlcoholMod.Instance.Load();
			}
		}

		[HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData))]
		internal static class SaveModData
		{
			public static void Postfix()
			{
				MelonLogger.Msg("--------------------SaveGameSystem.SaveGlobalData");
				AlcoholMod.Instance.Save();
			}
		}

		[HarmonyPatch(typeof(StatusBar), nameof(StatusBar.GetRateOfChange))] 
		internal static class StatusBar_GetRateOfChange
		{
			private static void Postfix(StatusBar __instance, ref float __result)
			{
				if (AlcoholMod.Instance?.HealthManager == null) return;
				if (__instance.m_StatusBarType == StatusBar.StatusBarType.Fatigue)
				{
					var fatigueMonitor = AlcoholMod.Instance.HealthManager.FatigueMonitor;
					__result = fatigueMonitor.GetRateOfChange();
				}
				else if (__instance.m_StatusBarType == StatusBar.StatusBarType.Thirst)
				{
					var thirstMonitor = AlcoholMod.Instance.HealthManager.ThirstMonitor;
					__result = thirstMonitor.GetRateOfChange();
				}
			}
		}

		[HarmonyPatch(typeof(Condition), nameof(Condition.UpdateBlurEffect))] 
		internal static class Condition_UpdateBlurEffect
		{
			public static void Prefix(Condition __instance, ref float percentCondition, ref bool lowHealthStagger)
			{
				if (AlcoholMod.Instance?.HealthManager?.Data != null)
				{
					bool shouldStagger = AlcoholMod.Instance.HealthManager.Data.alcoholPermille >= AlcoholHealth.MIN_PERMILLE_FOR_STAGGERING;
					lowHealthStagger = percentCondition <= __instance.m_HPToStartBlur || shouldStagger;
					percentCondition = Math.Min(percentCondition, __instance.m_HPToStartBlur * (1 - AlcoholMod.Instance.HealthManager.GetAlcoholBlurValue()) + 0.01f);

					if (!lowHealthStagger)
					{
						GameManager.GetVpFPSCamera().m_BasePitch = Mathf.Lerp(GameManager.GetVpFPSCamera().m_BasePitch, 0.0f, 0.01f);
						GameManager.GetVpFPSCamera().m_BaseRoll = Mathf.Lerp(GameManager.GetVpFPSCamera().m_BaseRoll, 0.0f, 0.01f);
					}
				}
			}
		}

		[HarmonyPatch(typeof(Freezing), nameof(Freezing.CalculateBodyTemperature))]
		internal static class Freezing_CalculateBodyTemperature
		{
			public static void Postfix(ref float __result)
			{
				if (AlcoholMod.Instance?.HealthManager?.Data == null) return;
				__result += AlcoholMod.Instance.HealthManager.Data.alcoholPermille * AlcoholHealth.BODY_TEMP_BONUS_PER_PERMILLE;
			}
		}

		[HarmonyPatch(typeof(Frostbite), nameof(Frostbite.CalculateBodyTemperatureWithoutClothing))]
		internal static class Frostbite_CalculateBodyTemperatureWithoutClothing
		{
			public static void Postfix(ref float __result)
			{
				if (AlcoholMod.Instance?.HealthManager?.Data == null) return;
				__result += AlcoholMod.Instance.HealthManager.Data.alcoholPermille * AlcoholHealth.FROSTBITE_TEMP_BONUS_PER_PERMILLE;
			}
		}

		[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Start))]
		internal static class PlayerManagerStartPatch
		{
			public static void Postfix(PlayerManager __instance)
			{
				AlcoholMod.Instance.HealthManager = __instance.gameObject.GetOrAddComponent<AlcoholHealth>();
			}
		}
	}
}
