namespace PaySphere.BuildingBlocks.Enums
{
    /// <summary>
    /// Enum representing different ledger transaction types used by the WalletService.
    /// Transfer operations create a paired debit (sender) and credit (receiver) entry.
    /// </summary>
    public enum TransactionType
    {
        TopUp = 1,
        Withdrawal = 2,
        TransferDebit = 3,
        TransferCredit = 4
    }
}
