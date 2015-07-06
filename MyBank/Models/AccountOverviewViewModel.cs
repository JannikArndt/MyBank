using System.Collections.Generic;

namespace MyBank.Models
{
    public class AccountOverviewViewModel
    {
        public List<BankAccount> Accounts { get; set; }
        public string Message { get; set; }
        public bool Error { get; set; }
        public bool Success { get; set; }
    }
}