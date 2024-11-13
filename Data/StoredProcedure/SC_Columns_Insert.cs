namespace ElAmir.Data.StoredProcedure
{
    public class SC_Columns_Insert : BaseEntity
    {
        public string? Column_Name { get; set; }
        public int Type { get; set; }
        public bool Allow_null { get; set; }
        public string? Insert_User { get; set; }
    }
}