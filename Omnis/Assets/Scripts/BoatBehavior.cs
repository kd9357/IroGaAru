using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class BoatBehavior : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Rigidbody2D _player;

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();

            var p = GameObject.FindGameObjectWithTag("Player");
            if (p == null)
            {
                Debug.LogError(gameObject.name + " could not find a player object!");
            }
            else
            {
                _player = p.GetComponent<Rigidbody2D>();
            }
        }

        void FixedUpdate()
        {
            if (_rb.velocity.x >= .01f || _rb.velocity.x <= -0.1f && !_player.isKinematic)
            {
                _player.isKinematic = true;
            }
            else if (_rb.velocity.x <= 0f && _player.isKinematic)
            {
                _player.isKinematic = false;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                collision.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
                collision.gameObject.transform.parent = gameObject.transform;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                collision.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                collision.gameObject.transform.parent = null;
            }
        }
    }
}
