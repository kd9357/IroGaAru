using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxing : MonoBehaviour {

    public Transform[] Backgrounds;
    public float Smoothing = 1f; //must be greater than 0

    private float[] _parallaxScales;
    private Transform _cameraPos;
    private Vector3 _lastCameraPos;

    private void Start()
    {
        _cameraPos = Camera.main.transform;
        _lastCameraPos = _cameraPos.position;
        _parallaxScales = new float[Backgrounds.Length];
        for (int i = 0; i < Backgrounds.Length; i++)
        {
            _parallaxScales[i] = Backgrounds[i].position.z * -1;
        }
    }

    private void Update()
    {
        for (int i = 0; i < Backgrounds.Length; i++)
        {
            //float parallax = (_lastCameraPos.x - _cameraPos.position.x) * _parallaxScales[i];
            //float backgroundTargetPosX = Backgrounds[i].position.x + parallax;
            //Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, Backgrounds[i].position.y, Backgrounds[i].position.z);
            //Backgrounds[i].position = Vector3.Lerp(Backgrounds[i].position, backgroundTargetPos, Smoothing * Time.deltaTime);

            Vector3 parallax = (_lastCameraPos - _cameraPos.position) * _parallaxScales[i];
            Vector3 backgroundTargetPos = new Vector3(Backgrounds[i].position.x + parallax.x, Backgrounds[i].position.y + parallax.y, Backgrounds[i].position.z);
            Backgrounds[i].position = Vector3.Lerp(Backgrounds[i].position, backgroundTargetPos, Smoothing * Time.deltaTime);
        }

        _lastCameraPos = _cameraPos.position;
    }

}
