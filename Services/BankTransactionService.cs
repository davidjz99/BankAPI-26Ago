using Microsoft.EntityFrameworkCore;
using BankAPI.Data;
using BankAPI.Data.BankModels;
using TestBankAPI.Data.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Services;

public class BankTransactionService
{
    private readonly BankContext _context;
    private readonly AccountService accountservice;
    private readonly LoginService loginservice;
    public int ownerAccountId;
    
    public BankTransactionService(BankContext context, AccountService accountService, LoginService loginService)
    {
        _context = context;
        this.accountservice = accountService;
        this.loginservice = loginService;
    }

/*
    public async Task<Account?> GetByClientId(int clientID)
    {
        return await _context.Accounts.FindAsync(clientID);
    }
*/

    public async Task<ActionResult<IEnumerable<AccountDtoOut?>>> GetAllAcountsClient(int clientId)
    {
        return await accountservice.GetDtoByClientId(clientId);
    }

    public async Task <decimal> GetBalance(int id)
    {
        var existingAccount = await accountservice.GetDtoById(id);
        decimal actualBalance = existingAccount.Balance;

        return actualBalance;
    }

    public async Task Retire(int id, decimal balance)
    {
        var existingAccount = await accountservice.GetDtoById(id);

        if(existingAccount is not null)
        {
            existingAccount.Balance = (existingAccount.Balance - balance);

            await _context.SaveChangesAsync();
        }
    }

    public async Task Deposit(int id, decimal balance)
    {
        var existingAccount = await accountservice.GetDtoById(id);

        if(existingAccount is not null)
        {
            existingAccount.Balance = (existingAccount.Balance + balance);

            await _context.SaveChangesAsync();
        }
    }

    public async Task Delete(int id)
    {
        var accountToDelete = await accountservice.GetById(id);

        if(accountToDelete is not null)
        {
            _context.Accounts.Remove(accountToDelete);
            await _context.SaveChangesAsync();
        }
    }

    public void GetIdClient(int id)
    {
        ownerAccountId = id;
    }
    
    public int returnClientId()
    {
        return ownerAccountId;
    }
}