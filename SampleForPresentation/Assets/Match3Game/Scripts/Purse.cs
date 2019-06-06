using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Purse {

    private static int max = 4000;

    private static int _balance = 0;
    public static int Balance
    {
        get
        {
            return _balance;
        }
        private set {
            _balance = value;
        }
    }

    public static void Refresh()
    {
        Balance = 0;
    }

    public static void AddCombo(int count)
    {
        switch (count)
        {
            case 3:
                _balance += 10;
                break;
            case 4:
                _balance += 20;
                break;
            case 5:
                _balance += 35;
                break;
            default:
                _balance += count * 10;
                break;
        }
        UIController.instance.SetText(_balance.ToString());
        SoundManager.instance.PlaySingle();
        if (_balance >= max)
        {
            UIController.instance.SetText("Win!");
            FieldManager.instance.HideAllChips();
            GameController.instance.isWin = true;
        }
    }
}
