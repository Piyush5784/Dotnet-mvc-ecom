namespace VMart.Utility
{
    public static class SD
    {
        // Roles
        public const string Role_User = "User";
        public const string Role_Admin = "Admin";

        // Order Statuses
        public const string OrderStatusPending = "Pending";
        public const string OrderStatusConfirmed = "Confirmed";
        public const string OrderStatusProcessing = "Processing";
        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusDelivered = "Delivered";
        public const string OrderStatusCancelled = "Cancelled";
        public const string OrderStatusRefunded = "Refunded";


        // payment statuses
        public const string Payment_Status_pending = "Pending";
        public const string Payment_Status_Completed = "Completed";
        public const string Payment_Status_Failed = "Failed";
        public const string Payment_Status_Cancelled = "Cancelled";

        // logs types
        // Log Levels
        public const string Log_Info = "Info";
        public const string Log_Success = "Success";
        public const string Log_Warning = "Warning";
        public const string Log_Error = "Error";
        public const string Log_Fatal = "Fatal";


    }
}
