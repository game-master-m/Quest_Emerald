#region ·¹ÀÌ¾î
public static class LayerManager
{
    public static int GetLayerMask(ELayerName layerName)
    {
        return 1 << (int)layerName;
    }
}
public enum ELayerName
{
    Default, TransparentFX, ignoreRaycast, Ground, Water, UI, Item, Player
}
#endregion
public enum EItemType
{
    None = 0, Gold, Red, Green, Blue, Black, Wall, Stepper, FakeBlack
}
public enum EBgmName
{
    None, Bgm
}
public enum ESfxName
{
    None, WallTouch, BallTouch, GoldTouch, StepperTouch, BombTouch
}
public enum EEffectName
{
    None = 0, Wall, Red, Blue, Green, Black, Gold, FakeBlack
}