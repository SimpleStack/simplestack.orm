using System;

namespace SimpleStack.Orm.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class |
                    AttributeTargets.Struct)]
    public class StringLengthAttribute : Attribute
    {
        public StringLengthAttribute(int maximumLength)
        {
            MaximumLength = maximumLength;
        }

        public int MaximumLength { get; set; }
    }
}