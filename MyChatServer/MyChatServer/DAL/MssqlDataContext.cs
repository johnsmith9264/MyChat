﻿namespace Andriy.MyChat.Server.DAL
{
    using Andriy.MyChat.Server.ChatServerDataSetTableAdapters;

    class MssqlDataContext : DataContext
    {
        private LoginsTableAdapter _loginsTableAdapter;

        private MssqlDataContext(string connectionString)
        {
            this._loginsTableAdapter = new LoginsTableAdapter();
            this._loginsTableAdapter.Connection.ConnectionString = connectionString;
        }

        private MssqlDataContext(LoginsTableAdapter loginsTableAdapter)
        {
            this._loginsTableAdapter = loginsTableAdapter;
        }

        public override bool LoginExists(string login)
        {
            return (int)this._loginsTableAdapter.isLoginInBase(login) > 0;
        }

        public override bool ValidateLoginPass(string login, string password)
        {
            return (int)this._loginsTableAdapter.ValidateLogPass(login, password) > 0;
        }


        public override void AddUser(string login, string password)
        {
            this._loginsTableAdapter.addNewLoginPass(login, password);
        }
    }
}