using MoneySpendAdmin.DAL.Entities;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneySpendAdmin.DAL.Repository
{
    public class MovementRepository : IRepository<Movement>
    {
        private readonly SQLiteAsyncConnection _db;

        public MovementRepository(IDataAccess dal)
        {
            _db = dal.GetAsyncConnection();
        }

        public async Task DeleteAsync(Movement move)
        {
            await _db.DeleteAsync(move);
        }

        public async Task<List<Movement>> GetAllAsync()
        {
            var moves = await _db.Table<Movement>().ToListAsync();
            return moves;
        }

        public async Task SaveAsync(Movement move)
        {
            var ba = await _db.Table<Movement>().FirstOrDefaultAsync(b => b.id == move.id);
            if (ba == null)
            {
                await _db.InsertAsync(move);
            }
            else
            {
                await _db.UpdateAsync(move);
            }
        }
    }
}
