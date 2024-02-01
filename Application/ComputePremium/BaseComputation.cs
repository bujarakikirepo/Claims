namespace Application.ComputePremium
{
    public abstract class BaseComputation
    {
        protected const decimal BasePremiumPerDay = 1250m;
        //force derived class has it's own Multiplier
        public abstract decimal Multiplier { get; }

        //default discount implementation 
        protected virtual decimal CalculateDailyPremium(int dayIndex, decimal premiumPerDay)
        {
            var discount = 0m; //there is no discount first 30 days
            if (dayIndex > 30 && dayIndex < 180)
            {
                discount = 0.02m;
            }
            else if (dayIndex >= 180)
            {
                discount = 0.01m;
            }
            return premiumPerDay - premiumPerDay * discount;
        }
    }
}
