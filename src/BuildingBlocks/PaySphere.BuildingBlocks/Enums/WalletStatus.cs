namespace PaySphere.BuildingBlocks.Enums
{
    /// <summary>
    /// Represents the lifecycle status of a wallet. Services should check status
    /// before allowing financial operations (only Active wallets allow operations).
    /// </summary>
    public enum WalletStatus
    {
        Active = 1,
        Frozen = 2,
        Closed = 3
    }
}
