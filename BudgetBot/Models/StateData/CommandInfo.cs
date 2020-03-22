namespace BudgetBot.Models.StateData
{
    public class CommandInfo
    {
        public string Name { get; set; }

        public int CurrentStep { get; set; }

        public CommandInfo(string name, int currentStep)
        {
            Name = name;
            CurrentStep = currentStep;
        }
    }
}
