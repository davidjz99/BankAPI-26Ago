using Microsoft.EntityFrameworkCore;
using BankAPI.Data;
using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;

namespace BankAPI.Services;

public class LoginService
{
    private readonly BankContext _context;
    private readonly ClientService clientService;

    public LoginService(BankContext context, ClientService clientService)
    {
        _context = context;
        this.clientService = clientService;
    }

    public async Task<Administrator?> GetAdmin(AdminDto admin)
    {
        return await _context.Administrators.
                    SingleOrDefaultAsync(x => x.Email == admin.Email && x.Pwd == admin.Pwd);
    }

    public async Task<Client?> GetClient(ClientDto client)
    {
        return await _context.Clients.
                    SingleOrDefaultAsync(x => x.Name == client.Name && x.Pwd == client.Pwd);
    }
}