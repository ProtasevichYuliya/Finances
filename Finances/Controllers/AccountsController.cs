using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Finances.ApiModels;
using Finances.Data;
using Finances.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finances.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public AccountsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // GET: api/accounts
        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<Account>>> GetAll()
        {
            var accounts = await this.context.Accounts
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return accounts;
        }

        // GET: api/accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> Get(int id)
        {
            var account = await this.context.Accounts
                .FirstOrDefaultAsync(x => x.Id == id);
            if (account == null)
            {
                return NotFound();
            }
            return account;
        }

        // POST: api/accounts
        [HttpPost("")]
        public async Task<ActionResult<Account>> Register(AccountCreateModel model)
        {
            if (model.Balance < 0)
            {
                this.ModelState.AddModelError(nameof(model.Balance), "Balance can not be negative!");
            }
            if (this.ModelState.IsValid)
            {
                var account = new Account
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    BirthDate = model.BirthDate?.Date,
                    Balance = model.Balance
                };
                this.context.Accounts.Add(account);
                await this.context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = account.Id }, account);
            }
            return this.BadRequest(this.ModelState);
        }

        // PUT: api/accounts/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Edit(int id, AccountEditModel model)
        {
            var account = await this.context.Accounts
                .FirstOrDefaultAsync(x => x.Id == id);

            if (account == null)
            {
                return NotFound();
            }

            if (this.ModelState.IsValid)
            {
                account.LastName = model.LastName;
                account.FirstName = model.FirstName;
                account.MiddleName = model.MiddleName;
                account.BirthDate = model.BirthDate;
                await this.context.SaveChangesAsync();
                return NoContent();
            }

            return BadRequest(this.ModelState);
        }

        // DELETE: api/accounts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var account = await context.Accounts
                .FirstOrDefaultAsync(x => x.Id == id);
            context.Accounts.Remove(account);
            await context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/accounts/5/balance
        [HttpGet("{id}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(int id)
        {
            var account = await this.context.Accounts
                .FirstOrDefaultAsync(x => x.Id == id);
            if (account == null)
            {
                return NotFound();
            }
            return account.Balance;
        }

        //POST: api/accounts/5/deposit
        [HttpPost("{id}/deposit")]
        public async Task<ActionResult> Deposit(int id, [FromBody] decimal sum)
        {
            var account = await this.context.Accounts
                .FirstOrDefaultAsync(x => x.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            if (sum <= 0)
            {
                return BadRequest("Deposited sum must be more than zero!");
            }

            while (true)
            {
                try
                {
                    account.Balance += sum;
                    await context.SaveChangesAsync();
                    return NoContent();
                }
                catch (DbUpdateConcurrencyException)
                {
                    await this.context.Entry(account).ReloadAsync();
                }
            }
        }

        //POST: api/accounts/5/withdraw
        [HttpPost("{id}/withdraw")]
        public async Task<ActionResult> Withdraw(int id, [FromBody] decimal sum)
        {
            var account = await this.context.Accounts
                .FirstOrDefaultAsync(x => x.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            if (sum <= 0)
            {
                return BadRequest("Withdrawal sum must be more than zero!");
            }

            while (true)
            {
                try
                {
                    if (sum > account.Balance)
                    {
                        return BadRequest("You do not have enough money on your account to withdraw requested sum!");
                    }

                    account.Balance -= sum;
                    await context.SaveChangesAsync();
                    return NoContent();

                }
                catch (DbUpdateConcurrencyException)
                {
                    await this.context.Entry(account).ReloadAsync();
                }
            }
        }
    }
}