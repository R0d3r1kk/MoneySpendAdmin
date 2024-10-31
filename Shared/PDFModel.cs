using MoneySpendAdmin.DAL;
using MoneySpendAdmin.DAL.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoneySpendAdmin.Shared
{
    public class PDFModel
    {
        public class PageData
        {
            public string nombre { get; set; }
            public string direccion { get; set; }
            public string periodo { get; set; }
            public string contrato { get; set; }
            public string sucursal { get; set; }
            public string cuenta_cheques { get; set; }
            public string tarjeta_debito { get; set; }
            public string tarjeta_credito { get; set; }
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

        public class PageTransaction
        {
            public string fecha { get; set; }
            public string concepto { get; set; }
            public string retirosOdepositos { get; set; }
            public string retiros { get; set; }
            public string depositos { get; set; }
            public string saldo { get; set; }
            public int page_index { get; set; }
        }

        public List<string> pages;
        public List<PageTransaction> transactions;
        public PageData data;
        public TextRepository txtRepo;
        public MovementRepository moveRepo;
        public BankAccountRepository bankRepo;
        public BalanceRepository balRepo;

        public PDFModel(IDataAccess da, List<string> pages)
        {
            this.pages = pages;
            this.transactions = new List<PageTransaction>();
            this.txtRepo = new TextRepository(da);
            this.moveRepo = new MovementRepository(da);
            this.bankRepo = new BankAccountRepository(da);
            this.balRepo = new BalanceRepository(da);
        }
        private List<string> GetPageLines(string text)
        {
            var lines = text.Replace("\r", "").Split('\n').ToList();
            return lines;
        }

        public async Task formatPageLines(string path, string name)
        {
            int pageIndex = 1;
            string year = string.Empty;
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
                            tarjeta_debito = findConcidence("Número de Tarjeta de Débito", lines),
                            tarjeta_credito = findConcidence("Número de Tarjeta de Crédito", lines),
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
                        await savePages(path, name);
                        var ps = data.periodo.Split("al");
                        var dia_mesT_inicial = ps[0].Split("de");
                        var fs = ps[1].Split("del");
                        var dia_mesT_final = fs[0].Split("de");
                        var año = fs[1];
                        year = año;

                        var per_inicial = $"{dia_mesT_inicial[0]}/{dia_mesT_inicial[1]}/{año}";
                        var per_final = $"{dia_mesT_final[0]}/{dia_mesT_final[1]}/{año}";


                        await bankRepo.SaveAsync(new DAL.Entities.BankAccount()
                        {
                            nombre = data.nombre,
                            ahorro_facil = data.ahorro_facil,
                            clabe_int = data.clabe_int,
                            contrato = data.contrato,
                            cuenta_cheques = data.cuenta_cheques,
                            direccion = data.direccion,
                            fecha_corte = DateTime.Parse(data.corte),
                            fecha_periodo_final = DateTime.Parse(per_final),
                            fecha_periodo_inicial = DateTime.Parse(per_inicial),
                            num_cliente = data.num_cliente,
                            rfc = data.rfc,
                            sucursal = data.sucursal,
                            tarjeta_debito = data.tarjeta_debito,
                            tarjeta_credito = data.tarjeta_credito,
                        });
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        var trans = findPageTransactions(lines, pageIndex);
                        this.transactions.AddRange(trans);
                        await saveTransactions(path, name, year);
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

        public List<PageTransaction> findPageTransactions(List<string> lines, int pageIdx)
        {
            var tDatePattern = @"^(\d{1,2}) ([A-Z]{1,3})$"; //24 SEP
            var tCurrencyPattern = @"[^$]\b\d+\.\d{2}\b"; //32834.00

            Regex rgdate = new Regex(tDatePattern);
            Regex rgcurrency = new Regex(tCurrencyPattern);

            //Se obtienen los ids de las filas de la columan inicial de la tabla en el texto
            var line_count = 0;
            var rows = new List<PageTransaction>();
            PageTransaction row = null;
            foreach (var line in lines)
            {

                if (rgdate.IsMatch(line))
                {
                    row = new PageTransaction();
                    row.fecha = line;
                    row.page_index = pageIdx;
                }
                else
                {
                    if(row != null)
                    {
                        //Si es la ultima linea de la transaccion
                        if (rgcurrency.IsMatch(line))
                        {
                            var words = rgcurrency.Matches(line).ToList();
                            if(words.Count > 2)
                            {
                                row.retirosOdepositos = "0";
                                row.retiros = words[0].Value;
                                row.depositos = words[1].Value;
                                row.saldo = words[2].Value;
                            }else if(words.Count == 1)
                            {
                                row.retiros = "0";
                                row.depositos = "0";
                                row.retirosOdepositos = "0";
                                row.saldo = words[0].Value;
                            }
                            else
                            {
                                row.retiros = "0";
                                row.depositos = "0";
                                row.retirosOdepositos = words[0].Value;
                                row.saldo = words[1].Value;
                            }
                            var fline = rgcurrency.Replace(line, "");
                            row.concepto = fline;
                            rows.Add(row);
                            row = null;
                        }
                        else
                        {
                            row.concepto += line;
                        }
                    }
                }

                line_count++;
            }

            return rows;
        }

        private async Task savePages(string path, string name)
        {
            int pageIndex = 1;
            foreach (var page in pages)
            {
                //File.WriteAllText(path.Replace(".pdf", $"_Page-{pageIndex}.txt"), page);
                await txtRepo.SaveAsync(new DAL.Entities.TextExtraction
                {
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

        private async Task saveTransactions(string path, string name, string year)
        {
            int pageIndex = 1;
            foreach (var tr in transactions)
            {
                //File.WriteAllText(path.Replace(".pdf", $"_Page-{pageIndex}.txt"), page);
                tr.saldo = trimValue(tr.saldo);
                tr.retirosOdepositos = trimValue(tr.retirosOdepositos);
                await moveRepo.SaveAsync(new DAL.Entities.Movement
                {
                    concepto = tr.concepto,
                    deposito = stringToDecimal(tr.depositos),
                    fecha = $"{tr.fecha}{year}",
                    retiro = stringToDecimal(tr.retiros),
                    saldo = stringToDecimal(tr.saldo),
                    retiro_deposito = stringToDecimal(tr.retirosOdepositos)
                });
                pageIndex++;
            }
        }

        private decimal stringToDecimal(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return 0;
            }
            else
            {
                var dec = decimal.Parse(val);
                return dec;
            }
        }

        private string trimValue(string val)
        {
            return val.TrimStart([',', '.']).Trim();
        }
    }
}
