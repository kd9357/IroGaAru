using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SmoothCamera2D : MonoBehaviour {

    public float SmoothTimeX;
    public float SmoothTimeY;
    public Transform Target;
    [Tooltip("Horizontal offset camera centers on")]
    public float XOffset = 0;
    [Tooltip("Vertical offset camera centers on")]
    public float YOffset = 0;

    public bool CameraBounds;

    [Tooltip("The minimum x, y and z the camera may go to in world coordinates")]
    public Vector3 MinCameraPos;
    [Tooltip("The maximum x, y and z the camera may go to in world coordinates")]
    public Vector3 MaxCameraPos;

    private Camera _camera;
    private Vector2 _velocity;

    void Start()
    {
        _camera = GetComponent<Camera>();    
    }

    private void FixedUpdate()
    {
        Vector3 center = Target.position + new Vector3(XOffset, YOffset, 0);
        float posX = Mathf.SmoothDamp(_camera.transform.position.x, center.x, ref _velocity.x, SmoothTimeX);
        float posY = Mathf.SmoothDamp(_camera.transform.position.y, center.y, ref _velocity.y, SmoothTimeY);

        _camera.transform.position = new Vector3(posX, posY, _camera.transform.position.z);

        if(CameraBounds)
        {
            _camera.transform.position = new Vector3(Mathf.Clamp(_camera.transform.position.x, MinCameraPos.x, MaxCameraPos.x),
                Mathf.Clamp(_camera.transform.position.x, MinCameraPos.x, MaxCameraPos.x),
                Mathf.Clamp(_camera.transform.position.z, MinCameraPos.z, MaxCameraPos.z));

        }
    }



}
