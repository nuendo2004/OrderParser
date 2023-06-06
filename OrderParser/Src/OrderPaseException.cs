using System;
namespace OrderParser.Src
{
	public class OrderPaseException : Exception
	{
		public bool IsSuccess;
		public OrderPaseException(string message):base(message)
		{
		}
    }
}

