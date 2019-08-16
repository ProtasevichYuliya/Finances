using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finances.Models;
using Microsoft.EntityFrameworkCore;

namespace Finances.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
    }
}
