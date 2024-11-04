using SQLite;
using System;

namespace MoneySpendAdmin.DAL.Entities
{
    [Table("Balance")]
    public class Balance
    {
        [PrimaryKey, Unique]
        private string id { get; set; }
        public string mes { get; set; }
        public decimal saldo_anterior { get; set; }
        public decimal depositos { get; set; }
        public decimal retiros { get; set; }
        public decimal otros_cargos { get; set; }
        public decimal saldo_corte { get; set; }
        public decimal saldo_promedio { get; set; }
        private DateTime fecha_creacion { get; set; }


        public string getId()
        {
            return id;
        }

        public void setId(string Id)
        {
            this.id = Id;
        }

        public DateTime getDate()
        {
            return fecha_creacion;
        }

        public void setDate(DateTime d)
        {
            this.fecha_creacion = d;
        }
    }
}
