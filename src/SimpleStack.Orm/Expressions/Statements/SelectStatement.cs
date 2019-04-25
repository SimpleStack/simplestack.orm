using System;
using System.Text;

namespace SimpleStack.Orm.Expressions.Statements
{
    public class SelectStatement : CountStatement
    {
        private int? _maxRows;
        private int? _offset;
        internal StringBuilder OrderByExpression { get; } = new StringBuilder();

        internal int? Offset
        {
            get => _offset;
            set
            {
                if (value < 0)
                    throw new ArgumentException($"Offset value:'{value.ToString()}' must be>=0");
                _offset = value;
            }
        }

        internal int? MaxRows
        {
            get => _maxRows;
            set
            {
                if (value < 0)
                    throw new ArgumentException($"MaxRows value:'{value.ToString()}' must be>=0");
                _maxRows = value;
            }
        }

        public override void Clear()
        {
            base.Clear();

            OrderByExpression.Clear();
            Offset = null;
            MaxRows = null;
        }
    }
}