using System;
using System.Collections.Generic;

namespace BankAPI.Data.BankModels
{
    public partial class AccountType
    {
        public AccountType()
        {
            Accounts = new HashSet<Account>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime Regdate { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
