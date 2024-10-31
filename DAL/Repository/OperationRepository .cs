using MoneySpendAdmin.DAL.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneySpendAdmin.DAL.Repository
{
    public class OperationRepository : IRepository<Operation>
    {
        private readonly SQLiteAsyncConnection _db;

        public OperationRepository(IDataAccess dal)
        {
            _db = dal.GetAsyncConnection();
        }

        public async Task DeleteAsync(Operation op)
        {
            await _db.DeleteAsync(op);
        }

        public async Task<List<Operation>> GetAllAsync()
        {
            var op = await _db.Table<Operation>().ToListAsync();
            return op;
        }

        public async Task SaveAsync(Operation op)
        {
            var ba = await _db.Table<Operation>().FirstOrDefaultAsync(b => b.id == op.id);
            if (ba == null)
            {
                op.id = Guid.NewGuid().ToString();
                op.fecha_creacion = DateTime.Now;
                await _db.InsertAsync(op);
            }
            else
            {
                op.id = ba.id;
                op.fecha_creacion = DateTime.Now;
                await _db.UpdateAsync(op);
            }
        }
    }
}
