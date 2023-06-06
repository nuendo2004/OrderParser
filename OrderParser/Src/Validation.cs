using System;
namespace OrderParser.Src
{
	public class Validation
	{
        public int ParseNumber(string substring, int lineNumber, string type)
        {
            int number;
            if (!int.TryParse(substring.TrimStart().TrimEnd(), out number))
                throw new OrderPaseException($"Incorrect {type} number at line {lineNumber}.");

            return number;
        }
        public decimal ParsePrice(string substring, int lineNumber, string type)
        {
            decimal price;
            if (!decimal.TryParse(substring.TrimStart().TrimEnd(), out price))
                throw new OrderPaseException($"Incorrect {type} value at line {lineNumber}.");

            return decimal.Parse(price.ToString("N2"));
        }
        public string ParseString(string substring, int lineNumber, string type)
        {
            string res = substring.TrimStart().TrimEnd();

            if (res.Length == 0)
                throw new OrderPaseException($"{type} can not be empty at line {lineNumber}.");
            return res;
        }
        public DateTime ParseDate(string substring, int lineNumber, string type)
        {
            DateTime date;
            if (DateTime.TryParse(substring, out date) == false)
                throw new OrderPaseException($"Incorrect {type} format at line {lineNumber}.");
            return date;
        }
        public bool ParseBool(string substring, int lineNumber, string type)
        {
            if (!(substring[0] == '1' || substring[0] == '0'))
                throw new OrderPaseException($"{type} value must be 1 or 0 at line {lineNumber}.");
            bool res = int.Parse(substring) != 0;
            return res;
        }
    }
}

