using MoneySpendAdmin.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneySpendAdmin.Shared
{
    public class Global
    {
        public IDataAccess dataAccess {  get; set; }
    }
}
