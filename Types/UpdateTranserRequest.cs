namespace PayOSNetCore.Types;

public record UpdateTranserRequest(
    string ?code,
    string ?desc,
    UpdateTranserData data,
    string ?signature
);

public record UpdateTranserData(
    int orderCode,
    int amount,
    string description,
    string accountNumber,
    string reference,
    string transactionDateTime,
    string paymentLinkId,
    string virtualAccountNumber,
    string counterAccountBankId,
    string counterAccountBankName,
    string counterAccountName,
    string counterAccountNumber,
    string virtualAccountName,
    string code,
    string desc
);