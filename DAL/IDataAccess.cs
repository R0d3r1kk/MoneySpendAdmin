using System.Threading.Tasks;
using SQLite;

namespace MoneySpendAdmin.DAL
{
    public interface IDataAccess
    {
        Task Initialize();
        SQLiteAsyncConnection GetAsyncConnection();
    }
}
