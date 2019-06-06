using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipController : MonoBehaviour {

    private bool _isSelected;
    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            if (value)
            {
                _isSelected = true;
                gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
            }
            else
            {
                _isSelected = false;
                gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }
    private GameObject left;
    private GameObject right;
    private GameObject up;
    private GameObject down;
    
    public void OnTileDown()
    {
        if (!FieldManager.instance.isWorking)
        {
            // Проверка, чтобы не выделялись не соседние ячейки (одновременно)
            if (IsSelected)
            {
                IsSelected = false;
                FieldManager.instance.previousChip = null;
            }
            else
            {
                IsSelected = true;

                left = GetAdjacentChip(new Vector3(-1f, 0f, 0f), Vector2.left);
                up = GetAdjacentChip(new Vector3(0f, 1f, 0f), Vector2.up);
                right = GetAdjacentChip(new Vector3(1f, 0f, 0f), Vector2.right);
                down = GetAdjacentChip(new Vector3(0f, -1f, 0f), Vector2.down);

                List<GameObject> adjacentChips = new List<GameObject>
                {
                    left,
                    right,
                    up,
                    down
                };

                if (FieldManager.instance.previousChip != null)
                {
                    bool foundAdjacentChip = false;
                    foreach (GameObject chip in adjacentChips)
                    {
                        if (chip != null)
                        {
                            if (chip == FieldManager.instance.previousChip)
                            {
                                foundAdjacentChip = true;
                                IsSelected = false;
                                chip.GetComponent<ChipController>().IsSelected = false;
                                ChangeSprites(gameObject, chip);
                                StartCoroutine(FieldManager.instance.CheckCombination());
                                FieldManager.instance.previousChip = null;
                            }
                        }
                    }
                    if (!foundAdjacentChip)
                    {
                        IsSelected = false;
                        FieldManager.instance.previousChip.GetComponent<ChipController>().IsSelected = false;
                        FieldManager.instance.previousChip = null;
                    }
                }
                else
                {
                    FieldManager.instance.previousChip = gameObject;
                }
            }
        }
    }

    private GameObject GetAdjacentChip(Vector3 offset, Vector2 vector)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + offset, vector);
        if (hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    private void ChangeSprites(GameObject g1, GameObject g2)
    {
        Sprite temp = g1.GetComponent<SpriteRenderer>().sprite;
        g1.GetComponent<SpriteRenderer>().sprite = g2.GetComponent<SpriteRenderer>().sprite;
        g2.GetComponent<SpriteRenderer>().sprite = temp;
    }
}
