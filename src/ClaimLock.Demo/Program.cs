namespace ClaimLock.Demo;

public static class Program
{
    private static readonly string[] Recipes =
        ["garlic-chicken", "mushroom-risotto", "lemon-pasta", "herb-salmon"];

    public static async Task Main()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "claim-lock-demo");
        if (Directory.Exists(baseDir))
        {
            Directory.Delete(baseDir, true);
        }

        var store = new FileClaimStore(baseDir);
        var manager = new ClaimManager(store);
        var ttl = TimeSpan.FromSeconds(3);

        Console.WriteLine("=== Claim Lock Demo: Shared Kitchen ===");
        Console.WriteLine();

        // Phase 1: Three chefs compete for recipes.
        Console.WriteLine("--- Phase 1: Three chefs claim recipes ---");
        var chefs = new[] { "chef-alice", "chef-bob", "chef-carol" };
        var tasks = chefs.Select(chef => Task.Run(async () =>
        {
            foreach (var recipe in Recipes)
            {
                var result = manager.Acquire(recipe, chef, ttl);
                if (result.Outcome == AcquireOutcome.Acquired)
                {
                    Console.WriteLine($"  {chef} claimed {recipe}");
                    await Task.Delay(500);
                    manager.Release(recipe, chef);
                    Console.WriteLine($"  {chef} finished {recipe}");
                    return;
                }
            }

            Console.WriteLine($"  {chef} found no available recipes");
        })).ToArray();

        await Task.WhenAll(tasks);
        Console.WriteLine();

        // Phase 2: Non-owner release is a no-op.
        Console.WriteLine("--- Phase 2: Non-owner release ---");
        manager.Acquire("herb-salmon", "chef-alice", ttl);
        Console.WriteLine("  chef-alice claims herb-salmon");
        var released = manager.Release("herb-salmon", "chef-bob");
        Console.WriteLine($"  chef-bob tries to release herb-salmon: {(released ? "released" : "no-op")}");
        manager.Release("herb-salmon", "chef-alice");
        Console.WriteLine("  chef-alice releases herb-salmon");
        Console.WriteLine();

        // Phase 3: TTL expiry recovery.
        Console.WriteLine("--- Phase 3: TTL recovery ---");
        var shortTtl = TimeSpan.FromSeconds(1);
        manager.Acquire("lemon-pasta", "chef-dave", shortTtl);
        Console.WriteLine("  chef-dave claims lemon-pasta (1s TTL) and crashes...");
        await Task.Delay(1500);
        var recovery = manager.Acquire("lemon-pasta", "chef-carol", ttl);
        Console.WriteLine($"  chef-carol reclaims lemon-pasta after TTL: {recovery.Outcome}");
        manager.Release("lemon-pasta", "chef-carol");
        Console.WriteLine();

        // Phase 4: Force override.
        Console.WriteLine("--- Phase 4: Force override ---");
        manager.Acquire("mushroom-risotto", "chef-alice", ttl);
        Console.WriteLine("  chef-alice claims mushroom-risotto");
        var overrideResult = manager.ForceOverride("mushroom-risotto", "chef-bob", ttl);
        Console.WriteLine($"  chef-bob force-overrides: {overrideResult.Outcome}");
        manager.Release("mushroom-risotto", "chef-bob");

        Console.WriteLine();
        Console.WriteLine("=== Demo complete ===");
    }
}
