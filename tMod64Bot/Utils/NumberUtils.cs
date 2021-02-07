namespace tMod64Bot.Utils
{
    public static class NumberUtils
    {
        public static int LastDigit(this int number) => (number % 10);
        
        public static string NumberEnding(this int number)
        {
            switch (number.LastDigit())
            {
                case 0:
                    return "th";
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }
    }
}