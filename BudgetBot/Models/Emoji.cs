using System;

namespace BudgetBot.Models
{
    public class Emoji
    {
        readonly int code;
        public Emoji(int code)
        {
            this.code = code;
        }

        public override string ToString()
        {
            return Char.ConvertFromUtf32(code);
        }
    }
}
