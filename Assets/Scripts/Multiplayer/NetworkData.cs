using Bolt;
using UnityEngine;

public class NetworkData : EntityBehaviour<IEFPlayerState>
{
/*    public override void Attached()                // аналог Start()
    {
        var evnt = EFPlayerJoinedEvent.Create();   
        evnt.Username = PlayerPrefs.GetString("username");
        state.Username = evnt.Username;
        evnt.Send();
    }

    public override void Detached()
    {
        var evnt = EFPLayerLeftEvent.Create();
        evnt.Send();
    }*/
}

 
// на клиенте по изменению Decision создавать болт-команду (и отправлять на сервер). Расчет урона самому не выполнять!
// на сервере как только есть болт-команда и решение серверного игрока, выполняем команду выход команды на сервере - ?генерим событие на сервере?
// или выход сразу уходит в клиент и мы выполняем анимацию и пр.

//нет, не так
// на клиенте: по изменению Decision создавать событие (и отправлять на сервер). Расчет урона самому не выполнять, ожидать ответного события!
// на сервере: как только есть это событие и решение серверного игрока, выполняем расчет урона (плюс анимацию и пр) и генерим ответное событие
// на клиенте: отлавливаем ответное событие и выполняем анимацию и пр.    