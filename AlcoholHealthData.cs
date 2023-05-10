using System.Collections.Generic;

namespace AlcoholMod
{
	internal class AlcoholHealthData
	{
		public float alcoholPermille;
		public IList<AlcoholUptake> uptakes;

        public AlcoholHealthData()
        {
            this.uptakes = new List<AlcoholUptake>();
        }

        public AlcoholHealthData(IList<AlcoholUptake> uptakes, float alcoholPermille)
        {
            this.uptakes = uptakes;
            this.alcoholPermille = alcoholPermille;
        }

        public AlcoholHealthData(float alcoholPermille, IList<AlcoholUptake> uptakes)
        {
            this.alcoholPermille = alcoholPermille;
            this.uptakes = uptakes;
        }
    }
}
