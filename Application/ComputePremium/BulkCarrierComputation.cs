namespace Application.ComputePremium
{
    public class BulkCarrierComputation : BaseComputation, IComputePremium
    {
        public override decimal Multiplier => 1.3m;

        public decimal Compute(DateOnly startDate, DateOnly endDate)
        {
            var premiumPerDay = BasePremiumPerDay * Multiplier;
            var insuranceLength = endDate.DayNumber - startDate.DayNumber;
            var totalPremium = 0m;

            for (var i = 0; i < insuranceLength; i++)
            {
                totalPremium += base.CalculateDailyPremium(i, premiumPerDay);
            }

            return totalPremium;
        }
    }
}
