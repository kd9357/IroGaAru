﻿using UnityEngine;

public enum Item_Type
{
    Health
}

public class Item : MonoBehaviour
{
    // Type of pickup; default to health
    public Item_Type ItemType = Item_Type.Health;
    public int Value = 1;

    private bool _pickedUp = false;
    private readonly System.Object _itemLock = new System.Object();

    public void ResetItem()
    {
        _pickedUp = false;
        gameObject.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            var player = collider.gameObject.GetComponent<Player>();
            if (player == null)
            {
                Debug.LogError("Player Script does not exist while picking up item!");
                return;
            }

            lock (_itemLock)
            {
                if (_pickedUp)
                    return;

                switch (ItemType)
                {
                    case Item_Type.Health:
                        player.RestoreHealth(Value);
                        _pickedUp = true;

                        gameObject.GetComponent<AudioSource>().Play();
                        break;
                    default:
                        Debug.LogError("Invalid item type: " + ItemType);
                        break;
                }

                if (_pickedUp)
                {
                    gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
    }
}
