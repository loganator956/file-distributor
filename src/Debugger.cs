namespace file_distributor.Debugging
{
    internal static class Debugger
    {
        public static void PrintInColour(string message, ConsoleColor colour)
        {
            ConsoleColor prevColour = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ForegroundColor = prevColour;
        }
    }
}