using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;
using BankAPI.Data.BankModels;
using TestBankAPI.Data.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BankAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class AccountController : ControllerBase
{
    private readonly AccountService accountservice;

    private readonly AccountTypeService accountTypeservice;

    private readonly ClientService clienttservice;
    
    public AccountController(AccountService accountService,
                             AccountTypeService accountTypeService,
                             ClientService clientService)
    {
        this.accountservice = accountService;
        this.accountTypeservice = accountTypeService;
        this.clienttservice = clientService;
    }

    [HttpGet("getall")]
    public async Task<IEnumerable<AccountDtoOut>> Get()
    {
        return await accountservice.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDtoOut>> GetById(int id)
    {
        var account = await accountservice.GetDtoById(id);

        if(account is null)
        {
            return AccountNotFound(id);
        }

        return account;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(AccountDtoIn account)
    {   
        string validateId = accountservice.CheckId(account.ClientId.Value);
        if(validateId.Equals("Error"))
        {
            return BadRequest(new { message = $"El cliente con ID = {account.ClientId} no existe."});
        }

        string ValidationAccountType = await ValidateAccountType(account.AccountType);
        if(!ValidationAccountType.Equals("Valid"))
        {
            return BadRequest(new { message = ValidationAccountType});
        }

        var newAccount = await accountservice.Create(account);

        return CreatedAtAction(nameof(GetById), new { id = newAccount.Id}, newAccount);
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, AccountDtoIn account)
    {
        if(id != account.Id)
        {
            return BadRequest(new { message = $"El ID({id}) de la URL no coincide con el ID({account.Id}) del cuerpo de la solicitud."});
        }

        string ValidationAccountType = await ValidateAccountType(account.AccountType);
        if(!ValidationAccountType.Equals("Valid"))
        {
            return BadRequest(new { message = ValidationAccountType});
        }
        
        var accountToUpdate = await accountservice.GetById(id);

        if (accountToUpdate is not null)
        {
            string validateClientId = await accountservice.CheckClientId(id, account.ClientId.Value);
            if(validateClientId.Equals("Ok"))
            {
                await accountservice.Update(id, account);
                return NoContent();
            }
            else
            {
                return BadRequest(new { message = "El ID del usuario no puede ser cambiado en ninguna cuenta"});
            }
        }
        else
        {
            return AccountNotFound(id);
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var clientToDelete = await accountservice.GetById(id);
        if(clientToDelete is not null)
        {
            await accountservice.Delete(id);
            return Ok();
        }
        else
        {
            return AccountNotFound(id);
        }
    }

    public NotFoundObjectResult AccountNotFound(int id)
    {
        return NotFound(new { message = $"La cuenta con ID = {id} no existe" });
    }

    public async Task<string> ValidateAccountType(int accountType)
    {
        string result = "Valid";

        var accountTypeValidation = await accountTypeservice.GetById(accountType);

        if(accountTypeValidation is null)
        {
            result = $"El tipo de cuenta {accountType} no existe";
        }

        return result;
    }
}