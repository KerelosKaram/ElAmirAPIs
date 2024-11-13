using System.ComponentModel.DataAnnotations;

namespace ElAmir.Data.Entities
{
    public class SC_Visit_Pic : BaseEntity
    {
        [Key]
        public int SC_Visit_Pic_ID { get; set; }
        public int SC_Visit_header_ID { get; set; }
        public string Pic_Name { get; set; }
        public string Insert_User { get; set; }
        public DateTime? Insert_Date { get; set; }
        public bool Delete_Flag { get; set; }
    }
}