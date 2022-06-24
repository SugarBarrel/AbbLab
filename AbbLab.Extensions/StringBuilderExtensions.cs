using System.Text;
using JetBrains.Annotations;

namespace AbbLab.Extensions
{
    public static class StringBuilderExtensions
    {
        [Pure] public static string ToStringAndClear(this StringBuilder sb)
        {
            string res = sb.ToString();
            sb.Clear();
            return res;
        }
    }
}
