using System;

namespace SimpleStack.Orm
{
	public class OrmException : Exception
	{
		public OrmException(string message, Exception inner = null)
			:base(message,inner)
		{
			
		}
	}
}