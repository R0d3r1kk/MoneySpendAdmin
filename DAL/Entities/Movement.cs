using SQLite;
using System;

namespace MoneySpendAdmin.DAL.Entities
{
    [Table("Movement")]
    public class Movement
    {
        [PrimaryKey, Unique]
        public string id { get; set; }
        public DateTime fecha_creacion { get; set; }
        public string fecha { get; set; }
        public string concepto { get; set; }
        public decimal retiro { get; set; }
        public decimal deposito { get; set; }
        public decimal retiro_deposito { get; set; }
        public decimal saldo { get; set; }
    }
}
