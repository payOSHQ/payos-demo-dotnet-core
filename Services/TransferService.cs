using PayOSDemo.Models;
using PayOS.Models.V1.Payouts;

namespace PayOSDemo.Services;

public static class TransferService
{
    private static readonly List<Transfer> Transfers = [];

    public static List<Transfer> GetAllTransfers()
    {
        return Transfers;
    }

    public static Transfer? GetTransferById(string id)
    {
        return Transfers.FirstOrDefault(t => t.Id == id);
    }

    public static void CreateTransfer(Transfer transfer)
    {
        Transfers.Add(transfer);
    }

    public static bool UpdateTransfer(string id, Transfer updatedTransfer)
    {
        var transfer = Transfers.FirstOrDefault(t => t.Id == id);
        if (transfer == null) return false;
        transfer.Category = updatedTransfer.Category;
        transfer.ApprovalState = updatedTransfer.ApprovalState;
        transfer.CreatedAt = updatedTransfer.CreatedAt;
        transfer.Transactions = updatedTransfer.Transactions;

        return true;
    }

    public static bool DeleteTransfer(string id)
    {
        var transfer = Transfers.FirstOrDefault(t => t.Id == id);
        if (transfer == null) return false;
        Transfers.Remove(transfer);
        return true;
    }
}
