using PayOSDemo.Models;

namespace PayOSDemo.Services;

public static class OrderTransactionService
{
    private static readonly List<OrderTransaction> Transactions = [];
    private static int _nextId = 1;

    public static List<OrderTransaction> GetAllTransactions()
    {
        return Transactions;
    }

    public static OrderTransaction? GetTransactionById(int id)
    {
        return Transactions.FirstOrDefault(t => t.Id == id);
    }

    public static List<OrderTransaction> GetTransactionsByOrderId(int orderId)
    {
        return Transactions.Where(t => t.OrderId == orderId).ToList();
    }

    public static List<OrderTransaction> GetTransactionsByOrderCode(long orderCode)
    {
        return Transactions.Where(t => t.OrderCode == orderCode).ToList();
    }

    public static List<OrderTransaction> GetTransactionsByPaymentLinkId(string paymentLinkId)
    {
        return Transactions.Where(t => t.PaymentLinkId == paymentLinkId).ToList();
    }

    public static void CreateTransaction(OrderTransaction transaction)
    {
        transaction.Id = _nextId++;
        Transactions.Add(transaction);
    }

    public static void CreateTransactions(List<OrderTransaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            CreateTransaction(transaction);
        }
    }

    public static bool UpdateTransaction(int id, OrderTransaction updatedTransaction)
    {
        var transaction = Transactions.FirstOrDefault(t => t.Id == id);
        if (transaction == null) return false;

        transaction.OrderId = updatedTransaction.OrderId;
        transaction.OrderCode = updatedTransaction.OrderCode;
        transaction.PaymentLinkId = updatedTransaction.PaymentLinkId;
        transaction.Reference = updatedTransaction.Reference;
        transaction.Amount = updatedTransaction.Amount;
        transaction.AccountNumber = updatedTransaction.AccountNumber;
        transaction.Description = updatedTransaction.Description;
        transaction.TransactionDateTime = updatedTransaction.TransactionDateTime;
        transaction.VirtualAccountName = updatedTransaction.VirtualAccountName;
        transaction.VirtualAccountNumber = updatedTransaction.VirtualAccountNumber;
        transaction.CounterAccountBankId = updatedTransaction.CounterAccountBankId;
        transaction.CounterAccountBankName = updatedTransaction.CounterAccountBankName;
        transaction.CounterAccountName = updatedTransaction.CounterAccountName;
        transaction.CounterAccountNumber = updatedTransaction.CounterAccountNumber;

        return true;
    }

    public static bool DeleteTransaction(int id)
    {
        var transaction = Transactions.FirstOrDefault(t => t.Id == id);
        if (transaction == null) return false;
        Transactions.Remove(transaction);
        return true;
    }

    public static bool DeleteTransactionsByOrderId(int orderId)
    {
        var transactionsToRemove = Transactions.Where(t => t.OrderId == orderId).ToList();
        foreach (var transaction in transactionsToRemove)
        {
            Transactions.Remove(transaction);
        }
        return transactionsToRemove.Count > 0;
    }
}