namespace BudgetBot.Models.Statistics
{
    public class RevenueStatistic
    {
        public string Categrory { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Percent { get; set; }

        public RevenueStatistic(string categry, decimal totalAmount, decimal percent)
        {
            Categrory = categry;
            TotalAmount = totalAmount;
            Percent = percent;
        }
    }
}
