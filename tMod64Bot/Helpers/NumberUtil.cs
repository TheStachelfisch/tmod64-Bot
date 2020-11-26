namespace tMod64Bot.Helpers
{
    public static class NumberUtil
    {
        public static int LastDigit(this int number) => (number % 10);

        // No idea what I should call this
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
                case 4:
                    return "th";
                case 5:
                    return "th";
                case 6:
                    return "th";
                case 7:
                    return "th";
                case 8:
                    return "th";
                case 9:
                    return "th";
            }

            return "";
        }
    }
}