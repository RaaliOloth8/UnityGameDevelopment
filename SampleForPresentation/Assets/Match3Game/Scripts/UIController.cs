using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public Text scoreText;
    public static UIController instance = null;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void Start()
    {
        scoreText.text = "0";
    }

    public void SetText(string text)
    {
        scoreText.text = text;
    }
}
