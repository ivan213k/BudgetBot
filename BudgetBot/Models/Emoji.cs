namespace BudgetBot.Models
{
    public class Emoji
    {
        private readonly int _code;
        public Emoji(int code)
        {
            _code = code;
        }

        public override string ToString()
        {
            return char.ConvertFromUtf32(_code);
        }
    }
}
