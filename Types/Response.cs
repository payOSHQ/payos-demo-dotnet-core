namespace PayOSNetCore.Types;


public record Response(
    int error,
    String message,
    object? data
);

public record CheckoutUrl(
    String checkoutUrl
);

public record OrderInfo(
    int id,
    string status,
    List<Item> items,
    int amount,
    string ref_id,
    string description,
    DateTime ?transaction_when,
    string payment_link_id,
    string transaction_code,
    DateTime created_at,
    DateTime updated_at,
    WebhookSnapshot ?webhook_snapshot
);