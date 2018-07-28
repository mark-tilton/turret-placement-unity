using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float _speed = 10;

    public float _gravity = 9.8f;

    public float _jumpVelocity = 250;

    public float _lookSensitivity = 1;

    public AnimationCurve _stopReaction;

    private CharacterController _controller;

    private Camera _camera;

    private float _verticalSpeed = 0;

    // Use this for initialization
    void Start()
    {
        _controller = transform.GetComponent<CharacterController>();
        _camera = transform.GetChild(1).GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate gravity
        _verticalSpeed -= _gravity;
        if (_controller.isGrounded)
        {
            _verticalSpeed = 0;
            if (Input.GetButton("Jump"))
            {
                _verticalSpeed = _jumpVelocity;
            }
        }
        var gravityVector = new Vector3(0, _verticalSpeed, 0);

        // Calculate movement
        var forward = Input.GetAxis("Vertical");
        var side = Input.GetAxis("Horizontal");
        var movementDirection = new Vector3(side, 0, forward);
        var movementVector = _camera.transform.rotation
                           * Vector3.ClampMagnitude(movementDirection, 1)
                           * _speed;

        // Move controller
        _controller.Move((movementVector + gravityVector) * Time.deltaTime);

        // Handle Cursor for testing.
        if (Input.GetButton("Cancel"))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (Cursor.lockState == CursorLockMode.None)
        {
            return;
        }
        var xRot = Input.GetAxis("Mouse X");
        var yRot = Input.GetAxis("Mouse Y");
        _camera.transform.rotation = Quaternion.Euler(
            new Vector3(-yRot, xRot, 0)
            * _lookSensitivity
            + _camera.transform.rotation.eulerAngles);
    }
}
