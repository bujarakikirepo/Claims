namespace Application.ComputePremium
{
    public interface IComputePremium
    {
        public decimal Compute(DateOnly startDate, DateOnly endDate);
    }
}
