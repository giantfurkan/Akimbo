using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private InputDataSO input;

    private float movementHorizontal;
    private float movementVertical;

    private Vector3 direction;

    private Rigidbody rigidbody;
    private Entity thisBody;

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();

        if (thisBody == null)
            thisBody = GetComponent<Entity>();

        if (input == null)
            input = Resources.Load<InputDataSO>("Input");

    }

    void Update()
    {
        Move();
    }

    private void FixedUpdate()
    {
        FixedMovement();
    }

    private void Move()
    {
        movementHorizontal = input.value.x;
        movementVertical = input.value.y;

        float temp = Mathf.Max(Mathf.Abs(movementHorizontal), Mathf.Abs(movementVertical));
        Vector2 inputMovement = new Vector2(movementHorizontal, movementVertical).normalized;
        inputMovement *= temp;
        direction = new Vector3(inputMovement.x, 0, inputMovement.y);
    }

    private void FixedMovement()
    {
        if (!Gunner.inRange)
        {
            transform.LookAt(transform.position + direction);
        }

        rigidbody.velocity = direction * thisBody.Speed;
        if (direction == Vector3.zero)
        {
            rigidbody.angularVelocity = direction;
        }
    }
  
}
