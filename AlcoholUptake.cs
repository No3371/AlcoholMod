namespace AlcoholMod
{
	public struct AlcoholUptake
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
