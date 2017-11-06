using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class BoatBehavior : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Rigidbody2D _player;
        private bool _onBoat;

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _onBoat = false;

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
            if (_onBoat && Mathf.Abs(_rb.velocity.x) >= .05f)
            {
                _player.isKinematic = true;
            }
            else if (_onBoat && (Mathf.Abs(_rb.velocity.x) <= 0.05f))
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
                _onBoat = true;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                collision.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                collision.gameObject.transform.parent = null;
                _onBoat = false;
            }
        }
    }
}
