using EF.Localization;


public class SaveSnapshot /*: ISerializationCallbackReceiver*/
// Возможно, этот интерфейс надо реализовывать, если будем сохранять какие-то контейнеры:
// сперва сериализуем объекты контейнера, а потом уже содержащий контейнер класс SaveSnapshot
    {
        public Language language;
        public int tournamentsWon;
        public float SFXLvl;
        public float musicLvl;
    }