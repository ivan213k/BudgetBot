namespace BudgetBot.Models.Statistics
{
    public class ExpenseStatistic
    {
        public string Categrory { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Percent { get; set; }

        public ExpenseStatistic(string categry, decimal totalAmount, decimal percent)
        {
            Categrory = categry;
            TotalAmount = totalAmount;
            Percent = percent;
        }
    }
}
