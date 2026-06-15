using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Domain.Exceptions;

namespace Festpay.Onboarding.Domain.Tests.Entities;

public class AccountTests
{
    [Fact]
    public void Should_Create_Account_When_Data_Is_Valid()
    {
        var account = new Account.Builder()
            .WithName("John Doe")
            .WithDocument("16670073607")
            .WithEmail("john.doe@example.com")
            .WithPhone("11999999999")
            .Build();

        Assert.Equal("John Doe", account.Name);
        Assert.Equal("16670073607", account.Document);
        Assert.Equal("john.doe@example.com", account.Email);
        Assert.Equal("11999999999", account.Phone);
    }

    [Fact]
    public void Should_Throw_RequiredFieldException_When_Name_Is_Empty()
    {
        var exception = Assert.Throws<RequiredFieldException>(
            () =>
                new Account.Builder()
                    .WithName("")
                    .WithDocument("16670073607")
                    .WithEmail("john.doe@example.com")
                    .WithPhone("11999999999")
                    .Build()
        );

        Assert.Equal("Name", exception.FieldName);
    }

    [Fact]
    public void Should_Throw_InvalidDocumentNumberException_When_Document_Is_Invalid()
    {
        var invalidDocument = "00000000000";

        var exception = Assert.Throws<InvalidDocumentNumberException>(
            () =>
                new Account.Builder()
                    .WithName("John Doe")
                    .WithDocument(invalidDocument)
                    .WithEmail("john.doe@example.com")
                    .WithPhone("11999999999")
                    .Build()
        );

        Assert.Equal(invalidDocument, exception.Document);
    }

    [Fact]
    public void Should_Throw_InvalidEmailFormatException_When_Email_Is_Invalid()
    {
        var invalidEmail = "john.doeexample.com";

        var exception = Assert.Throws<InvalidEmailFormatException>(
            () =>
                new Account.Builder()
                    .WithName("John Doe")
                    .WithDocument("16670073607")
                    .WithEmail(invalidEmail)
                    .WithPhone("11999999999")
                    .Build()
        );

        Assert.Equal(invalidEmail, exception.Email);
    }

    [Fact]
    public void Should_Throw_InvalidPhoneNumberException_When_Phone_Is_Invalid()
    {
        var invalidPhone = "123";

        var exception = Assert.Throws<InvalidPhoneNumberException>(
            () =>
                new Account.Builder()
                    .WithName("John Doe")
                    .WithDocument("16670073607")
                    .WithEmail("john.doe@example.com")
                    .WithPhone(invalidPhone)
                    .Build()
        );

        Assert.Equal(invalidPhone, exception.Phone);
    }
    
    [Fact]
    public void Should_Deposit_Successfully_When_Amount_Is_Valid()
    {
        var account = new Account.Builder()
            .WithName("John Doe")
            .WithDocument("16670073607")
            .WithEmail("test@test.com")
            .WithPhone("11999999999")
            .Build();
            
        account.Deposit(100.00m);
        
        Assert.Equal(100.00m, account.Balance);
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_Deposit_Is_Zero_Or_Negative()
    {
        var account = new Account.Builder()
            .WithName("John Doe")
            .WithDocument("16670073607")
            .WithEmail("test@test.com")
            .WithPhone("11999999999")
            .Build();

        Assert.Throws<InvalidAccountOperationAmountException>(() => account.Deposit(0));
        Assert.Throws<InvalidAccountOperationAmountException>(() => account.Deposit(-10.00m));
    }

    [Fact]
    public void Should_Withdraw_Successfully_When_Balance_Is_Sufficient()
    {
        var account = new Account.Builder()
            .WithName("John Doe")
            .WithDocument("16670073607")
            .WithEmail("test@test.com")
            .WithPhone("11999999999")
            .Build();
            
        account.Deposit(150.00m);
        account.Withdraw(50.00m);
        
        Assert.Equal(100.00m, account.Balance);
    }

    [Fact]
    public void Should_Throw_InvalidOperationException_When_Withdraw_Exceeds_Balance()
    {
        var account = new Account.Builder()
            .WithName("John Doe")
            .WithDocument("16670073607")
            .WithEmail("test@test.com")
            .WithPhone("11999999999")
            .Build();
            
        account.Deposit(50.00m);
        
        var exception = Assert.Throws<InsufficientBalanceException>(
            () => account.Withdraw(100.00m)
        );
        
        Assert.Equal("Insufficient balance to complete the operation.", exception.Message);
    }
    
}
