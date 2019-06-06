using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public static GameController instance = null;
    public bool isWin = false;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void Update()
    {
		System.Action<Vector3> updateInput = (Vector3 pos) =>
		{
			if (!isWin)
			{
				RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
				if (hit.collider != null)
				{
					hit.transform.gameObject.GetComponent<ChipController>().OnTileDown();
				}
			}
			else
			{
				Purse.Refresh();
				isWin = false;
				SceneManager.LoadScene(0);
			}
		};

        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase.Equals(TouchPhase.Began))
            {
				updateInput(Input.GetTouch(i).position);
            }
        }

		if(Input.GetMouseButtonDown(0))
		{
			updateInput(Input.mousePosition);
		}
    }
}
