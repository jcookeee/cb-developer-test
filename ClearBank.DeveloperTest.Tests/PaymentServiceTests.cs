using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Xunit;

namespace ClearBank.DeveloperTest.Tests;

public class PaymentServiceTests
{
    [Fact]
    public void MakePayment_WhenAccountDoesNotExist_ReturnsFailure()
    {
        var (service, _) = CreatePaymentService(account: null);

        var result = service.MakePayment(CreateRequest(PaymentScheme.Bacs));

        Assert.False(result.Success);
    }

    [Fact]
    public void MakePayment_WhenPaymentSucceeds_DeductsAmountFromBalance()
    {
        var account = CreateAccount(AllowedPaymentSchemes.FasterPayments, balance: 100m);
        var (service, _) = CreatePaymentService(account);

        service.MakePayment(CreateRequest(PaymentScheme.FasterPayments, amount: 25m));

        Assert.Equal(75m, account.Balance);
    }

    [Fact]
    public void MakePayment_WhenPaymentSucceeds_UpdatesAccount()
    {
        var account = CreateAccount(AllowedPaymentSchemes.FasterPayments, balance: 100m);
        var (service, dataStore) = CreatePaymentService(account);

        service.MakePayment(CreateRequest(PaymentScheme.FasterPayments, amount: 25m));

        Assert.Same(account, dataStore.UpdatedAccount);
    }

    [Fact]
    public void MakePayment_WhenPaymentFails_DoesNotUpdateAccount()
    {
        var account = CreateAccount(AllowedPaymentSchemes.Bacs, balance: 100m);
        var (service, dataStore) = CreatePaymentService(account);

        service.MakePayment(CreateRequest(PaymentScheme.FasterPayments, amount: 25m));

        Assert.Null(dataStore.UpdatedAccount);
    }

    [Fact]
    public void MakePayment_WhenBacsIsAllowed_ReturnsSuccess()
    {
        var account = CreateAccount(AllowedPaymentSchemes.Bacs);
        var (service, _) = CreatePaymentService(account);

        var result = service.MakePayment(CreateRequest(PaymentScheme.Bacs));

        Assert.True(result.Success);
    }

    [Fact]
    public void MakePayment_WhenBacsIsNotAllowed_ReturnsFailure()
    {
        var account = CreateAccount(AllowedPaymentSchemes.FasterPayments);
        var (service, _) = CreatePaymentService(account);

        var result = service.MakePayment(CreateRequest(PaymentScheme.Bacs));

        Assert.False(result.Success);
    }

    [Fact]
    public void MakePayment_WhenFasterPaymentsIsAllowedAndBalanceIsSufficient_ReturnsSuccess()
    {
        var account = CreateAccount(AllowedPaymentSchemes.FasterPayments, balance: 100m);
        var (service, _) = CreatePaymentService(account);

        var result = service.MakePayment(CreateRequest(PaymentScheme.FasterPayments, amount: 100m));

        Assert.True(result.Success);
    }

    [Fact]
    public void MakePayment_WhenFasterPaymentsIsNotAllowed_ReturnsFailure()
    {
        var account = CreateAccount(AllowedPaymentSchemes.Bacs, balance: 100m);
        var (service, _) = CreatePaymentService(account);

        var result = service.MakePayment(CreateRequest(PaymentScheme.FasterPayments, amount: 25m));

        Assert.False(result.Success);
    }

    [Fact]
    public void MakePayment_WhenFasterPaymentsBalanceIsInsufficient_ReturnsFailure()
    {
        var account = CreateAccount(AllowedPaymentSchemes.FasterPayments, balance: 24.99m);
        var (service, _) = CreatePaymentService(account);

        var result = service.MakePayment(CreateRequest(PaymentScheme.FasterPayments, amount: 25m));

        Assert.False(result.Success);
    }

    [Fact]
    public void MakePayment_WhenChapsIsAllowedAndAccountIsLive_ReturnsSuccess()
    {
        var account = CreateAccount(AllowedPaymentSchemes.Chaps, status: AccountStatus.Live);
        var (service, _) = CreatePaymentService(account);

        var result = service.MakePayment(CreateRequest(PaymentScheme.Chaps));

        Assert.True(result.Success);
    }

    [Fact]
    public void MakePayment_WhenChapsIsNotAllowed_ReturnsFailure()
    {
        var account = CreateAccount(AllowedPaymentSchemes.Bacs, status: AccountStatus.Live);
        var (service, _) = CreatePaymentService(account);

        var result = service.MakePayment(CreateRequest(PaymentScheme.Chaps));

        Assert.False(result.Success);
    }

    [Fact]
    public void MakePayment_WhenChapsAccountIsDisabled_ReturnsFailure()
    {
        var account = CreateAccount(AllowedPaymentSchemes.Chaps, status: AccountStatus.Disabled);
        var (service, _) = CreatePaymentService(account);

        var result = service.MakePayment(CreateRequest(PaymentScheme.Chaps));

        Assert.False(result.Success);
    }

    [Fact]
    public void MakePayment_WhenChapsAccountIsInboundPaymentsOnly_ReturnsFailure()
    {
        var account = CreateAccount(AllowedPaymentSchemes.Chaps, status: AccountStatus.InboundPaymentsOnly);
        var (service, _) = CreatePaymentService(account);

        var result = service.MakePayment(CreateRequest(PaymentScheme.Chaps));

        Assert.False(result.Success);
    }

    private static (PaymentService Service, FakeAccountDataStore DataStore) CreatePaymentService(Account account)
    {
        var dataStore = new FakeAccountDataStore(account);

        return (new PaymentService(dataStore), dataStore);
    }

    private static MakePaymentRequest CreateRequest(PaymentScheme paymentScheme, decimal amount = 10m)
    {
        return new MakePaymentRequest
        {
            DebtorAccountNumber = "123",
            Amount = amount,
            PaymentScheme = paymentScheme
        };
    }

    private static Account CreateAccount(
        AllowedPaymentSchemes allowedPaymentSchemes,
        decimal balance = 100m,
        AccountStatus status = AccountStatus.Live)
    {
        return new Account
        {
            AccountNumber = "123",
            AllowedPaymentSchemes = allowedPaymentSchemes,
            Balance = balance,
            Status = status
        };
    }

    private class FakeAccountDataStore : IAccountDataStore
    {
        private readonly Account _account;

        public FakeAccountDataStore(Account account)
        {
            _account = account;
        }

        public Account UpdatedAccount { get; private set; }

        public Account GetAccount(string accountNumber)
        {
            return _account;
        }

        public void UpdateAccount(Account account)
        {
            UpdatedAccount = account;
        }
    }
}
