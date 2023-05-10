using Il2Cpp;
using MelonLoader;
using ModData;

namespace AlcoholMod
{
	internal class AlcoholMod : MelonMod
    {
		internal static AlcoholMod Instance { get; private set; }
		public override void OnInitializeMelon()
		{
			Instance = this;
			//Two sets of commands because different regions don't express this in the same unit
			uConsole.RegisterCommand("set_alcohol_permille", new Action(Console_SetAlcoholPermille));
			uConsole.RegisterCommand("set_alcohol_percent", new Action(Console_SetAlcoholPercent));
			uConsole.RegisterCommand("get_alcohol_permille", new Action(Console_GetAlcoholPermille));
			uConsole.RegisterCommand("get_alcohol_percent", new Action(Console_GetAlcoholPercent));
		}

        internal ModDataManager DataManager { get; } = new ModDataManager("AlcoholMod", false);
		internal AlcoholHealth? HealthManager { get; set; }
		internal void Load ()
		{
			MelonLogger.Msg("Loading AlcoholHealth");
			string? jsonText = DataManager.Load("health");
			if (jsonText == null)
			{
				HealthManager.Data = new AlcoholHealthData();
				return;
			}

			var dict = MelonLoader.TinyJSON.JSON.Load(jsonText) as MelonLoader.TinyJSON.ProxyObject;
			if (dict == null)
			{
				HealthManager.Data = new AlcoholHealthData();
				return;
			}

			dict.TryGetValue("alcoholPermille", out var alcoholPermille);
			dict.TryGetValue("uptakes", out var uptakesArr);
			List<AlcoholUptake> uptakes = new List<AlcoholUptake>();
			if (uptakesArr != null)
			{
				var array = uptakesArr as MelonLoader.TinyJSON.ProxyArray;
				if (array != null)
				foreach (MelonLoader.TinyJSON.ProxyObject item in array)
				{
					uptakes.Add(new AlcoholUptake(item["amountPerGameSecond"], item["remainingGameSeconds"]));
				}
			}
			var data = new AlcoholHealthData(uptakes, (float) alcoholPermille);
			data.alcoholPermille = Utils.NotNan(data.alcoholPermille, "Nan in ModHealthManager.SetData");
			HealthManager.Data = data;
		}

		internal void Save ()
		{
			MelonLogger.Msg("Saving AlcoholHealth");
			if (HealthManager?.Data == null) HealthManager.Data = new AlcoholHealthData();
			DataManager.Save(MelonLoader.TinyJSON.JSON.Dump(HealthManager.Data, MelonLoader.TinyJSON.EncodeOptions.NoTypeHints), "health");
		}

		#region ConsoleCommands

		private void Console_SetAlcoholPercent()
		{
			if (AlcoholMod.Instance?.HealthManager?.Data == null)
			{
				uConsole.Log("  alcohol system not initialiedd");
				return;
			} 
			if (uConsole.GetNumParameters() != 1)
			{
				uConsole.Log("  exactly one parameter required");
				return;
			}

			AlcoholMod.Instance.HealthManager.Data.alcoholPermille = uConsole.GetFloat() * 10f;
		}

		private void Console_SetAlcoholPermille()
		{
			if (AlcoholMod.Instance?.HealthManager?.Data == null)
			{
				uConsole.Log("  alcohol system not initialiedd");
				return;
			} 
			if (uConsole.GetNumParameters() != 1)
			{
				uConsole.Log("  exactly one parameter required");
				return;
			}

			AlcoholMod.Instance.HealthManager.Data.alcoholPermille = uConsole.GetFloat();
		}

		private void Console_GetAlcoholPercent()
		{
			if (AlcoholMod.Instance?.HealthManager?.Data == null)
			{
				uConsole.Log("  alcohol system not initialiedd");
				return;
			} 
			uConsole.Log(string.Format("  Current alcohol percent: {0}", AlcoholMod.Instance.HealthManager.Data.alcoholPermille / 10f));
		}

		private void Console_GetAlcoholPermille()
		{
			if (AlcoholMod.Instance?.HealthManager?.Data == null)
			{
				uConsole.Log("  alcohol system not initialiedd");
				return;
			} 
			uConsole.Log(string.Format("  Current alcohol permille: {0}", AlcoholMod.Instance.HealthManager.Data.alcoholPermille));
		}
		#endregion
	}
}
