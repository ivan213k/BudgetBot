namespace BudgetBot.Models
{
    public class Category
    {
        private Emoji emoji;
        public Category(string name, bool isStandard, Emoji image=null)
        {
            Name = name;
            emoji = image;
            IsStandardCategory = isStandard;
        }
        public Category(long userId, string name, bool isStandard = false, Emoji image = null)
            :this(name,isStandard,image)
        {
            UserId = userId;
            IsStandardCategory = isStandard;
        }
        public long UserId { get; set; }

        public string Name { get; set; }

        public bool IsStandardCategory { get; set; }

        public CategoryType CategoryType { get; set; }
        public string GetImage()
        {
            if (emoji!=null)
            {
                return emoji.ToString();
            }
            return "";
        }
    }
}
