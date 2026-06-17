using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services;
internal static class PaymentValidator
{
    public static bool PaymentCanBeMade(MakePaymentRequest request, Account account)
    {
        return AccountExists(account) && AccountMeetsSchemeRules(request, account);
    }

    private static bool AccountExists(Account account)
    {
        return account != null;
    }

    private static bool AccountMeetsSchemeRules(MakePaymentRequest request, Account account)
    {
        switch (request.PaymentScheme)
        {
            case PaymentScheme.Bacs:
                return AccountAllowsBacs(account);

            case PaymentScheme.FasterPayments:
                return AccountAllowsFasterPayments(account)
                       && AccountHasSufficientBalance(request, account);

            case PaymentScheme.Chaps:
                return AccountAllowsChaps(account)
                       && AccountIsLive(account);
        }

        return true;
    }

    private static bool AccountAllowsBacs(Account account)
    {
        return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs);
    }

    private static bool AccountAllowsFasterPayments(Account account)
    {
        return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments);
    }

    private static bool AccountAllowsChaps(Account account)
    {
        return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps);
    }

    private static bool AccountHasSufficientBalance(MakePaymentRequest request, Account account)
    {
        return account.Balance >= request.Amount;
    }

    private static bool AccountIsLive(Account account)
    {
        return account.Status == AccountStatus.Live;
    }
}
