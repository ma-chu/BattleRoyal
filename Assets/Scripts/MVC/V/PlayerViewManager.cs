public class PlayerViewManager : HeroViewManager
{
    protected override void Awake()
    {
        base.Awake();

        HeroType = Heroes.Player;
    }

    protected override void OnEnable()
    {
        inventory.CloseItemDescription();
        base.OnEnable();                                            
    }
}
