using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneySpendAdmin.DAL.Entities
{
    [Table("Movement")]
    public class Movement
    {
        public int id { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha { get; set; }
        public string concepto { get; set; }
        public decimal retiro { get; set; }
        public decimal deposito { get; set; }
        public decimal saldo { get; set; }
    }
}
