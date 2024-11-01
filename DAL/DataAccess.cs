using System;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using System.IO;
using SQLite;
using System.Threading.Tasks;
using MoneySpendAdmin.DAL.Entities;
namespace MoneySpendAdmin.DAL
{
    public  class DataAccess : IDataAccess
    {
        public string dbName = "moneySpendAdmin.db";
        private SQLiteAsyncConnection _dbConnection;
        private bool dbExist;
        private bool connected = false;

        public DataAccess()
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
           dbName);
           dbExist = File.Exists(dbpath);
            if (!connected)
            {
                Task.Run(() => Initialize());
            }
        }

        public async Task Initialize()
        {

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbName);
            if (!dbExist) {
                await ApplicationData.Current.LocalFolder
                   .CreateFileAsync(dbName, CreationCollisionOption.OpenIfExists);
                _dbConnection = new SQLiteAsyncConnection(dbpath);
            }
            else
                _dbConnection = new SQLiteAsyncConnection(dbpath);

            connected = _dbConnection != null;

            await _dbConnection.CreateTableAsync<BankAccount>();
            await _dbConnection.CreateTableAsync<Balance>();
            await _dbConnection.CreateTableAsync<Movement>();
            await _dbConnection.CreateTableAsync<Operation>();
            await _dbConnection.CreateTableAsync<TextExtraction>();
        }

        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return _dbConnection;
        }
        

    }
}
