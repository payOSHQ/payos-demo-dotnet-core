using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using PayOS;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V1.Payouts.Batch;
using PayOS.Models.V1.PayoutsAccount;

using PayOSDemo.Models;
using PayOSDemo.Services;

namespace PayOSDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferController([FromKeyedServices("TransferClient")] PayOSClient client) : ControllerBase
{
    private readonly PayOSClient _client = client;

    [HttpGet("{id}")]
    public async Task<ActionResult<Transfer>> Get(string id)
    {
        try
        {
            var transfer = TransferService.GetTransferById(id);
            Payout payout;
            if (transfer == null)
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(transfer.PayoutId))
            {
                var payoutPage = await _client.Payouts.ListAsync(new GetPayoutListParam { ReferenceId = id });
                if (payoutPage.Data.Count == 0)
                {
                    return NotFound();
                }
                payout = payoutPage.Data[0]; // TODO: Handle case where multiple payouts exist with the same referenceId. Currently selecting the first one; consider filtering by status or date for accuracy.
            }
            else
            {
                payout = await _client.Payouts.GetAsync(transfer.PayoutId);
            }
            TransferService.UpdateTransfer(id, MapPayoutToTransfer(payout));
            return Ok(transfer);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to retrieve transfer", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<Transfer>>> List([FromQuery] GetPayoutListParam? param = null)
    {
        try
        {
            var page = await _client.Payouts.ListAsync(param);
            var transfers = page.Data.Select(MapPayoutToTransfer).ToList();
            foreach (Transfer transfer in transfers)
            {
                TransferService.UpdateTransfer(transfer.Id, transfer);
            }
            return Ok(transfers);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to retrieve transfers", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Transfer>> CreateTransfer([FromBody] TransferCreateRequest request)
    {
        if (request == null)
        {
            return BadRequest("Transfer data is required");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var payoutRequest = new PayoutRequest
        {
            ReferenceId = Guid.NewGuid().ToString(), // Using your id generator
            Amount = request.Amount,
            Description = request.Description,
            ToBin = request.ToBin,
            ToAccountNumber = request.ToAccountNumber,
            Category = request.Category
        };

        try
        {
            var payoutResponse = await _client.Payouts.CreateAsync(payoutRequest);

            var transfer = MapPayoutToTransfer(payoutResponse);

            TransferService.CreateTransfer(transfer);

            return CreatedAtAction(nameof(Get), new { id = transfer.Id }, transfer);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to create payout", error = ex.Message });
        }
    }

    [HttpPost("batch")]
    public async Task<ActionResult<Transfer>> CreateBatchTransfer([FromBody] List<TransferCreateRequest> requests)
    {
        if (requests == null || requests.Count == 0)
        {
            return BadRequest("Transfers data is required");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var batchPayoutRequest = new PayoutBatchRequest
        {
            ReferenceId = Guid.NewGuid().ToString(), // Using your id generator
            Payouts = [.. requests.Select(r => new PayoutBatchItem
            {
                ReferenceId = Guid.NewGuid().ToString(), // Using your id generator
                Amount = r.Amount,
                Description = r.Description,
                ToBin = r.ToBin,
                ToAccountNumber = r.ToAccountNumber
            })],
            Category = [.. requests.SelectMany(r => r.Category ?? []).Distinct()]
        };

        try
        {
            var payoutResponse = await _client.Payouts.Batch.CreateAsync(batchPayoutRequest);

            var batchTransfer = MapPayoutToTransfer(payoutResponse);

            TransferService.CreateTransfer(batchTransfer);

            return CreatedAtAction(nameof(Get), new { id = batchTransfer.Id }, batchTransfer);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to create batch payout", error = ex.Message });
        }
    }

    [HttpGet("account-balance")]
    public async Task<ActionResult<PayoutAccountInfo>> GetAccountBalance()
    {
        try
        {
            var payoutAccount = await _client.PayoutsAccount.GetBalanceAsync();
            return payoutAccount;
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to get account balance", error = ex.Message });
        }
    }

    [HttpPost("estimate-credit")]
    public async Task<ActionResult<EstimateCredit>> EstimateCredit([FromBody] List<TransferCreateRequest> requests)
    {
        if (requests == null || requests.Count == 0)
        {
            return BadRequest("Transfers data is required");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var batchPayoutRequest = new PayoutBatchRequest
        {
            ReferenceId = Guid.NewGuid().ToString(), // Using your id generator
            Payouts = [.. requests.Select(r => new PayoutBatchItem
            {
                ReferenceId = Guid.NewGuid().ToString(), // Using your id generator
                Amount = r.Amount,
                Description = r.Description,
                ToBin = r.ToBin,
                ToAccountNumber = r.ToAccountNumber
            })],
            Category = [.. requests.SelectMany(r => r.Category ?? []).Distinct()]
        };
        try
        {
            var result = await _client.Payouts.EstimateCredit(batchPayoutRequest);

            return result;
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Failed to estimate credit for transfer request", error = ex.Message });
        }
    }

    private static Transfer MapPayoutToTransfer(Payout payout)
    {
        return new Transfer
        {
            Id = payout.ReferenceId,
            PayoutId = payout.Id,
            Category = payout.Category,
            ApprovalState = payout.ApprovalState,
            CreatedAt = DateTimeOffset.TryParse(payout.CreatedAt, out var createdAt) ? createdAt : DateTimeOffset.Now,
            Transactions = [.. payout.Transactions.Select(MapPayoutTransactionToTransferTransaction)]
        };
    }

    private static TransferTransaction MapPayoutTransactionToTransferTransaction(PayoutTransaction transaction)
    {
        return new TransferTransaction
        {
            Id = transaction.ReferenceId,
            PayoutTransactionId = transaction.Id,
            Amount = transaction.Amount,
            Description = transaction.Description,
            ToBin = transaction.ToBin,
            ToAccountNumber = transaction.ToAccountNumber,
            ToAccountName = transaction.ToAccountName,
            Reference = transaction.Reference,
            TransactionDatetime = DateTimeOffset.TryParse(transaction.TransactionDatetime, out var transactionDatetime) ? transactionDatetime : null,
            ErrorMessage = transaction.ErrorMessage,
            ErrorCode = transaction.ErrorCode,
            State = transaction.State
        };
    }
}
