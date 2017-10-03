using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Taken from https://www.youtube.com/watch?time_continue=1&v=4qE8cuHI93c time 10:25

public class CameraFollow : MonoBehaviour {

    [Tooltip("Distance in the player can move right before the camera follows")]
    public float PositiveXMargin = 1f;
    [Tooltip("Distance in the player can move left before the camera follows")]
    public float NegativeXMargin = 1f;
    [Tooltip("Distance in the player can move up before the camera follows")]
    public float PositiveYMargin = 1f;
    [Tooltip("Distance in the player can move down before the camera follows")]
    public float NegativeYMargin = 1f;
    [Tooltip("How smoothly the camera catches up with it's target movement in the x axis (larger values = less smooth)")]
    public float XSmooth = 8f;
    [Tooltip("How smoothly the camera catches up with it's target movement in the y axis (larger values = less smooth)")]
    public float YSmooth = 8f;
    [Tooltip("The offset horizontal to the player the camera will center on")]
    public float XCenterOffset;
    [Tooltip("The offset vertical to the player the camera will center on")]
    public float YCenterOffset;
    [Tooltip("Check if Camera has minimum and maximum range")]
    public bool CameraBounded = false;
    [Tooltip("The minimum x and y coordinates the camera can have")]
    public Vector2 MinXY;
    [Tooltip("The maximum x and y coordinates the camera can have")]
    public Vector2 MaxXY;

    private Transform _target;

    //Used for changing target
    private bool _playerLocked;

    void Awake () {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _playerLocked = true;
	}
	
    //Return true if out of bounds
    bool CheckXMargin()
    {
        //return Mathf.Abs(transform.position.x - _target.position.x) > XMargin;
        if (transform.position.x > _target.position.x + XCenterOffset)
            return transform.position.x - _target.position.x + XCenterOffset > NegativeXMargin;
        else if (transform.position.x < _target.position.x + XCenterOffset)
            return _target.position.x - transform.position.x + XCenterOffset > PositiveXMargin;
        return false;
    }

    bool CheckYMargin()
    {
        //return Mathf.Abs(transform.position.y - _target.position.y) > YMargin;
        if (transform.position.y > _target.position.y + YCenterOffset)
            return transform.position.y - _target.position.y + YCenterOffset > NegativeYMargin;
        else if (transform.position.y < _target.position.y + YCenterOffset)
            return _target.position.y - transform.position.y + YCenterOffset > PositiveYMargin;
        return false;
    }

    void FixedUpdate () {
        TrackTarget();
	}

    void TrackTarget()
    {
        // By default the target x and y coordinates of the camera are it's current x and y coordinates
        float targetX = transform.position.x;
        float targetY = transform.position.y;

        //If locked to player, use deadzone + offset
        if(_playerLocked)
        {
            Vector2 xdist = _target.transform.position;
            Vector2 ydist = xdist;
            xdist.x += XCenterOffset;
            ydist.y += YCenterOffset;
            if (CheckXMargin())
            {
                float dist = Vector2.Distance(transform.position, xdist);
                targetX = Mathf.Lerp(transform.position.x, _target.position.x + XCenterOffset, dist * XSmooth * Time.deltaTime);
            }
            if (CheckYMargin())
            {
                float dist = Vector2.Distance(transform.position, ydist);
                targetY = Mathf.Lerp(transform.position.y, _target.position.y + YCenterOffset, dist * YSmooth * Time.deltaTime);
            }
        }
        //Else don't check deadzone or use offset
        else
        {
            targetX = Mathf.Lerp(transform.position.x, _target.position.x, XSmooth * Time.deltaTime);
            targetY = Mathf.Lerp(transform.position.y, _target.position.y, YSmooth * Time.deltaTime);
        }

        //Clamp to Min & MaxXY
        if(CameraBounded)
        {
            targetX = Mathf.Clamp(targetX, MinXY.x, MaxXY.x);
            targetY = Mathf.Clamp(targetY, MinXY.y, MaxXY.y);
        }

        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }

    public void SetTarget(Transform newTarget, bool lockToPlayer)
    {
        _target = newTarget;
        _playerLocked = lockToPlayer;
    }
}
