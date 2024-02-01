namespace Application.ComputePremium
{
    public class YachtComputation : BaseComputation, IComputePremium
    {
        public override decimal Multiplier { get => 1.1m; }
        public decimal Compute(DateOnly startDate, DateOnly endDate)
        {
            var premiumPerDay = BasePremiumPerDay * Multiplier;
            var insuranceLength = endDate.DayNumber - startDate.DayNumber;
            var totalPremium = 0m;

            for (var i = 0; i < insuranceLength; i++)
            {
                totalPremium += CalculateDailyPremium(i, premiumPerDay);
            }

            return totalPremium;
        }

        protected override decimal CalculateDailyPremium(int dayIndex, decimal premiumPerDay)
        {
            var discount = 0m;
            if (dayIndex > 30 && dayIndex < 180)
            {
                discount = 0.05m;
            }
            else if (dayIndex >= 180)
            {
                discount = 0.03m;
            }
            return premiumPerDay - premiumPerDay * discount;
        }
    }
}
