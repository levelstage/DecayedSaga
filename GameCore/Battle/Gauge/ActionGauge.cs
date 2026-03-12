namespace GameCore.Battle.Gauge;

public class ActionGauge
{
    public const float MaxValue = 4f;

    public float Value { get; private set; } = 0f;
    public bool IsReady => Value <= 0f;

    public void Tick(float delta) => Value = Math.Max(0f, Value - delta);
    public void Recharge(int gaugeWeight) => Value = Math.Min(MaxValue, Value + gaugeWeight);
    public void SetFull() => Value = MaxValue;
}
