using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GameOfLifeController : MonoBehaviour
{
    Camera _camera;
    Vector3 previousMousePosition;

    //get camera component in start
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    //camera controls
    void Update()
    {
        //zoom the camera, and restrict it's zoom level within reasonable numbers
        _camera.orthographicSize *= -Input.mouseScrollDelta.y * 0.1f + 1;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize,0.0001f,1.0f);
        if (Input.GetMouseButtonDown(2))
        {
            previousMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            //calculate camera movement and apply it
            Vector3 lastWorld = _camera.ScreenToWorldPoint(previousMousePosition);
            Vector3 nowWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = lastWorld - nowWorld;
            transform.position += new Vector3(delta.x, delta.y);
            previousMousePosition = Input.mousePosition;
            //restrict camera position
            Vector3 camPos = _camera.transform.position;
            _camera.transform.position = new Vector3(Mathf.Clamp(camPos.x,-0.5f, 0.5f),Mathf.Clamp(camPos.y,-0.5f, 0.5f),camPos.z);
        }

    }
}

