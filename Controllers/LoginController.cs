using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;
using BankAPI.Data.DTOs;
using BankAPI.Data.BankModels;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace BankAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class LoginController : ControllerBase
{
    private readonly LoginService loginService;
    private readonly ClientService clientService;
    private IConfiguration config;
    private BankTransactionService bankTransactionService;
    public LoginController(LoginService loginService, IConfiguration config, BankTransactionService bankTransactionService, ClientService clientService)
    {
        this.loginService = loginService;
        this.config = config;
        this.bankTransactionService = bankTransactionService;
        this.clientService = clientService;
    }

    //Login Admin DB
    [HttpPost("admin/authenticate")]
    public async Task<IActionResult> LoginAdmin(AdminDto adminDto)
    {
        var admin = await loginService.GetAdmin(adminDto);

        if(admin is null)
        {
            return BadRequest(new { message = "Credenciales invalidas." });
        }

        string jwtToken = GenerateTokenAdmin(admin);

        return Ok(new { token = jwtToken } );
    }

    private string GenerateTokenAdmin(Administrator admin)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, admin.Name),
            new Claim(ClaimTypes.Email, admin.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("JWT:Key").Value));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var securityToken = new JwtSecurityToken(
                            claims: claims,
                            expires: DateTime.Now.AddMinutes(60),
                            signingCredentials: creds);

        string token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return token;
    }


    //Login Client
    [HttpPost("client/authenticate")]
    public async Task<IActionResult> LoginClient(ClientDto clientDto)
    {
        var client = await loginService.GetClient(clientDto);

        if(client is null)
        {
            return BadRequest(new { message = "Credenciales invalidas." });
        }

        string jwtToken = GenerateTokenClient(client);

        int clientId = client.Id;

        bankTransactionService.GetIdClient(clientId);

        return Ok(new { token = jwtToken } );
        //return BadRequest(new { message = $"Tu id es {clientId}." });
    }

    private string GenerateTokenClient(Client client)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, client.Name),
            new Claim(ClaimTypes.MobilePhone, client.PhoneNumber)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("JWT:Key").Value));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var securityToken = new JwtSecurityToken(
                            claims: claims,
                            expires: DateTime.Now.AddMinutes(60),
                            signingCredentials: creds);

        string token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return token;
    }
}