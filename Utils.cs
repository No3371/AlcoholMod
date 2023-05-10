using MelonLoader;

namespace AlcoholMod
{
    internal static class Utils
    {
        internal static float NotNan(float number, string message = "")
        {
            if (float.IsNaN(number))
            {
                if (!string.IsNullOrEmpty(message)) MelonLogger.Error(message);
                else MelonLogger.Error("Nan value found in ModHealthManager");
                return 0;
            }
            else return number;
        }
    }
}