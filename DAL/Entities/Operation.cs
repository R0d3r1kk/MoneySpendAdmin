using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneySpendAdmin.DAL.Entities
{
    [Table("Operation")]
    public class Operation
    {
        public int id { get; set; }
        public DateTime fecha_creacion { get; set; }
        public string concepto { get; set; }
        public string tipo { get; set; }
        public int consumos { get; set; }
        public decimal importe { get; set; }
    }
}
