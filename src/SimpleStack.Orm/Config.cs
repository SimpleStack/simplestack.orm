namespace SimpleStack.Orm
{
    public static class Config
    {
        static Config()
        {
            IdField = "Id";
        }

        public static string IdField { get; set; }
    }
}