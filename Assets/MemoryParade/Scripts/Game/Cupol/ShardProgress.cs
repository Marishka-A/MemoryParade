public static class ShardProgress
{
    public static int PlayerShards = 0;
    public static int BarrierShards = 0;
    public static int TargetShards = 12;

    public static void AddPlayerShards(int amount)
    {
        PlayerShards += amount;
        if (PlayerShards < 0)
            PlayerShards = 0;
    }

    public static void DepositAll()
    {
        BarrierShards += PlayerShards;
        PlayerShards = 0;

        if (BarrierShards > TargetShards)
            BarrierShards = TargetShards;
    }

    public static bool IsBarrierFull()
    {
        return BarrierShards >= TargetShards;
    }
}