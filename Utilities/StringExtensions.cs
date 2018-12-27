using System.Text;

namespace ExchangeCleaner
{
    public static class StringExtensions
    {
        public static StringBuilder Reverse(this StringBuilder value)
        {
            StringBuilder reverse = new StringBuilder(value.Length);
            for (int i = value.Length - 1; i >= 0; i--)
            {
                reverse.Append(value[i]);
            }

            return reverse;
        }
    }
}
