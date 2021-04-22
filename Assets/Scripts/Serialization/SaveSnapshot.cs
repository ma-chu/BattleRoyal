using System;
using System.Globalization;
using EF.Localization;
using UnityEngine;

public class SaveSnapshot /*: ISerializationCallbackReceiver*/
// Возможно, этот интерфонтейнерыейс надо реализовывать, если будем сохранять какие-то к:
// сперва сериализуем объекты контейнера, а потом уже содержащий контейнер класс SaveSnapshot
    {
        public Language language;
        public int tournamentsWon;
    }