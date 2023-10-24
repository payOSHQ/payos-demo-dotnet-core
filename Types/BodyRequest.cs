namespace PayOSNetCore.Types;

public record Item(
    String name,
    int quantity,
    int price
);

public record BodyRequest(
    int orderCode,
    int amount,
    String description,
    List<Item> items,
    String cancelUrl,
    String returnUrl,
    String signature
);