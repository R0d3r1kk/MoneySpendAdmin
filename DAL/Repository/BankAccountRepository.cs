using MoneySpendAdmin.DAL.Entities;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneySpendAdmin.DAL.Repository
{
    public class BankAccountRepository : IRepository<BankAccount>
    {
        private readonly SQLiteAsyncConnection _db;

        public BankAccountRepository(IDataAccess dal)
        {
            _db = dal.GetAsyncConnection();
        }

        public async Task DeleteAsync(BankAccount bankAccount)
        {
            await _db.DeleteAsync(bankAccount);
        }

        public async Task<List<BankAccount>> GetAllAsync()
        {
            var bas = await _db.Table<BankAccount>().ToListAsync();
            return bas;
        }

        public async Task SaveAsync(BankAccount bankAccount)
        {
            var ba = await _db.Table<BankAccount>().FirstOrDefaultAsync(b => b.id == bankAccount.id);
            if (ba == null)
            {
                await _db.InsertAsync(bankAccount);
            }
            else
            {
                await _db.UpdateAsync(bankAccount);
            }
        }
    }
}
