using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaySphere.WalletService.Migrations;

public partial class AddTransactionHistoryStoredProcedure : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE PROCEDURE [dbo].[GetTransactionsForWallet]
                @WalletId INT,
                @Search NVARCHAR(200) = NULL,
                @Type INT = NULL,
                @DateFrom DATETIME2 = NULL,
                @DateTo DATETIME2 = NULL,
                @SortBy NVARCHAR(50) = 'createdAt',
                @SortOrder NVARCHAR(10) = 'desc',
                @PageNumber INT = 1,
                @PageSize INT = 10
            AS
            BEGIN
                SET NOCOUNT ON;

                -- Result set 1: total records
                SELECT COUNT(*) AS TotalRecords
                FROM Transactions
                WHERE WalletId = @WalletId
                  AND (@Search IS NULL
                       OR Reference LIKE '%' + @Search + '%'
                       OR Description LIKE '%' + @Search + '%')
                  AND (@Type IS NULL OR Type = @Type)
                  AND (@DateFrom IS NULL OR CreatedAt >= @DateFrom)
                  AND (@DateTo IS NULL OR CreatedAt <= @DateTo);

                -- Result set 2: paged transactions
                SELECT
                    Id,
                    WalletId,
                    Type,
                    Amount,
                    BalanceBefore,
                    BalanceAfter,
                    Reference,
                    Description,
                    CreatedAt,
                    UpdatedAt
                FROM Transactions
                WHERE WalletId = @WalletId
                  AND (@Search IS NULL
                       OR Reference LIKE '%' + @Search + '%'
                       OR Description LIKE '%' + @Search + '%')
                  AND (@Type IS NULL OR Type = @Type)
                  AND (@DateFrom IS NULL OR CreatedAt >= @DateFrom)
                  AND (@DateTo IS NULL OR CreatedAt <= @DateTo)
                ORDER BY
                    CASE WHEN @SortBy = 'amount' AND @SortOrder = 'asc'
                         THEN Amount END ASC,
                    CASE WHEN @SortBy = 'amount' AND @SortOrder = 'desc'
                         THEN Amount END DESC,
                    CASE WHEN @SortBy <> 'amount' AND @SortOrder = 'asc'
                         THEN CreatedAt END ASC,
                    CASE WHEN @SortBy <> 'amount' AND @SortOrder <> 'asc'
                         THEN CreatedAt END DESC
                OFFSET (@PageNumber - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP PROCEDURE IF EXISTS [dbo].[GetTransactionsForWallet];
            """);
    }
}