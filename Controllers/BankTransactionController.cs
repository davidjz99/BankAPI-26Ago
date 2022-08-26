using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;
using BankAPI.Data.BankModels;
using TestBankAPI.Data.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BankAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class BankTransactionController : ControllerBase
{
    private readonly BankTransactionService _service;
    private readonly AccountService accountservice;
    private readonly LoginService loginservice;
    public BankTransactionController(BankTransactionService bankTransaction, AccountService accountService, LoginService loginService)
    {
        _service = bankTransaction;
        this.accountservice = accountService;
        this.loginservice = loginService;
    }
/*
    [HttpGet("{clientId}")]
    public async Task<ActionResult<Account>> GetByClientId(int clientId)
    {
        var accountClient = await _service.GetByClientId(clientId);

        if(accountClient is null)
        {
            return AccountNotFound(clientId);
        }
        
        return accountClient;
    }
*/

    [HttpGet("get/{clientId}")]
    public async Task<ActionResult<IEnumerable<AccountDtoOut?>>> Get(int clientId)
    {
        int ownerAccountId = _service.returnClientId();
        if(clientId == ownerAccountId)
        {
            var existingAcounts = await _service.GetAllAcountsClient(clientId);
            
            if(existingAcounts is not null)
            {
                return existingAcounts;
            }
            else
            {
                return AccountNotFound(clientId);
            }
        }
        else
        {
           return BadRequest(new { message = $"No puedes acceder las cuentas del cliente con id {clientId} ya que no eres el dueno, tu id es {ownerAccountId}."});
        }
    }

    [HttpPut("retire/{id}")]
    public async Task<IActionResult> Retire(int id, decimal balance)
    {
        int ownerAccountId = _service.returnClientId();
        var accountToUpdate = await accountservice.GetById(id);
        if(accountToUpdate is not null)
        {
            if(accountToUpdate.ClientId == ownerAccountId)
            {
                var actualBalance = await _service.GetBalance(id);

                if(balance < actualBalance)
                {
                    await _service.Retire(id, balance);
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = $"no puedes retirar más dinero del que tienes. Usted cuenta con ${actualBalance} y quiere retirar ${balance}."});
                }
            }
            else
            {
                return BadRequest(new { message = $"No eres dueño de esa cuenta con id {id}, por lo tanto no puedes retirar ${balance}." });
            }
        }
        else
        {
            return AccountNotFound(id);
        }
    }

    //Hacer depositos
    [HttpPut("deposit/{id}")]
    public async Task<IActionResult> Deposit(int id, decimal balance)
    {
        var accountToUpdate = await accountservice.GetById(id);
        if(accountToUpdate is not null)
        {
            int ownerAccountId = _service.returnClientId();
            if(accountToUpdate.ClientId == ownerAccountId)
            {
                await _service.Deposit(id, balance);
                    return NoContent();
            }
            else
            {
                return BadRequest(new { message = $"No eres dueño de esa cuenta con id {id}, por lo tanto no puedes depositar." });
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
        var accountToDelete = await accountservice.GetById(id);
        if(accountToDelete is not null)
        {
            if(accountToDelete.Balance == 0)
            {
                await accountservice.Delete(id);
                return Ok();
            }
            else
            {
                return BadRequest(new { message = $"No puedes eliminar cuentas que tengan dinero guardado en ellas." });
            }
        }
        else
        {
            return AccountNotFound(id);
        }
    }

    public NotFoundObjectResult AccountNotFound(int clientId)
    {
        return NotFound(new { message = $"El cliente con ID = {clientId} no tiene cuentas registradas" });
    }
}