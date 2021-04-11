using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Character/Character Motor")]
public class CharacterMotor : MonoBehaviour
{
    public bool canControl = true;
    public bool useFixedUpdate = true;

    [System.NonSerialized]
    public Vector3 inputMoveDirection = Vector3.zero;

    [System.NonSerialized]
    public bool inputJump = false;

    public class CharacterMotorMovement
    {
        public float maxForwardSpeed = 10f;
        public float maxSidewaysSpeed = 10f;
        public float maxBackwardsSpeed = 10f;

        public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));

        public float maxGroundAcceleration = 30f;
        public float maxAirAcceleration = 20f;

        public float gravity = 10f;
        public float maxFallSpeed = 20f;

        [System.NonSerialized]
        public CollisionFlags collisionFlags;

        [System.NonSerialized]
        public Vector3 velocity;

        [System.NonSerialized]
        public Vector3 frameVelocity = Vector3.zero;

        [System.NonSerialized]
        public Vector3 hitPoint = Vector3.zero;

        [System.NonSerialized]
        public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0f, 0f);
    }

    public CharacterMotorMovement movement = new CharacterMotorMovement();

    public enum MovementTransferOnJump
    {
        None,
        InitTransfer,
        PermaTransfer,
        PermaLocked
    }

    public class CharacterMotorJumping
    {
        public bool enabled = true;
        public float baseHeight = 1f;
        public float extraHeight = 4.1f;
        public float perpAmount = 0f;
        public float steepPerpAmount = 0.5f;

        [System.NonSerialized]
        public bool jumping = false;

        [System.NonSerialized]
        public bool holdingJumpButton = false;

        [System.NonSerialized]
        public float lastStartTime = 0f;

        [System.NonSerialized]
        public float lastButtonDownTime = -100f;

        [System.NonSerialized]
        public Vector3 jumpDir = Vector3.up;
    }

    public CharacterMotorJumping jumping = new CharacterMotorJumping();

    public class CharacterMotorMovingPlatform
    {
        public bool enabled = true;

        public MovementTransferOnJump movementTransfer = MovementTransferOnJump.PermaTransfer;

        [System.NonSerialized]
        public Transform hitPlatform;

        [System.NonSerialized]
        public Transform activePlatform;

        [System.NonSerialized]
        public Vector3 activeLocalPoint;

        [System.NonSerialized]
        public Vector3 activeGlobalPoint;

        [System.NonSerialized]
        public Quaternion activeLocalRotation;

        [System.NonSerialized]
        public Quaternion activeGlobalRotation;

        [System.NonSerialized]
        public Matrix4x4 lastMatrix;

        [System.NonSerialized]
        public Vector3 platformVelocity;

        [System.NonSerialized]
        public bool newPlatform;
    }

    public CharacterMotorMovingPlatform movingPlatform = new CharacterMotorMovingPlatform();

    public class CharacterMotorSliding
    {
        public bool enabled = true;

        public float slidingSpeed = 15f;

        public float sidewaysControl = 1f;

        public float speedControl = 0.4f;
    }

    public CharacterMotorSliding sliding = new CharacterMotorSliding();

    [System.NonSerialized]
    public bool grounded = true;

    [System.NonSerialized]
    public Vector3 groundNormal = Vector3.zero;

    private Vector3 lastGroundNormal = Vector3.zero;
    private Transform tr;
    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        tr = transform;
    }

    private void UpdateFunction()
    {
        var velocity = movement.velocity;

        velocity = ApplyInputVelocityChange(velocity);

        velocity = ApplyGravityAndJumping(velocity);

        var moveDistance = Vector3.zero;
        if (MoveWithPlatform())
        {
            var newGlobalPoint = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);
            moveDistance = (newGlobalPoint - movingPlatform.activeGlobalPoint);
            if (moveDistance != Vector3.zero)
                controller.Move(moveDistance);

            var newGlobalRotation = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
            var rotationDiff = newGlobalRotation * Quaternion.Inverse(movingPlatform.activeGlobalRotation);

            var yRotation = rotationDiff.eulerAngles.y;
            if (yRotation != 0)
            {
                tr.Rotate(0, yRotation, 0);
            }
        }

        var lastPosition = tr.position;

        var currentMovementOffset = velocity * Time.deltaTime;

        var pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
        if (grounded)
            currentMovementOffset -= pushDownOffset * Vector3.up;

        movingPlatform.hitPlatform = null;
        groundNormal = Vector3.zero;

        movement.collisionFlags = controller.Move(currentMovementOffset);

        movement.lastHitPoint = movement.hitPoint;
        lastGroundNormal = groundNormal;

        if (movingPlatform.enabled && movingPlatform.activePlatform != movingPlatform.hitPlatform)
        {
            if (movingPlatform.hitPlatform != null)
            {
                movingPlatform.activePlatform = movingPlatform.hitPlatform;
                movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
                movingPlatform.newPlatform = true;
            }
        }

        var oldHVelocity = new Vector3(velocity.x, 0, velocity.z);
        movement.velocity = (tr.position - lastPosition) / Time.deltaTime;
        var newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);

        if (oldHVelocity == Vector3.zero)
        {
            movement.velocity = new Vector3(0, movement.velocity.y, 0);
        }
        else
        {
            var projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
            movement.velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + movement.velocity.y * Vector3.up;
        }

        if (movement.velocity.y < velocity.y - 0.001)
        {
            if (movement.velocity.y < 0)
            {
                movement.velocity.y = velocity.y;
            }
            else
            {
                jumping.holdingJumpButton = false;
            }
        }

        if (grounded && !IsGroundedTest())
        {
            grounded = false;

            if (movingPlatform.enabled &&
                (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
            )
            {
                movement.frameVelocity = movingPlatform.platformVelocity;
                movement.velocity += movingPlatform.platformVelocity;
            }

            SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
            tr.position += pushDownOffset * Vector3.up;
        }
        else if (!grounded && IsGroundedTest())
        {
            grounded = true;
            jumping.jumping = false;
            StartCoroutine(SubtractNewPlatformVelocity());

            SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
        }

        if (MoveWithPlatform())
        {
            // Use the center of the lower half sphere of the capsule as reference point.
            // This works best when the character is standing on moving tilting platforms. 
            movingPlatform.activeGlobalPoint = tr.position + Vector3.up * (controller.center.y - controller.height * 0.5f + controller.radius);
            movingPlatform.activeLocalPoint = movingPlatform.activePlatform.InverseTransformPoint(movingPlatform.activeGlobalPoint);

            // Support moving platform rotation as well:
            movingPlatform.activeGlobalRotation = tr.rotation;
            movingPlatform.activeLocalRotation = Quaternion.Inverse(movingPlatform.activePlatform.rotation) * movingPlatform.activeGlobalRotation;
        }
    }

    private void FixedUpdate()
    {
        if (movingPlatform.enabled)
        {
            if (movingPlatform.activePlatform != null)
            {
                if (!movingPlatform.newPlatform)
                {
                    movingPlatform.platformVelocity = (
                        movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)
                        - movingPlatform.lastMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)
                    ) / Time.deltaTime;
                }
                movingPlatform.lastMatrix = movingPlatform.activePlatform.localToWorldMatrix;
                movingPlatform.newPlatform = false;
            }
            else
            {
                movingPlatform.platformVelocity = Vector3.zero;
            }
        }

        if (useFixedUpdate)
            UpdateFunction();
    }

    private void Update()
    {
        if (!useFixedUpdate)
            UpdateFunction();
    }

    private Vector3 ApplyInputVelocityChange(Vector3 velocity)
    {
        if (!canControl)
            inputMoveDirection = Vector3.zero;

        Vector3 desiredVelocity;
        if (grounded && TooSteep())
        {
            desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
            var projectedMoveDir = Vector3.Project(inputMoveDirection, desiredVelocity);
            desiredVelocity = desiredVelocity + projectedMoveDir * sliding.speedControl + (inputMoveDirection - projectedMoveDir) * sliding.sidewaysControl;
            desiredVelocity *= sliding.slidingSpeed;
        }
        else
            desiredVelocity = GetDesiredHorizontalVelocity();

        if (movingPlatform.enabled && movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
        {
            desiredVelocity += movement.frameVelocity;
            desiredVelocity.y = 0;
        }

        if (grounded)
            desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
        else
            velocity.y = 0;

        var maxVelocityChange = GetMaxAcceleration(grounded) * Time.deltaTime;
        var velocityChangeVector = (desiredVelocity - velocity);
        if (velocityChangeVector.sqrMagnitude > maxVelocityChange * maxVelocityChange)
        {
            velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;
        }

        if (grounded || canControl)
            velocity += velocityChangeVector;

        if (grounded)
        {
            velocity.y = Mathf.Min(velocity.y, 0);
        }

        return velocity;
    }

    private Vector3 ApplyGravityAndJumping(Vector3 velocity)
    {

        if (!inputJump || !canControl)
        {
            jumping.holdingJumpButton = false;
            jumping.lastButtonDownTime = -100;
        }

        if (inputJump && jumping.lastButtonDownTime < 0 && canControl)
            jumping.lastButtonDownTime = Time.time;

        if (grounded)
            velocity.y = Mathf.Min(0, velocity.y) - movement.gravity * Time.deltaTime;
        else
        {
            velocity.y = movement.velocity.y - movement.gravity * Time.deltaTime;

            if (jumping.jumping && jumping.holdingJumpButton)
            {
                if (Time.time < jumping.lastStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight))
                {
                    velocity += jumping.jumpDir * movement.gravity * Time.deltaTime;
                }
            }

            velocity.y = Mathf.Max(velocity.y, -movement.maxFallSpeed);
        }

        if (grounded)
        {
            if (jumping.enabled && canControl && (Time.time - jumping.lastButtonDownTime < 0.2))
            {
                grounded = false;
                jumping.jumping = true;
                jumping.lastStartTime = Time.time;
                jumping.lastButtonDownTime = -100;
                jumping.holdingJumpButton = true;

                if (TooSteep())
                    jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);
                else
                    jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);

                velocity.y = 0;
                velocity += jumping.jumpDir * CalculateJumpVerticalSpeed(jumping.baseHeight);

                if (movingPlatform.enabled &&
                    (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                    movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
                )
                {
                    movement.frameVelocity = movingPlatform.platformVelocity;
                    velocity += movingPlatform.platformVelocity;
                }

                SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                jumping.holdingJumpButton = false;
            }
        }

        return velocity;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0 && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0)
        {
            if ((hit.point - movement.lastHitPoint).sqrMagnitude > 0.001 || lastGroundNormal == Vector3.zero)
                groundNormal = hit.normal;
            else
                groundNormal = lastGroundNormal;

            movingPlatform.hitPlatform = hit.collider.transform;
            movement.hitPoint = hit.point;
            movement.frameVelocity = Vector3.zero;
        }
    }

    private IEnumerator SubtractNewPlatformVelocity()
    {
        // When landing, subtract the velocity of the new ground from the character's velocity
        // since movement in ground is relative to the movement of the ground.
        if (movingPlatform.enabled &&
            (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
            movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
        )
        {
            // If we landed on a new platform, we have to wait for two FixedUpdates
            // before we know the velocity of the platform under the character
            if (movingPlatform.newPlatform)
            {
                var platform = movingPlatform.activePlatform;
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                if (grounded && platform == movingPlatform.activePlatform)
                    yield return null;
            }
            movement.velocity -= movingPlatform.platformVelocity;
        }
    }

    private bool MoveWithPlatform()
    {
        return (
            movingPlatform.enabled
            && (grounded || movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked)
            && movingPlatform.activePlatform != null
        );
    }

    private Vector3 GetDesiredHorizontalVelocity()
    {
        // Find desired velocity
        var desiredLocalDirection = tr.InverseTransformDirection(inputMoveDirection);
        var maxSpeed = MaxSpeedInDirection(desiredLocalDirection);
        if (grounded)
        {
            // Modify max speed on slopes based on slope speed multiplier curve
            var movementSlopeAngle = Mathf.Asin(movement.velocity.normalized.y) * Mathf.Rad2Deg;
            maxSpeed *= movement.slopeSpeedMultiplier.Evaluate(movementSlopeAngle);
        }
        return tr.TransformDirection(desiredLocalDirection * maxSpeed);
    }

    private Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
    {
        var sideways = Vector3.Cross(Vector3.up, hVelocity);
        return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;
    }

    private bool IsGroundedTest()
    {
        return (groundNormal.y > 0.01);
    }

    private float GetMaxAcceleration(bool grounded)
    {
        if (grounded)
            return movement.maxGroundAcceleration;
        else
            return movement.maxAirAcceleration;
    }

    private float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * targetJumpHeight * movement.gravity);
    }

    private bool IsJumping()
    {
        return jumping.jumping;
    }

    private bool IsSliding()
    {
        return (grounded && sliding.enabled && TooSteep());
    }

    private bool IsTouchingCeiling()
    {
        return (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0;
    }

    private bool IsGrounded()
    {
        return grounded;
    }

    private bool TooSteep()
    {
        return (groundNormal.y <= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad));
    }

    private Vector3 GetDirection()
    {
        return inputMoveDirection;
    }

    private void SetControllable(bool controllable)
    {
        canControl = controllable;
    }

    // Project a direction onto elliptical quater segments based on forward, sideways, and backwards speed.
    // The function returns the length of the resulting vector.
    private float MaxSpeedInDirection(Vector3 desiredMovementDirection)
    {
        if (desiredMovementDirection == Vector3.zero)
            return 0;
        else
        {
            var zAxisEllipseMultiplier = (desiredMovementDirection.z > 0 ? movement.maxForwardSpeed : movement.maxBackwardsSpeed) / movement.maxSidewaysSpeed;
            var temp = new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;
            var length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * movement.maxSidewaysSpeed;
            return length;
        }
    }

    private void SetVelocity(Vector3 velocity)
    {
        grounded = false;
        movement.velocity = velocity;
        movement.frameVelocity = Vector3.zero;
        SendMessage("OnExternalVelocity");
    }
}