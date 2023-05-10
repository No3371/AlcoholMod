namespace AlcoholMod
{
	public class AlcoholUptake
	{
		public float amountPerGameSecond;

		public float remainingGameSeconds;

		public AlcoholUptake (float amount, float gameSeconds)
		{
			amountPerGameSecond = amount / gameSeconds;
			remainingGameSeconds = gameSeconds;
		}
	}
}
