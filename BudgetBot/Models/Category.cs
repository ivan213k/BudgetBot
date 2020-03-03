using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBot.Models
{
    public class Category
    {
        public Category(string name, bool isStandard, string imageUrl=null)
        {
            Name = name;
            ImageUrl = imageUrl;
            IsStandardCategory = isStandard;
        }
        public Category(long userId, string name, bool isStandard = false, string imageUrl = null)
            :this(name,isStandard,imageUrl)
        {
            UserId = userId;
            IsStandardCategory = isStandard;
        }
        public long UserId { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public bool IsStandardCategory { get; set; } 
    }
}
