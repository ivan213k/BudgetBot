using System.Collections.Generic;

namespace BudgetBot.Models.StateData
{
    public static class State
    {
        static Dictionary<long, CommandInfo> currentCommands = new Dictionary<long, CommandInfo>();

        public static string GetCurrentCommand(long userId)
        {
            if (currentCommands.TryGetValue(userId, out CommandInfo currentCommand))
            {
                return currentCommand.Name;
            }
            return null;
        }

        public static void AddCurrentCommand(long userId, string commandName)
        {
            if (!currentCommands.ContainsKey(userId))
            {
                currentCommands.Add(userId, new CommandInfo(commandName, currentStep:0));
            }
            else
            {
                currentCommands[userId] = new CommandInfo(commandName, currentStep: 0);
            }
        }

        public static void FinishCurrentCommand(long userId)
        {
            currentCommands.Remove(userId);
        }

        public static int GetCurrentStep(long userId)
        {
            return currentCommands[userId].CurrentStep;
        }

        public static void NextStep(long userId)
        {
            currentCommands[userId].CurrentStep = currentCommands[userId].CurrentStep + 1;
        }
    }
}
