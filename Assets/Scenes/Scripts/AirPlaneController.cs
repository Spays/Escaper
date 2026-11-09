using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class AirplaneController : MonoBehaviour
{
    [Header("Скорость и Тяга")]
    [Tooltip("Постоянная скорость движения вперёд")]
    public float forwardSpeed = 0f;

    [Tooltip("Минимальная скорость при отпущенном курке")]
    public float minSpeed = 0f;

    [Tooltip("Максимальная скорость при полностью нажатом курке")]
    public float maxSpeed = 100f;
    
	//public float maxSpeed = 1f;
    [Tooltip("Насколько быстро самолёт набирает скорость (инерция разгона)")]
    public float acceleration = 5f;

    [Header("Управление (Вращение)")]
    [Tooltip("Скорость наклона носа вверх/вниз (Pitch)")]
    public float pitchSpeed = 60f;
    [Tooltip("Скорость вращения вокруг своей оси (Roll)")]
    public float rollSpeed = 80f;
    [Tooltip("Скорость поворота влево/вправо (Yaw)")]
    public float yawSpeed = 30f;
    [Header("Связь рыскания и крена")]
    
    [Tooltip("Автоматический наклон при рыскании")]
    public float yawRollInfluence = 0.3f;

    [Header("Управление вращением после столкновений")]
    [Tooltip("Скорость изменения вращения при управлении")]
    public float angularControlAcceleration = 8f;

    [Tooltip("Скорость торможения вращения без ввода")]
    public float angularBraking = 2f;
    
    [Header("Плавность вращения (Инерция)")]
    public float rotationSmoothness = 5f;

	[Header("Зависимость инерции от скорости")]
	[Tooltip("Сила подавления скольжения на единицу скорости")]
	public float perpendicularDragCoefficient = 0.3f;

	[Tooltip("Максимальная сила подавления скольжения")]
	public float maxPerpendicularDrag = 5f;

	[Header("Разворот на 180")]
	public float turnAroundSpeed = 180f;
	private bool isTurningAround;
	private Quaternion turnAroundTarget;

    private Rigidbody rb;
    private float currentSpeed;
    
    // Переменные для плавного ввода (математическая инерция руля)
    private float targetPitch, targetRoll, targetYaw;
    private float currentPitch, currentRoll, currentYaw;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Отключаем гравитацию для аркадной модели полёта
        rb.useGravity = false; 
    }

    void Update()
    {
	    float keyboardPitch = 0f;
	    float keyboardRoll = 0f;
	    float keyboardYaw = 0f;
	    float keyboardSpeed = 0f;

// Клавиатура
	    if (Keyboard.current != null)
	    {
		    if (Keyboard.current.sKey.isPressed) keyboardPitch += 1f;
		    if (Keyboard.current.wKey.isPressed) keyboardPitch -= 1f;

		    if (Keyboard.current.aKey.isPressed) keyboardRoll += 1f;
		    if (Keyboard.current.dKey.isPressed) keyboardRoll -= 1f;

		    if (Keyboard.current.qKey.isPressed) keyboardYaw -= 1f;
		    if (Keyboard.current.eKey.isPressed) keyboardYaw += 1f;

		    if (Keyboard.current.spaceKey.isPressed) keyboardSpeed += 1f;
		    if (Keyboard.current.ctrlKey.isPressed) keyboardSpeed -= 1f;
	    }
	    
	    // Значения управления с геймпада
	    float gamepadPitch = 0f;
	    float gamepadRoll = 0f;
	    float gamepadYaw = 0f;
	    float gamepadSpeed = 0f;
	    
        // 1. Получение ввода от игрока
        //if (forwardSpeed > 0) { forwardSpeed = forwardSpeed - Gamepad.current.leftTrigger.ReadValue() * acceleration; }
        if (Gamepad.current != null)
        {
	        Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
	        Vector2 rightStick = Gamepad.current.rightStick.ReadValue();

	        gamepadPitch = leftStick.y;
	        gamepadRoll = -leftStick.x;
	        gamepadYaw = rightStick.x;

	        float increaseInput = Gamepad.current.rightTrigger.ReadValue();
	        float decreaseInput = Gamepad.current.leftTrigger.ReadValue();

	        gamepadSpeed = increaseInput - decreaseInput;
	        forwardSpeed = Mathf.Lerp(minSpeed, maxSpeed, gamepadSpeed);
        }
        // Объединяем управление
        float pitchInput = Mathf.Clamp(keyboardPitch + gamepadPitch, -1f, 1f);
        float rollInput = Mathf.Clamp(keyboardRoll + gamepadRoll, -1f, 1f);
        float yawInput = Mathf.Clamp(keyboardYaw + gamepadYaw, -1f, 1f);
        float speedInput = Mathf.Clamp(keyboardSpeed + gamepadSpeed, -1f, 1f);

// Передаём управление самолёту
        targetPitch = pitchInput * pitchSpeed;

        targetYaw = yawInput * yawSpeed;

        float stickRoll = rollInput * rollSpeed;
        targetRoll = stickRoll - targetYaw * yawRollInfluence;

// Изменение заданной скорости
	    forwardSpeed = Mathf.Lerp(minSpeed, maxSpeed, speedInput);
	    
		if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || (Keyboard.current.rKey.isPressed)) && !isTurningAround)
		{
    		isTurningAround = true;
		    turnAroundTarget = rb.rotation * Quaternion.Euler(0f, 180f, 0f);
		}
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
		ApplyAerodynamicStability();
		HandleTurnAround();
    }

	void ApplyAerodynamicStability()
	{
		// Переводим мировую скорость в локальные оси корабля
		Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

		// Скорость именно вдоль носа
		float forwardVelocity = Mathf.Max(0f, localVelocity.z);

		// Прямая зависимость от продольной скорости
		float perpendicularDrag = Mathf.Clamp(forwardVelocity * perpendicularDragCoefficient, 0f, maxPerpendicularDrag);

		// Подавляем только движение поперёк корпуса
		Vector3 localCorrection = new Vector3( -localVelocity.x, -localVelocity.y, 0f);

		// Возвращаем корректирующий вектор в мировые координаты
		Vector3 worldCorrection =transform.TransformDirection(localCorrection);

		rb.AddForce( worldCorrection * perpendicularDrag, ForceMode.Acceleration );
	}
    void HandleMovement()
    {
        // Плавный разгон до целевой скорости (инерция хода)
        //currentSpeed = Mathf.Lerp(currentSpeed, forwardSpeed, Time.fixedDeltaTime * acceleration);

        // Движение вперёд через Rigidbody (сохраняет физические столкновения)
        //rb.velocity = transform.forward * currentSpeed;
        
        // Инерционный разгон до целевой скорости
        currentSpeed = Mathf.Lerp(
            currentSpeed,
            forwardSpeed,
            Time.fixedDeltaTime * acceleration
        );

        // Желаемая скорость вдоль носа самолёта
        Vector3 targetVelocity = transform.forward * currentSpeed;

        // Реальная скорость тела плавно догоняет желаемую
        rb.velocity = Vector3.MoveTowards(
            rb.velocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );
    }

    void HandleRotation()
    {
        // Плавная интерполяция ввода для симуляции тяжести управления
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.fixedDeltaTime * rotationSmoothness);
        currentRoll = Mathf.Lerp(currentRoll, targetRoll, Time.fixedDeltaTime * rotationSmoothness);
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.fixedDeltaTime * rotationSmoothness);

        /*
        // Переводим углы в локальные оси самолёта
        Vector3 rotationVector = new Vector3(currentPitch, currentYaw, currentRoll);
        
        // Применяем вращение через Rigidbody, чтобы не ломать физику движка
        Quaternion deltaRotation = Quaternion.Euler(rotationVector * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);
        */
        
        // Целевая угловая скорость в локальных осях корабля.
        // Unity хранит angularVelocity в радианах в секунду.
        Vector3 localAngularVelocity = new Vector3(
            currentPitch,
            currentYaw,
            currentRoll
        ) * Mathf.Deg2Rad;

        // Переводим её в мировые координаты
        Vector3 targetAngularVelocity =
            rb.rotation * localAngularVelocity;

        // Если управления нет — плавно гасим вращение.
        // Если управление есть — стремимся к скорости,
        // заданной стиками.
        bool hasInput =
            Mathf.Abs(targetPitch) > 0.1f ||
            Mathf.Abs(targetRoll) > 0.1f ||
            Mathf.Abs(targetYaw) > 0.1f;

        float rate = hasInput
            ? angularControlAcceleration
            : angularBraking;

        rb.angularVelocity = Vector3.MoveTowards(
            rb.angularVelocity,
            targetAngularVelocity,
            rate * Time.fixedDeltaTime
        );
    }
	void HandleTurnAround()
	{
    	if (!isTurningAround)
	        return;

	    rb.MoveRotation(Quaternion.RotateTowards(
	        rb.rotation,
	        turnAroundTarget,
	        turnAroundSpeed * Time.fixedDeltaTime
	    ));

    	if (Quaternion.Angle(rb.rotation, turnAroundTarget) < 0.1f)
    	{
        	rb.MoveRotation(turnAroundTarget);
        	isTurningAround = false;
	    }
	}
}
