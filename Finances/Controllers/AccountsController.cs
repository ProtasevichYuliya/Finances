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

        /// <summary>
        /// Gets all accounts.
        /// </summary>
        /// <response code="200">Returns all accounts</response>
        // GET: api/accounts
        [HttpGet("")]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<Account>>> GetAll()
        {
            var accounts = await this.context.Accounts
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return accounts;
        }

        /// <summary>
        /// Gets a specific account.
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Returns the requested account</response>
        /// <response code="404">The requested account was not found</response>
        // GET: api/accounts/5
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
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

        /// <summary>
        /// Creates new account.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/accounts
        ///     {
        ///         "lastName": "Иванов",
        ///         "firstName": "Иван",
        ///         "middleName": "Иванович",
        ///         "birthDate": "1984-11-01",
        ///         "balance": 200.00
        ///     }
        ///
        /// </remarks>
        /// <param name="model"></param>
        /// <response code="201">Account is successfully created</response>
        /// <response code="400">Invalid data is provided</response>
        // POST: api/accounts
        [HttpPost("")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
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

        /// <summary>
        /// Edits a specific account.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/accounts/1
        ///     {
        ///         "lastName": "Иванов",
        ///         "firstName": "Иван",
        ///         "middleName": "Иванович",
        ///         "birthDate": "1984-11-01",
        ///         "balance": 200.00
        ///     }
        ///
        /// </remarks>
        /// <response code="204">Account is successfully updated</response>
        /// <response code="400">Invalid data is provided</response>
        /// <response code="404">The requested account was not found</response>
        // PUT: api/accounts/5
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
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

        /// <summary>
        /// Deletes a specific account.
        /// </summary>
        /// <param name="id"></param>
        /// <response code="204">Account is successfully deleted</response>
        /// <response code="404">The requested account was not found</response>
        // DELETE: api/accounts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        public async Task<ActionResult> Delete(int id)
        {
            var account = await context.Accounts
                .FirstOrDefaultAsync(x => x.Id == id);
            if (account == null)
            {
                return NotFound();
            }
            context.Accounts.Remove(account);
            await context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Returns balance of a specific account.
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Returns the balance of the requested account</response>
        /// <response code="404">The requested account was not found</response>
        // GET: api/accounts/5/balance
        [HttpGet("{id}/balance")]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
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

        /// <summary>
        /// Deposits sum on a specific account.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sum"></param>
        /// <response code="204">The sum is successfully deposited</response>
        /// <response code="400">Invalid data is provided</response>
        /// <response code="404">The requested account was not found</response>
        //POST: api/accounts/5/deposit
        [HttpPost("{id}/deposit")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Consumes("text/plain")]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
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

        /// <summary>
        /// Withdraws sum from a specific account.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sum"></param>
        /// <response code="204">The sum is successfully withdrawn</response>
        /// <response code="400">Invalid data is provided</response>
        /// <response code="404">The requested account was not found</response>
        //POST: api/accounts/5/withdraw
        [HttpPost("{id}/withdraw")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Consumes("text/plain")]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
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