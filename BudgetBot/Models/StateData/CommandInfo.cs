using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetBot.Models.StateData
{
    public class CommandInfo
    {
        public string Name { get; set; }

        public int CurrentStep { get; set; } = 0;

        public CommandInfo(string name, int currentStep)
        {
            Name = name;
            CurrentStep = currentStep;
        }
    }
}
