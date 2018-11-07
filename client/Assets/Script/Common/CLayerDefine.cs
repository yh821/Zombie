using UnityEngine;
using System.Collections;

public static class CLayerDefine
{
    public const int Default = 0;
    public const int Player = 8;
    public const int Hero = 9;
    public const int Monstor = 10;
    public const int Item = 11;

    public const int DefaultMask = 1 << Default;
    public const int PlayerMask = 1 << Player;
    public const int HeroMask = 1 << Hero;
    public const int MonstorMask = 1 << Monstor;
    public const int ItemMask = 1 << Item;
}









