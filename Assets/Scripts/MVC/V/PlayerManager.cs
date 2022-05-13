public class PlayerManager : HeroManager
{
    protected override void Awake()
    {
        base.Awake();                                               

        //inventory.CloseItemDescription();                      

        heroType = Heroes.Player;
    }

    protected override void OnEnable()                              // (back on again, следующий раунд)
    {
        inventory.CloseItemDescription();                           // скрыть описание инвентаря (если он был выигран в предыдущем раунде)

        base.OnEnable();                                            
    }
}
