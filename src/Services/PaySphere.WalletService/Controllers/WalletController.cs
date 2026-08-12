using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaySphere.BuildingBlocks.Pagination;
using PaySphere.WalletService.DTOs.Requests;
using PaySphere.WalletService.Services.Interfaces;

namespace PaySphere.WalletService.Controllers;

[ApiController]
[Route("api/v1/wallets")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

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
    public async Task<IActionResult> GetTransactions([FromQuery] PaginationRequest request)
    {
        var userId = GetUserId();
        var transactions = await _walletService.GetTransactionsAsync(userId, request);

        return Ok(transactions);
    }

    private int GetUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.Parse(userIdValue!);
    }
}
