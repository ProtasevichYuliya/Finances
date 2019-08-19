using System;
using System.Collections.Generic;
using System.Text;
using Finances.Models;

namespace Finances.UnitTests
{
    class AccountOperation
    {
        public Account Account { get; set; }

        public AccountOperationType OperationType { get; set; }

        public decimal Sum { get; set; }
    }
}
