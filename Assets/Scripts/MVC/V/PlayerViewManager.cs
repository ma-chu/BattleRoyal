public class PlayerViewManager : HeroViewManager
{
    protected override void Awake()
    {
        base.Awake();

        heroType = Heroes.Player;
    }

    protected override void OnEnable()                              // (back on again, следующий раунд)
    {
        inventory.CloseItemDescription();                           // скрыть описание инвентаря (если он был выигран в предыдущем раунде)

        base.OnEnable();                                            
    }
}
