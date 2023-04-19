
public class DistanceRewarder
{
    private float startDistance;
    private float lastDistance;
    private System.Func<float> getDistance;
    public DistanceRewarder(System.Func<float> getDistance)
    {
        this.getDistance = getDistance;
        startDistance = getDistance();
        lastDistance = startDistance;
    }
    public float Reward()
    {
        float distance = getDistance();
        float reward = (lastDistance - distance) / startDistance;
        lastDistance = distance;
        return reward;
    }
}
