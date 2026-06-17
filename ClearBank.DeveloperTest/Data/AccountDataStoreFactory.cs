using System.Configuration;

namespace ClearBank.DeveloperTest.Data;

internal static class AccountDataStoreFactory
{
    internal static IAccountDataStore CreateAccountDataStore()
    {
        var dataStoreType = ConfigurationManager.AppSettings["DataStoreType"];

        if (dataStoreType == "Backup")
        {
            return new BackupAccountDataStore();
        }

        return new AccountDataStore();
    }
}
