using System.ComponentModel.DataAnnotations;

namespace BudgetBot.Models.DataBase
{
    public class Category
    {
        public Category()
        {

        }
        public Category(string name, bool isStandard, CategoryType type, Emoji image = null)
        {
            Name = name;
            Emoji = image?.ToString();
            IsStandardCategory = isStandard;
            CategoryType = type;
        }
        public Category(long userId, string name, CategoryType type, bool isStandard = false, Emoji image = null)
            : this(name, isStandard, type, image)
        {
            UserId = userId;
            IsStandardCategory = isStandard;
        }
        public int Id { get; set; }
        public long UserId { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsStandardCategory { get; set; }

        public CategoryType CategoryType { get; set; }

        public string Emoji { get; set; }
        public string GetImage()
        {
            return Emoji ?? "";
        }
    }
}
