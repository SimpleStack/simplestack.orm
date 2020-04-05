namespace SimpleStack.Orm
{
    public interface ITableDefinition
    {
        string Name { get; }
        string SchemaName { get; }
    }

    public class TableDefinition : ITableDefinition
    {
        public string Name { get; set; }
        public string SchemaName { get; set; }
    }
}