### Test Description
In the 'PaymentService.cs' file you will find a method for making a payment. At a high level the steps for making a payment are:

 - Lookup the account the payment is being made from
 - Check the account is in a valid state to make the payment
 - Deduct the payment amount from the account's balance and update the account in the database
 
What we’d like you to do is refactor the code with the following things in mind:  
 - Adherence to SOLID principals
 - Testability  
 - Readability 

We’d also like you to add some unit tests to the ClearBank.DeveloperTest.Tests project to show how you would test the code that you’ve produced. The only specific ‘rules’ are:  

 - The solution should build.
 - The tests should all pass.
 - You should not change the method signature of the MakePayment method.

You are free to use any frameworks/NuGet packages that you see fit.  
 
You should plan to spend around 1 to 3 hours to complete the exercise.

### Coding Decisions

- The refactor keeps the existing payment business rules covered by tests before changing the structure of `PaymentService`.
- `PaymentService` depends on `IAccountDataStore` so the service can be tested with a fake store and the concrete data-store choice can be made outside the payment workflow.
- `AccountDataStoreFactory` preserves the original config-based selection behavior, but in a consuming application this wiring would normally happen at the composition root through dependency injection.
- The current factory uses `ConfigurationManager.AppSettings` because that is what the original code used. In a modern .NET application I would usually prefer typed configuration through `IOptions<T>`, environment-specific configuration providers, or explicit DI registration based on configuration rather than reading static app settings inside application code.
- Validation logic is kept lightweight and explicit rather than introducing a validation framework, because the current rules are small and tied to the loaded account state.
- Unknown `PaymentScheme` values are currently allowed by the original implementation because no `switch` case marks the result as failed. I have preserved that behavior during the refactor; in production I would normally fail closed and document that as an intentional defensive fix.
- `AllowedPaymentSchemes` is marked with `[Flags]` because the enum values are bit flags and the code checks them using `HasFlag`. This adds some intent + improves debug string 

### Business Rules (covered By tests)

- A missing debtor account causes the payment to fail.
- A successful payment deducts the payment amount from the debtor account balance.
- A successful payment updates the account in the data store.
- A failed payment does not update the account.
- Bacs requires the account to allow Bacs.
- Faster Payments requires the account to allow Faster Payments and have sufficient balance.
- Chaps requires the account to allow Chaps and have a `Live` status.

### Further Improvements

- Clarify expected behavior for null requests, invalid payment schemes, zero or negative payment amounts, and currently unused request fields such as `CreditorAccountNumber` and `PaymentDate`.
- Add richer failure information to `MakePaymentResult` if the public contract could change.
- Move data-store selection fully into the consuming application's dependency-injection setup.
