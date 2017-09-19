using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Item_Type
{
    Health
}

public class Item : MonoBehaviour
{
    // Type of pickup; default to health
    public Item_Type ItemType = Item_Type.Health;
    public int Value = 1;

    void OnTriggerEnter2D (Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            bool pickedUp = false;
            var player = collider.gameObject.GetComponent<Player>();
            if (player == null)
            {
                Debug.LogError("Player Script does not exist while picking up item!");
                return;
            }

            switch (ItemType)
            {
                case Item_Type.Health:
                    player.RestoreHealth(Value);
                    pickedUp = true;
                    break;
                default:
                    Debug.LogError("Invalid item type: " + ItemType);
                    break;
            }

            // May be inefficient if called a lot; could disable sprite and collider instead
            if (pickedUp)
                Destroy(gameObject);
        }
    }
}
