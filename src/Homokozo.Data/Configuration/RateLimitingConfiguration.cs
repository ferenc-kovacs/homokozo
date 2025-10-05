public class RateLimitingConfiguration
{
    public const string Path = "RateLimiting";

    public const string SlidingPolicy = "SlidingPolicy";

    public int PermitLimit { get; set; } = 100;
    public int QueueLimit { get; set; } = 0;
    public string QueueProcessingOrder { get; set; } = "OldestFirst";
    public int SegmentsPerWindow { get; set; } = 4;
    public int WindowSeconds { get; set; } = 60; 
}