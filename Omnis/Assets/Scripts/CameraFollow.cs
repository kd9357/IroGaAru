using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Taken from https://www.youtube.com/watch?time_continue=1&v=4qE8cuHI93c time 10:25

public class CameraFollow : MonoBehaviour {

    [Tooltip("Distance in the x axis the player can move before the camera follows")]
    public float XMargin = 1f;
    [Tooltip("Distance in the y axis the player can move before the camera follows")]
    public float YMargin = 1f;
    [Tooltip("How smoothly the camera catches up with it's target movement in the x axis (larger values = less smooth)")]
    public float XSmooth = 8f;
    [Tooltip("How smoothly the camera catches up with it's target movement in the y axis (larger values = less smooth)")]
    public float YSmooth = 8f;
    [Tooltip("Check if Camera has minimum and maximum range")]
    public bool CameraBounded = false;
    [Tooltip("The minimum x and y coordinates the camera can have")]
    public Vector2 MinXY;
    [Tooltip("The maximum x and y coordinates the camera can have")]
    public Vector2 MaxXY;

    private Transform _target;

    //Used for changing target
    private bool _deadZone;

    void Awake () {
        _target = GameObject.FindGameObjectWithTag("Player").transform; 
	}
	
    bool CheckXMargin()
    {
        return Mathf.Abs(transform.position.x - _target.position.x) > XMargin;
    }

    bool CheckYMargin()
    {
        return Mathf.Abs(transform.position.y - _target.position.y) > YMargin;
    }

    void FixedUpdate () {
        TrackTarget();
	}

    void TrackTarget()
    {
        // By default the target x and y coordinates of the camera are it's current x and y coordinates
        float targetX = transform.position.x;
        float targetY = transform.position.y;

        //Check if outside deadzone
        if (!_deadZone || CheckXMargin())
            targetX = Mathf.Lerp(transform.position.x, _target.position.x, XSmooth * Time.deltaTime);
        if (!_deadZone || CheckYMargin())
            targetY = Mathf.Lerp(transform.position.y, _target.position.y, YSmooth * Time.deltaTime);

        //Clamp to Min & MaxXY
        if(CameraBounded)
        {
            targetX = Mathf.Clamp(targetX, MinXY.x, MaxXY.x);
            targetY = Mathf.Clamp(targetY, MinXY.y, MaxXY.y);
        }

        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }

    public void SetTarget(Transform newTarget, bool useDeadZone)
    {
        _target = newTarget;
        _deadZone = useDeadZone;
    }
}
