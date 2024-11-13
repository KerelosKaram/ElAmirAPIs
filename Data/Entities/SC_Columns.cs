namespace ElAmir.Data.Entities
{
    public class SC_Columns : BaseEntity
    {
        public int SC_Customer_ID { get; set; }
        public string? Group_name { get; set; }
        public string? Column_Name { get; set; }
        public string? List { get; set; }
        public int Type { get; set; }
        public bool Allow_null { get; set; }
        public string Insert_User { get; set; }  = "user";
        public DateTime Insert_Date { get; set; } = DateTime.Now;
        public bool Delete_Flag { get; set; }
        public int Sort_by { get; set; }
        public int CalcColumn { get; set; }
    }
}
