namespace SimpleStack.Orm.PostgreSQL
{
    public class ColumnInformationSchema
    {
        public string table_catalog { get; set; }
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string column_name { get; set; }
        public int ordinal_position { get; set; }
        public string column_default { get; set; }
        public string is_nullable { get; set; }
        public string data_type { get; set; }
        public int character_maximum_length { get; set; }
        public int character_octet_length { get; set; }
        public int numeric_precision { get; set; }
        public int numeric_precision_radix { get; set; }
        public int numeric_scale { get; set; }
        public int datetime_precision { get; set; }
        public string interval_type { get; set; }
        public int interval_precision { get; set; }
        public string character_set_catalog { get; set; }
        public string character_set_schema { get; set; }
        public string character_set_name { get; set; }
        public string collation_catalog { get; set; }
        public string collation_schema { get; set; }
        public string collation_name { get; set; }
        public string domain_catalog { get; set; }
        public string domain_schema { get; set; }
        public string domain_name { get; set; }
        public string udt_catalog { get; set; }
        public string udt_schema { get; set; }
        public string udt_name { get; set; }
        public string scope_catalog { get; set; }
        public string scope_schema { get; set; }
        public string scope_name { get; set; }
        public int maximum_cardinality { get; set; }
        public string dtd_identifier { get; set; }
        public string is_self_referencing { get; set; }
        public string is_identity { get; set; }
        public string identity_generation { get; set; }
        public string identity_start { get; set; }
        public string identity_increment { get; set; }
        public string identity_maximum { get; set; }
        public string identity_minimum { get; set; }
        public string identity_cycle { get; set; }
        public string is_generated { get; set; }
        public string generation_expression { get; set; }
        public string is_updatable { get; set; }
    }
}