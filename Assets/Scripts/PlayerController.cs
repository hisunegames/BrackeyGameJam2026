using System;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines.Interpolators;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CinemachinePanTilt _cineCamera;
    [SerializeField] InputActionReference _moveInputAction;
    [SerializeField] InputActionReference _jumpInputAction;
    [SerializeField] InputActionReference _crouchInputAction;
    [SerializeField] InputActionReference _sprintInputAction;

    [Header("Movement Settings")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _crouchMoveSpeed = 2.5f;
    [SerializeField] float _sprintSpeed = 8f;
    [SerializeField] float _jumpForce = 6f;
    [SerializeField] float _gravity = -20f;
    [SerializeField] float _crouchHeight = 1f;
    [SerializeField] float _crouchTransitionSpeed = 8f;
    [SerializeField] LayerMask _obstructionLayers = ~0;
    [SerializeField] float _pushForce = 2f;
    [SerializeField] float _maxStamina = 100f;
    [SerializeField] float _staminaDrainRate = 25f;
    [SerializeField] float _staminaRegenRate = 15f;
    [SerializeField] float _staminaRegenDelay = 1f;
    [SerializeField] CinemachineBasicMultiChannelPerlin _sprintShake;
    [SerializeField] float _sprintShakeAmplitude = 1f;
    [SerializeField] float _shakeBlendSpeed = 4f;
    [SerializeField] float _handShakeStrength = 0.02f;
    [SerializeField] int _handShakeVibrato = 10;
    [SerializeField] float _handShakeDuration = 0.3f;

    [Header("Health")]
    [SerializeField] float _maxHealth = 100f;
    [SerializeField] float _currentHealth = 100f;
    [SerializeField] float _natRegenDelay = 5f;
    [SerializeField] float _natRegenRate = 1f;

    [Header("UI")]
    [SerializeField] TMPro.TMP_Text _staminaText;
    [SerializeField] GameObject _playerHand;
    [SerializeField] float _staminaBarDisappearDelay = 2f;
    [SerializeField] GameObject _staminaBackground;
    [SerializeField] GameObject _staminaBar;
    [SerializeField] float _staminaRedThreshold;
    [SerializeField] GameObject _blackVignette;
    [SerializeField] GameObject _redVignette;
    [SerializeField] float _asthmaThreshold;
    [SerializeField] float _erythropsiaThreshold;
    [SerializeField] GameObject blackScreen;
    [SerializeField] GameObject gameoverScreen;

    CharacterController _controller;
    float _standingHeight;
    float _verticalVelocity;
    bool _isCrouching;
    bool _isSprinting;
    [SerializeField]
    float _currentStamina;
    float _regenDelayTimer;
    float _staminaBarDelayTimer;
    float _healthRegenDelayTimer;
    bool _isTalking;
    Vector3 _handRestPosition;
    Tween _handShakeTween;
    bool _isDead;
    Vector3 _lastGroundedPosition;

    public float StaminaFraction => _currentStamina / _maxStamina;
    public bool IsCrouching => _isCrouching;
    public Vector3 Motion { get; private set; }
    
    public bool IsTalking => _isTalking;
    public bool IsDead => _isDead;

    public static event Action OnPlayerDied;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _standingHeight = _controller.height;
        _currentStamina = _maxStamina;
        _currentHealth = _maxHealth;

        if (_playerHand != null)
            _handRestPosition = _playerHand.transform.localPosition;
    }

    private void OnEnable()
    {
        _moveInputAction.action.Enable();
        _jumpInputAction.action.Enable();
        _crouchInputAction.action.Enable();
        _sprintInputAction.action.Enable();
    }

    private void OnDisable()
    {
        _moveInputAction.action.Disable();
        _jumpInputAction.action.Disable();
        _crouchInputAction.action.Disable();
        _sprintInputAction.action.Disable();
    }

    void Update()
    {
        if (_isDead)
            return;

        // Get movement input and convert it to a 3D vector
        Vector2 moveInput = _moveInputAction.action.ReadValue<Vector2>();
        Vector3 moveInputIn3D = new Vector3(moveInput.x, 0, moveInput.y);

        // Take the pan angle from the CinemachinePanTilt component
        float panAngle = _cineCamera.PanAxis.Value;
        Quaternion panRotation = Quaternion.Euler(0, panAngle, 0);

        // Rotate the movement input based on the pan angle
        Vector3 moveDirection = panRotation * moveInputIn3D;
        transform.localRotation = panRotation;

        float crouchHeightDelta = HandleCrouch();
        float speed = HandleSprint(moveInputIn3D.sqrMagnitude > 0.0001f);
        HandleGravityAndJump();
        HandleSprintShake();
        HandleHealth();

        Vector3 motion = moveDirection * speed;
        motion.y = _verticalVelocity;
        Motion = motion;
        _controller.Move(motion * Time.deltaTime + Vector3.up * (crouchHeightDelta / 2f));

        // Display Player Stamina
        if (_staminaText != null)
            _staminaText.text = $"Stamina: {Mathf.RoundToInt(_currentStamina)}";
    }

    void HandleGravityAndJump()
    {
        if (_controller.isGrounded)
        {
            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (!_isCrouching && _jumpInputAction.action.WasPerformedThisFrame())
                _verticalVelocity = _jumpForce;

            _lastGroundedPosition = transform.position;
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }

    // Called by a fall-through safety volume placed below the map.
    public void TeleportToLastGrounded()
    {
        TeleportTo(_lastGroundedPosition);
    }

    public void TeleportTo(Vector3 position)
    {
        _controller.enabled = false;
        transform.position = position;
        _verticalVelocity = 0f;
        _controller.enabled = true;
    }

    float HandleSprint(bool isMoving)
    {
        bool wasSprinting = _isSprinting;
        bool wantsSprint = !_isTalking && isMoving && !_isCrouching && _currentStamina > 0f && _sprintInputAction.action.IsPressed();
        float t = 1f - Mathf.Clamp01((_currentStamina / _maxStamina) / _staminaRedThreshold);

        if (_staminaBar != null)
        {
            Color barColor = Color.Lerp(Color.white, Color.red, t);
            barColor.a = _staminaBar.GetComponent<Image>().color.a;
            _staminaBar.GetComponent<Image>().color = barColor;
            _staminaBar.transform.localScale = new Vector2(_currentStamina / _maxStamina, 1f);
        }

        if (_blackVignette != null)
        {
            bool debounce = false;
            if ((_currentStamina / _maxStamina) < _asthmaThreshold && debounce == false)
            {
                _blackVignette.GetComponent<Image>().DOKill();
                _blackVignette.GetComponent<Image>().DOFade(1f, 0.2f);
                debounce = true;
            }
            else
            {
                _blackVignette.GetComponent<Image>().DOFade(0f, 0.2f);
                debounce = false;
            }
        }

        if (wantsSprint)
        {
            _isSprinting = true;
            _currentStamina = Mathf.Max(0f, _currentStamina - _staminaDrainRate * Time.deltaTime);

            _regenDelayTimer = _staminaRegenDelay;
            _staminaBarDelayTimer = _staminaBarDisappearDelay;
            if (_staminaBackground != null && _staminaBar != null)
            {
                if (_staminaBackground.GetComponent<Image>().color.a == 0)
                {
                    _staminaBackground.GetComponent<Image>().DOFade(1f, 0.05f);
                    _staminaBar.GetComponent<Image>().DOFade(1f, 0.05f);
                }
                else
                {
                    _staminaBackground.GetComponent<Image>().DOKill();
                    _staminaBar.GetComponent<Image>().DOKill();
                    Color _staminaBackgroundColor = _staminaBackground.GetComponent<Image>().color;
                    _staminaBackgroundColor.a = 1;
                    _staminaBackground.GetComponent<Image>().color = _staminaBackgroundColor;
                    Color _staminaBarColor = _staminaBar.GetComponent<Image>().color;
                    _staminaBarColor.a = 1;
                    _staminaBar.GetComponent<Image>().color = _staminaBarColor;
                }
            }

            if (_playerHand != null)
                _playerHand.transform.DOScale(new Vector3(0.22f, 0.22f, 0.22f), 0.2f);

            if (!wasSprinting)
                StartHandShake();
        }
        else
        {
            _isSprinting = false;

            if (_playerHand != null)
                _playerHand.transform.DOScale(new Vector3(0.23f, 0.23f, 0.23f), 0.2f);

            if (wasSprinting)
                StopHandShake();

            if (_regenDelayTimer > 0f)
                _regenDelayTimer -= Time.deltaTime;
            else
                _currentStamina = Mathf.Min(_maxStamina, _currentStamina + _staminaRegenRate * Time.deltaTime);

            if (_staminaBarDelayTimer > 0f)
                _staminaBarDelayTimer -= Time.deltaTime;
            else if (_staminaBar != null && _staminaBackground != null)
            {
                _staminaBar.GetComponent<Image>().DOFade(0f, 1f);
                _staminaBackground.GetComponent<Image>().DOFade(0f, 1f);
            }
        }

        return _isSprinting ? _sprintSpeed : (_isCrouching ? _crouchMoveSpeed : _moveSpeed);
    }

    void StartHandShake()
    {
        if (_playerHand == null)
            return;

        _handShakeTween?.Kill();
        _handShakeTween = _playerHand.transform.DOShakePosition(_handShakeDuration, _handShakeStrength, _handShakeVibrato)
            .SetLoops(-1, LoopType.Restart);
    }

    void StopHandShake()
    {
        if (_playerHand == null)
            return;

        _handShakeTween?.Kill();
        _handShakeTween = null;
        _playerHand.transform.DOLocalMove(_handRestPosition, 0.15f);
    }

    void HandleSprintShake()
    {
        if (_sprintShake == null)
            return;

        float targetAmplitude = _isSprinting ? _sprintShakeAmplitude : 0f;
        _sprintShake.AmplitudeGain = Mathf.MoveTowards(_sprintShake.AmplitudeGain, targetAmplitude, _shakeBlendSpeed * Time.deltaTime);
    }

    float HandleCrouch()
    {
        bool wantsCrouch = _crouchInputAction.action.IsPressed();

        if (wantsCrouch && !_isCrouching)
            _isCrouching = true;
        else if (!wantsCrouch && _isCrouching && CanStandUp())
            _isCrouching = false;

        float targetHeight = _isCrouching ? _crouchHeight : _standingHeight;
        float previousHeight = _controller.height;
        float newHeight = Mathf.MoveTowards(previousHeight, targetHeight, _crouchTransitionSpeed * Time.deltaTime);
        _controller.height = newHeight;
        return newHeight - previousHeight;
    }

    bool CanStandUp()
    {
        float currentTop = _controller.height / 2f;
        float checkDistance = _standingHeight / 2f - currentTop;
        if (checkDistance <= 0f)
            return true;

        Vector3 origin = transform.position + _controller.center + Vector3.up * currentTop;
        return !Physics.SphereCast(origin, _controller.radius * 0.9f, Vector3.up, out _, checkDistance, _obstructionLayers, QueryTriggerInteraction.Ignore);
    }

     void HandleHealth()
    {

        _healthRegenDelayTimer -= Time.deltaTime;
        if (_healthRegenDelayTimer <= 0)
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + _natRegenRate * Time.deltaTime);

        bool debounce = false;
        if ((_currentHealth / _maxHealth) <= _erythropsiaThreshold && debounce == false)
        {
            debounce = true;
            _redVignette.GetComponent<Image>().DOKill();
            _redVignette.GetComponent<Image>().DOFade(1f, 0.2f);
        }
        else
        {
            _redVignette.GetComponent<Image>().DOFade(0f, 2f);
            debounce = false;
        }
            
    }
    public void TakeDamage(float damage)
    {
        if (_isDead)
            return;

        _currentHealth -= damage;
        _healthRegenDelayTimer = _natRegenDelay;
        if (_currentHealth <= 0f)
            Die();
    }

    public void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        _currentHealth = 0f;
        _isSprinting = false;
        _playerHand.SetActive(false);

        StopHandShake();
        _sprintShake.AmplitudeGain = 0f;

        //Debug.Log("[PlayerController] Player died - no Game Over screen yet, this is a placeholder");
        StartCoroutine(ShowBlackThenGameOver());
        OnPlayerDied?.Invoke();
    }

    IEnumerator ShowBlackThenGameOver()
    {
        yield return new WaitForSeconds(2f);
        blackScreen.SetActive(true);
        yield return new WaitForSeconds(1f);
        gameoverScreen.SetActive(true);
        blackScreen.SetActive(false);
    }

    public void ApplyBuff(BuffType type, float amount)
    {
        switch (type)
        {
            case BuffType.MoveSpeed:
                _moveSpeed += amount;
                break;
            case BuffType.SprintSpeed:
                _sprintSpeed += amount;
                break;
            case BuffType.MaxStamina:
                _maxStamina += amount;
                _currentStamina += amount;
                break;
            case BuffType.MaxHealth:
                _maxHealth += amount;
                _currentHealth += amount;
                break;
        }
    }

    public void RemoveBuff(BuffType type, float amount)
    {
        switch (type)
        {
            case BuffType.MoveSpeed:
                _moveSpeed -= amount;
                break;
            case BuffType.SprintSpeed:
                _sprintSpeed -= amount;
                break;
            case BuffType.MaxStamina:
                _maxStamina -= amount;
                _currentStamina = Mathf.Min(_currentStamina, _maxStamina);
                break;
            case BuffType.MaxHealth:
                _maxHealth -= amount;
                _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
                break;
        }
    }

    // CharacterController doesn't push Rigidbodies on its own (and, having no Rigidbody itself,
    // can never be pushed back), so the shove has to be applied by hand on every contact.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
            return;

        if (hit.moveDirection.y < -0.3f)
            return;

        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        body.linearVelocity = pushDirection * _pushForce;
    }

    // Sent by Dialogue System for Unity to conversation participants
    void OnConversationStart(Transform actor)
    {
        _isTalking = true;
        _isSprinting = false;
        StopHandShake();

        if (_sprintShake != null)
            _sprintShake.AmplitudeGain = 0f;
    }

    void OnConversationEnd(Transform actor)
    {
        _isTalking = false;
    }

}
