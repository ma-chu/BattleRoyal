public class PlayerViewManager : HeroViewManager
{
    protected override void Awake()
    {
        base.Awake();

        heroType = Heroes.Player;
    }

    protected override void OnEnable()
    {
        inventory.CloseItemDescription();
        base.OnEnable();                                            
    }
}
