using MoneySpendAdmin.DAL.Entities;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneySpendAdmin.DAL.Repository
{
    public class BalanceRepository : IRepository<Balance>
    {
        private readonly SQLiteAsyncConnection _db;

        public BalanceRepository(IDataAccess dal)
        {
            _db = dal.GetAsyncConnection();
        }

        public async Task DeleteAsync(Balance balance)
        {
            await _db.DeleteAsync(balance);
        }

        public async Task<List<Balance>> GetAllAsync()
        {
            var ba = await _db.Table<Balance>().ToListAsync();
            return ba;
        }

        public async Task SaveAsync(Balance balance)
        {
            var ba = await _db.Table<Balance>().FirstOrDefaultAsync(b => b.id == balance.id);
            if (ba == null)
            {
                await _db.InsertAsync(balance);
            }
            else
            {
                await _db.UpdateAsync(balance);
            }
        }
    }
}
