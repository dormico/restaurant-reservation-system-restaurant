namespace Restaurant;

#nullable enable
public class Restaurant
{
    public RestaurantItem RestaurantItem { get; set; }
    public string Password { get; set; }
}

public class RestaurantItem
{
    public string? partitionKey { get; set; }
    public string? id { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool Takeaway { get; set; }
    public Map? Tables { get; set; }
    public int Pricing { get; set; }
    public string? CardNum { get; set; }
    public int OpeningH { get; set; }
    public int OpeningM { get; set; }
    public int ClosingH { get; set; }
    public int ClosingM { get; set; }
    public MenuItem[]? Menu { get; set; }

    public string? Image { get; set; }
    public string? Website { get; set; }
    public string? Style { get; set; }
    public Review[]? Reviews { get; set; }
    public int? Rating { get; set; }

    public class MenuItem
    {
        public string? dishId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Price { get; set; }
        public string? Image { get; set; }
    }

    public class Review
    {
        public string? id { get; set; }
        public string? Date { get; set; }
        public int Rating { get; set; }
        public string? Text { get; set; }
        public string? Answer { get; set; }
    }

    public class Map
    {
        public string? Name { get; set; }
        public int Size { get; set; }
        public string? Image { get; set; }
        public Field[][]? Fields { get; set; }
    }
}

public class Field
{
    public string? Type { get; set; }
    public int Data { get; set; }
    public (int, int)[]? Reserved { get; set; }
}