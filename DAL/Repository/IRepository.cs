using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;

namespace MoneySpendAdmin.DAL.Entities
{
    public interface IRepository<T>
    {
        Task SaveAsync(T table);
        Task DeleteAsync(T table);
        Task<List<T>> GetAllAsync();
    }
}
