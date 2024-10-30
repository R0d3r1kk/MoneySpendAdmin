using SQLite;
using System;

namespace MoneySpendAdmin.DAL.Entities
{
    [Table("TextExtraction")]
    public class TextExtraction
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int id { get; set; }
        public string path { get; set; }
        public string filename { get; set; }
        public int line_count { get; set; }
        public string lines { get; set; }
        public DateTime fecha_creacion { get; set; }
    }
}
