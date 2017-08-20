namespace OddsWebsite.Helpers
{
    public static class CalculationHelper
    {
        public static double CalculateKellyCriterionPercentage(double odd, double successRate)
        {
            return (successRate * odd - 1) / (odd - 1);
        }
    }
}
