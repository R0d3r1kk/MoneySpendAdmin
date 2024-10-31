using SQLite;
using System;

namespace MoneySpendAdmin.DAL.Entities
{
    [Table("Operation")]
    public class Operation
    {
        [PrimaryKey, Unique]
        public string id { get; set; }
        public DateTime fecha_creacion { get; set; }
        public string concepto { get; set; }
        public string tipo { get; set; }
        public int consumos { get; set; }
        public decimal importe { get; set; }
    }
}
