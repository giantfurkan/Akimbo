using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSInputController : MonoBehaviour
{
    private CharacterMotor motor;

    private void Awake()
    {
        motor = GetComponent<CharacterMotor>();
    }

    private void Update()
    {
        var directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (directionVector != Vector3.zero)
        {
            var directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;

            directionLength = Mathf.Min(1, directionLength);

            directionLength = directionLength * directionLength;

            directionVector = directionVector * directionLength;
        }

        motor.inputMoveDirection = transform.rotation * directionVector;
        motor.inputJump = Input.GetButton("Jump");
    }
}