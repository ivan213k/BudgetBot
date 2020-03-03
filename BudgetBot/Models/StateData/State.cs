using System.Collections.Generic;

namespace BudgetBot.Models.StateData
{
    public static class State
    {
        static Dictionary<long, string> currentCommands = new Dictionary<long, string>();

        public static string GetCurrentCommand(long userId)
        {
            if (currentCommands.TryGetValue(userId, out string currentCommand))
            {
                return currentCommand;
            }
            return null;
        }

        public static void AddCurrentCommand(long userId, string commandName)
        {
            if (!currentCommands.ContainsKey(userId))
            {
                currentCommands.Add(userId, commandName);
            }
            else
            {
                currentCommands[userId] = commandName;
            }
        }

        public static void FinishCurrentCommand(long userId)
        {
            currentCommands.Remove(userId);
        }
    }
}
