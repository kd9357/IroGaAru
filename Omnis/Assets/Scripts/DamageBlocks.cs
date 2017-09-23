using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBlocks : MonoBehaviour {

    public bool is_moving = false;
    public float gravity;
    public float velocity_x;
    public float velocity_y;
    public float speed;
    public float timeout;
    private float throw_timer = 0.0f;
    private RectTransform tf;
    private Vector3 init_position;
    private Quaternion init_rotation;
	// Use this for initialization
	void Start () {
        tf = GetComponent<RectTransform>();
        init_position = tf.localPosition;
        init_rotation = tf.localRotation;
	}
	
	// Update is called once per frame
	void Update () {
        if (is_moving == true)
        {
            Vector3 v = new Vector3(velocity_x, velocity_y, 0);
            Vector3 g = new Vector3(0, gravity, 0);
            tf.localPosition = tf.localPosition + v* throw_timer + 0.5f * g * throw_timer* throw_timer;
            throw_timer += speed;
            tf.Rotate(Vector3.back * throw_timer);
            if(throw_timer > timeout)
            {
                is_moving = false;
                throw_timer = 0.0f;
                tf.localPosition = init_position;
                tf.localRotation = init_rotation;
                this.transform.gameObject.SetActive(false);
            }
        }
	}

    public void Fall() //take damage
    {
        throw_timer = 1;
        is_moving = true;
    }

    public void Reset()
    {
        is_moving = false;
        throw_timer = 0.0f;
        tf.localPosition = init_position;
        tf.localRotation = init_rotation;
    }
}
