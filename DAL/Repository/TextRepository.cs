using MoneySpendAdmin.DAL.Entities;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneySpendAdmin.DAL.Repository
{
    public class TextRepository : IRepository<TextExtraction>
    {
        private readonly SQLiteAsyncConnection _db;

        public TextRepository(IDataAccess dal)
        {
            _db = dal.GetAsyncConnection();
        }

        public async Task DeleteAsync(TextExtraction txt)
        {
            await _db.DeleteAsync(txt);
        }

        public async Task<List<TextExtraction>> GetAllAsync()
        {
            var ba = await _db.Table<TextExtraction>().ToListAsync();
            return ba;
        }

        public async Task SaveAsync(TextExtraction txt)
        {
            var ba = await _db.Table<TextExtraction>().FirstOrDefaultAsync(b => b.id == txt.id || (b.mes == txt.mes && b.año == txt.año));
            if (ba == null)
            {
                await _db.InsertAsync(txt);
            }
            else
            {
                await _db.UpdateAsync(txt);
            }
        }
    }
}
