namespace BudgetBot.Models.Statistics
{
    public class RevenueStatistic
    {
        public string Category { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Percent { get; set; }

        public RevenueStatistic(string category, decimal totalAmount, decimal percent)
        {
            Category = category;
            TotalAmount = totalAmount;
            Percent = percent;
        }
    }
}
