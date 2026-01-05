using UnityEngine;
using Eraflo.Catalyst.ProceduralAnimation.Components.Locomotion;

namespace Eraflo.Catalyst.ProceduralAnimation.Components
{
    /// <summary>
    /// Simple character controller for testing procedural locomotion.
    /// Add this to the same GameObject as ProceduralLocomotion.
    /// Uses WASD/Arrow keys for movement.
    /// </summary>
    [AddComponentMenu("Catalyst/Procedural Animation/Simple Character Controller")]
    [RequireComponent(typeof(ProceduralLocomotion))]
    public class SimpleCharacterController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Movement speed in units per second.")]
        [SerializeField] private float _moveSpeed = 3f;
        
        [Tooltip("Rotation speed in degrees per second.")]
        [SerializeField] private float _rotationSpeed = 180f;
        
        [Tooltip("Use tank controls (rotate in place) instead of direct movement.")]
        [SerializeField] private bool _tankControls = false;
        
        [Header("Gravity")]
        [Tooltip("Apply gravity to the character.")]
        [SerializeField] private bool _useGravity = true;
        
        [Tooltip("Gravity force.")]
        [SerializeField] private float _gravity = 20f;
        
        [Tooltip("Ground check distance.")]
        [SerializeField] private float _groundCheckDistance = 0.2f;
        
        [Tooltip("Ground layer mask.")]
        [SerializeField] private LayerMask _groundMask = ~0;
        
        [Header("References")]
        [SerializeField] private ProceduralLocomotion _locomotion;
        
        private CharacterController _characterController;
        private bool _useCharacterController;
        private float _verticalVelocity;
        private bool _isGrounded;
        
        private void Awake()
        {
            if (_locomotion == null)
                _locomotion = GetComponent<ProceduralLocomotion>();
            
            _characterController = GetComponent<CharacterController>();
            _useCharacterController = _characterController != null;
        }
        
        private void Update()
        {
            // Ground check
            CheckGround();
            
            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            Vector3 moveDirection;
            float currentSpeed = _moveSpeed;
            
            // Sprint with Shift
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = _moveSpeed * 2f;
            }
            
            if (_tankControls)
            {
                // Tank controls: W/S = forward/back, A/D = rotate
                transform.Rotate(0, horizontal * _rotationSpeed * Time.deltaTime, 0);
                moveDirection = transform.forward * vertical;
            }
            else
            {
                // Direct controls: move in input direction
                moveDirection = new Vector3(horizontal, 0, vertical);
                
                if (moveDirection.magnitude > 0.1f)
                {
                    moveDirection.Normalize();
                    
                    // Rotate character to face movement direction
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, 
                        targetRotation, 
                        _rotationSpeed * Time.deltaTime
                    );
                }
            }
            
            // Apply gravity
            if (_useGravity)
            {
                if (_isGrounded && _verticalVelocity < 0)
                {
                    _verticalVelocity = -2f; // Small downward force to keep grounded
                }
                else
                {
                    _verticalVelocity -= _gravity * Time.deltaTime;
                }
            }
            
            // Build final velocity
            Vector3 velocity = moveDirection.normalized * currentSpeed;
            velocity.y = _verticalVelocity;
            
            // Move the character
            if (_useCharacterController)
            {
                _characterController.Move(velocity * Time.deltaTime);
                _isGrounded = _characterController.isGrounded;
            }
            else
            {
                // Direct transform movement with gravity
                transform.position += velocity * Time.deltaTime;
            }
            
            // Send input to locomotion system
            _locomotion.MovementInput = moveDirection;
            _locomotion.Speed = currentSpeed;
        }
        
        private void CheckGround()
        {
            if (_useCharacterController)
            {
                _isGrounded = _characterController.isGrounded;
            }
            else
            {
                // Manual ground check with raycast
                _isGrounded = UnityEngine.Physics.Raycast(
                    transform.position + Vector3.up * 0.1f,
                    Vector3.down,
                    _groundCheckDistance + 0.1f,
                    _groundMask
                );
            }
        }
    }
}
