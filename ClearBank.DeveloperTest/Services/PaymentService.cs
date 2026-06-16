using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;
using System.Configuration;

namespace ClearBank.DeveloperTest.Services;

public class PaymentService(IAccountDataStore accountDataStore) : IPaymentService
{
    private readonly IAccountDataStore _accountDataStore = accountDataStore;

    public MakePaymentResult MakePayment(MakePaymentRequest request)
    {
        var account = _accountDataStore.GetAccount(request.DebtorAccountNumber);
        
        var result = new MakePaymentResult();

        result.Success = true;

        switch (request.PaymentScheme)
        {
            case PaymentScheme.Bacs:
                if (account == null)
                {
                    result.Success = false;
                }
                else if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
                {
                    result.Success = false;
                }

                break;

            case PaymentScheme.FasterPayments:
                if (account == null)
                {
                    result.Success = false;
                }
                else if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments))
                {
                    result.Success = false;
                }
                else if (account.Balance < request.Amount)
                {
                    result.Success = false;
                }

                break;

            case PaymentScheme.Chaps:
                if (account == null)
                {
                    result.Success = false;
                }
                else if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps))
                {
                    result.Success = false;
                }
                else if (account.Status != AccountStatus.Live)
                {
                    result.Success = false;
                }

                break;
        }

        if (result.Success)
        {
            account.Balance -= request.Amount;

            _accountDataStore.UpdateAccount(account);
        }

        return result;
    }
    
    private static IAccountDataStore CreateAccountDataStore()
    {
        var dataStoreType = ConfigurationManager.AppSettings["DataStoreType"];

        if (dataStoreType == "Backup")
        {
            return new BackupAccountDataStore();
        }

        return new AccountDataStore();
    }
}