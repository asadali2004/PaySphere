using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaySphere.BuildingBlocks.Pagination;
using PaySphere.WalletService.DTOs.Requests;
using PaySphere.WalletService.Services.Interfaces;

// Handles wallet HTTP endpoints (create, balance, top-up, withdraw, transfer,
// transaction history). The controller is thin and delegates business rules to
// IWalletService which manages validation, transactions and repository operations.
namespace PaySphere.WalletService.Controllers;

[ApiController]
[Route("api/v1/wallets")]
[Authorize]
/// <summary>
/// Controller exposing wallet-related endpoints for the authenticated user.
/// All operations use the authenticated user's id from the JWT claim.
/// </summary>
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    // Controller methods are intentionally thin and simply forward authenticated user context to
    // the WalletService which contains the core business rules (validation, transactions, repository calls).
    [HttpPost]
    public async Task<IActionResult> CreateWallet()
    {
        var userId = GetUserId();
        var wallet = await _walletService.CreateWalletAsync(userId);

        return Ok(wallet);
    }

    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        var userId = GetUserId();
        var wallet = await _walletService.GetWalletAsync(userId);

        return Ok(wallet);
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetUserId();
        var balance = await _walletService.GetBalanceAsync(userId);

        return Ok(balance);
    }

    [HttpPost("top-up")]
    public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
    {
        var userId = GetUserId();
        var wallet = await _walletService.TopUpAsync(userId, request);

        return Ok(wallet);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        var userId = GetUserId();
        var wallet = await _walletService.WithdrawAsync(userId, request);

        return Ok(wallet);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var userId = GetUserId();
        var wallet = await _walletService.TransferAsync(userId, request);

        return Ok(wallet);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] PaginationRequest request,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc")
    {
        var userId = GetUserId();
        var transactions = await _walletService.GetTransactionsAsync(userId, request, search, type, dateFrom, dateTo, sortBy, sortOrder);

        return Ok(transactions);
    }

    private int GetUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.Parse(userIdValue!);
    }
}
