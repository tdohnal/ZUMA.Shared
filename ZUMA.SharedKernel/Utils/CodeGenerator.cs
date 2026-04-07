namespace ZUMA.SharedKernel.Utils
{
    public class CodeGenerator
    {
        public static string GenerateNumericCode(int length = 12)
        {
            Random random = new Random();
            char[] digits = new char[length];
            for (int i = 0; i < length; i++)
            {
                digits[i] = (char)('0' + random.Next(0, 10));
            }
            return new string(digits);
        }
    }
}
