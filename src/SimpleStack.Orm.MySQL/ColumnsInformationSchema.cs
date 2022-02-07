namespace SimpleStack.Orm.MySQL
{
    internal class InformationSchema
    {
        public string TABLE_CATALOG { get; set; }
        public string TABLE_SCHEMA { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public int ORDINAL_POSITION { get; set; }
        public string COLUMN_DEFAULT { get; set; }
        public string IS_NULLABLE { get; set; }
        public string DATA_TYPE { get; set; }
        public int? CHARACTER_MAXIMUM_LENGTH { get; set; }
        public int? CHARACTER_OCTET_LENGTH { get; set; }
        public int? NUMERIC_PRECISION { get; set; }
        public int? NUMERIC_SCALE { get; set; }
        public int? DATETIME_PRECISION { get; set; }
        public string CHARCTER_SET_NAME { get; set; }
        public string COLLATION_NAME { get; set; }
        public string COLUMN_TYPE { get; set; }
        public string COLUMN_KEY { get; set; }
        public string EXTRA { get; set; }
        public string GENERATION_EXPRESSION { get; set; }
    }
}