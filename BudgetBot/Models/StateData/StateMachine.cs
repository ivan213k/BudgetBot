using System.Collections.Generic;

namespace BudgetBot.Models.StateData
{
    public static class StateMachine
    {
        private static readonly Dictionary<long, CommandInfo> CurrentCommands = new Dictionary<long, CommandInfo>();

        public static string GetCurrentCommand(long userId)
        {
            return CurrentCommands.TryGetValue(userId, out CommandInfo currentCommand) ? currentCommand.Name : null;
        }

        public static void AddCurrentCommand(long userId, string commandName)
        {
            if (!CurrentCommands.ContainsKey(userId))
            {
                CurrentCommands.Add(userId, new CommandInfo(commandName, currentStep:0));
            }
            else
            {
                CurrentCommands[userId] = new CommandInfo(commandName, currentStep: 0);
            }
        }

        public static void FinishCurrentCommand(long userId)
        {
            CurrentCommands.Remove(userId);
        }

        public static int GetCurrentStep(long userId)
        {
            return CurrentCommands[userId].CurrentStep;
        }

        public static void NextStep(long userId)
        {
            CurrentCommands[userId].CurrentStep += 1;
        }
        public static void PreviousStep(long userId)
        {
            CurrentCommands[userId].CurrentStep -= 1;
        }
    }
}
