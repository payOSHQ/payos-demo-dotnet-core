namespace PayOSNetCore.Types;

public record WebhookSnapshotData(
    int orderCode,
    int amount,
    string description,
    string accountNumber,
    string reference,
    DateTime transactionDateTime,
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

public record WebhookSnapshot(
    string code,
    string desc,
    string signature,
    WebhookSnapshotData ?data
);