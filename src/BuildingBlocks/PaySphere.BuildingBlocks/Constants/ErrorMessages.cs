using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaySphere.BuildingBlocks.Constants
{
    public static class ErrorMessages
    {
        public const string UserNotFound = "User not found.";

        public const string WalletNotFound = "Wallet not found.";

        public const string InvalidCredentials = "Invalid email or password.";

        public const string InsufficientBalance = "Insufficient wallet balance.";

        public const string DuplicateUser = "User already exists.";
    }
}
