using MoneySpendAdmin.DAL;
using MoneySpendAdmin.DAL.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace MoneySpendAdmin.Shared
{
    public class PDFModel
    {
        public struct PageData
        {
            public string nombre { get; set; }
            public string direccion { get; set; }
            public string periodo { get; set; }
            public string contrato { get; set; }
            public string sucursal { get; set; }
            public string cuenta_cheques { get; set; }
            public string tarjeta_Debito { get; set; }
            public string clabe_int { get; set; }
            public string ahorro_facil { get; set; }
            public string num_cliente { get; set; }
            public string rfc { get; set; }
            public string corte { get; set; }
            public string interes { get; set; }
            public string saldo_anterior { get; set; }
            public string depositos { get; set; }
            public string retiros { get; set; }
            public string otros_cargos { get; set; }
            public string saldo_corte { get; set; }
            public string sald_promedio { get; set; }
        }

        public List<string> pages;
        public PageData data;
        public TextRepository txtRepo;

        public PDFModel(IDataAccess da, List<string> pages)
        {
            this.pages = pages;
            txtRepo = new TextRepository(da);
        }
        private List<string> GetPageLines(string text)
        {
            var lines = text.Replace("\r", "").Split('\n').ToList();
            return lines;
        }

        public void formatPageLines()
        {
            int pageIndex = 1;
            foreach (var page in pages)
            {
                var lines = GetPageLines(page);
                switch (pageIndex)
                {
                    case 1:
                        data = new PageData()
                        {
                            nombre = lines[4],
                            direccion = $"{lines[5]} {lines[6]} {lines[7]}",
                            periodo = findConcidence("Período del", lines),
                            corte = findConcidence("Fecha de Corte", lines),
                            interes = findConcidence("Interés aplicable o Rendimientos", lines),
                            contrato = findConcidence("Número de contrato", lines),
                            sucursal = findConcidence("Número de sucursal", lines),
                            cuenta_cheques = findConcidence("Número de cuenta de cheques", lines),
                            tarjeta_Debito = findConcidence("Número de Tarjeta de Débito", lines),
                            clabe_int = findConcidence("CLABE Interbancaria", lines),
                            ahorro_facil = findConcidence("AHORRO FACIL", lines),
                            num_cliente = findConcidence("Número de cliente", lines),
                            rfc = findConcidence("RFC", lines),
                            saldo_anterior = findConcidence("Saldo anterior", lines),
                            depositos = findConcidence("Depósitos", lines),
                            retiros = findConcidence("Retiros en efectivo", lines),
                            otros_cargos = findConcidence("Otros cargos", lines),
                            saldo_corte = findConcidence("Saldo al corte", lines),
                            sald_promedio = findConcidence("Saldo promedio mensual", lines)
                        };

                        break;
                }
                pageIndex++;
            }
        }

        public string findConcidence(string key, List<string> lines)
        {
            var text = lines.FirstOrDefault(txt => txt.ToLower().StartsWith(key.ToLower()));
            if (text != null)
                return text.Replace(key, "").Trim();
            else
                return null;
        }

        public async void savePages(string path, string name)
        {
            int pageIndex = 1;
            foreach (var page in pages)
            {
                //File.WriteAllText(path.Replace(".pdf", $"_Page-{pageIndex}.txt"), page);
                await txtRepo.SaveAsync(new DAL.Entities.TextExtraction
                {
                    fecha_creacion = DateTime.Now,
                    line_count = GetPageLines(page).Count,
                    lines = page,
                    path = path,
                    filename = name,
                    mes = DateTime.Parse(data.corte).ToString("MMMM"),
                    año = DateTime.Parse(data.corte).Year
                });
                pageIndex++;
            }
        }
    }
}
