public class MiniGroundPool : MiniPool
{
    public static MiniGroundPool _groundPool;
    public override void Awake()
    {
        _groundPool = this;
        base.Awake();
    }
}
