using Microsoft.EntityFrameworkCore;
using BankAPI.Data;
using BankAPI.Data.BankModels;
using TestBankAPI.Data.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Services;

public class AccountService
{
    private readonly BankContext _context;

    public AccountService(BankContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AccountDtoOut>> GetAll()
    {
        return await _context.Accounts.Select(a => new AccountDtoOut
        {
            Id = a.Id,
            AccountName = a.AccountTypeNavigation.Name,
            ClientName = a.Client != null ? a.Client.Name : "",
            Balance = a.Balance,
            RegDate = a.RegDate
        }).ToListAsync();
    }

    public async Task<AccountDtoOut?> GetDtoById(int id)
    {
        return await _context.Accounts.
            Where(a => a.Id == id).
            Select(a => new AccountDtoOut
        {
            Id = a.Id,
            AccountName = a.AccountTypeNavigation.Name,
            ClientName = a.Client != null ? a.Client.Name : "",
            Balance = a.Balance,
            RegDate = a.RegDate
        }).SingleOrDefaultAsync();
    }

    //Seleccionar cuentas de un cliente en especifico
    public async Task<ActionResult<IEnumerable<AccountDtoOut?>>> GetDtoByClientId(int clientId)
    {
        return await _context.Accounts.
            Where(a => a.ClientId == clientId).
            Select(a => new AccountDtoOut
        {
            Id = a.Id,
            AccountName = a.AccountTypeNavigation.Name,
            ClientName = a.Client != null ? a.Client.Name : "",
            Balance = a.Balance,
            RegDate = a.RegDate
        }).ToListAsync();
    }

    public async Task<Account?> GetById(int id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    public string CheckId(int id)
    {
        var existingClient = _context.Clients.Find(id);

        if(existingClient is null)
        {
            return "Error";
        }
        else
        {
            return "Ok";
        }
    }

    public async Task<Account> Create(AccountDtoIn newAccountDTO)
    {
        var newAccount = new Account();
        newAccount.AccountType = newAccountDTO.AccountType;
        newAccount.ClientId = newAccountDTO.ClientId;
        newAccount.Balance = newAccountDTO.Balance;

        _context.Accounts.Add(newAccount);
        await _context.SaveChangesAsync();

        return newAccount;
    }

    public async Task<string> CheckClientId(int id, int clientId)
    {
        var originalAccount = await GetById(id);

        if(originalAccount is not null)
        {
            if(originalAccount.ClientId != clientId)
            {
                return "Error";
            }
            else
            {
                return "Ok";
            }
        }
        else
        {
            return "Error";
        }
    }

    public async Task Update(int id, AccountDtoIn account)
    {
        var existingAccount = await GetById(id);

        if(existingAccount is not null)
        {
            existingAccount.AccountType = account.AccountType;
            existingAccount.Balance = account.Balance;

            await _context.SaveChangesAsync();
        }
    }

    public async Task Delete(int id)
    {
        var accountToDelete = await GetById(id);

        if(accountToDelete is not null)
        {
            _context.Accounts.Remove(accountToDelete);
            await _context.SaveChangesAsync();
        }
    }
}