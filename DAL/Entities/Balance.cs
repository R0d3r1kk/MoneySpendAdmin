using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneySpendAdmin.DAL.Entities
{
    [Table("Balance")]
    public class Balance
    {
        public int id { get; set; }
        public decimal saldo_anterior { get; set; }
        public decimal depositos { get; set; }
        public decimal retiros { get; set; }
        public decimal otros_cargos { get; set; }
        public decimal saldo_corte { get; set; }
        public decimal saldo_promedio { get; set; }
        public DateTime fecha_creacion { get; set; }
    }
}
