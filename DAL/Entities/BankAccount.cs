﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneySpendAdmin.DAL.Entities
{
    [Table("BankAccount")]
    public class BankAccount
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string direccion { get; set; }
        public string contrato { get; set; }
        public string sucursal { get; set; }
        public string cuenta_cheques { get; set; }
        public string tarjeta_Debito { get; set; }
        public string clabe_int { get; set; }
        public string ahorro_facil { get; set; }
        public string num_cliente { get; set; }
        public string rfc { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_corte { get; set; }
        public DateTime fecha_periodo_inicial { get; set; }
        public DateTime fecha_periodo_final { get; set; }
    }
}
