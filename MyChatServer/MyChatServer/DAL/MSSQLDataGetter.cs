﻿namespace MyChatServer.DAL
{
    using MyChatServer.ChatServerDataSetTableAdapters;

    class MSSQLDataGetter : DataGetter
    {
        private LoginsTableAdapter _loginsTableAdapter;

        private MSSQLDataGetter(string connectionString)
        {
            this._loginsTableAdapter = new LoginsTableAdapter();
            this._loginsTableAdapter.Connection.ConnectionString = connectionString;
        }

        private MSSQLDataGetter(LoginsTableAdapter loginsTableAdapter)
        {
            this._loginsTableAdapter = loginsTableAdapter;
        }

        public override bool ValidateLogin(string login)
        {
            return (int)this._loginsTableAdapter.isLoginInBase(login) > 0;
        }

        public override bool ValidateLoginPass(string login, string password)
        {
            return (int)this._loginsTableAdapter.ValidateLogPass(login, password) > 0;
        }


        public override void AddNewLoginPass(string login, string password)
        {
            this._loginsTableAdapter.addNewLoginPass(login, password);
        }
    }
}