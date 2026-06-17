using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services;

public class PaymentService : IPaymentService
{
    private readonly IAccountDataStore _accountDataStore;

    public PaymentService(IAccountDataStore accountDataStore)
    {
        _accountDataStore = accountDataStore;
    }

    public MakePaymentResult MakePayment(MakePaymentRequest request)
    {
        var account = _accountDataStore.GetAccount(request.DebtorAccountNumber);

        if (!PaymentValidator.PaymentCanBeMade(request, account))
        {
            return new MakePaymentResult { Success = false };
        }

        account.Balance -= request.Amount;

        _accountDataStore.UpdateAccount(account);

        return new MakePaymentResult { Success = true };
    }
}
