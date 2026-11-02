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
        // 1. Получение ввода от игрока
        //if (forwardSpeed > 0) { forwardSpeed = forwardSpeed - Gamepad.current.leftTrigger.ReadValue() * acceleration; }
        if (Gamepad.current != null)
        {
            // Левый стик: наклон носа вверх/вниз
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            targetPitch = leftStick.y * pitchSpeed;

            // Левый стик: крен
            float stickRoll = -leftStick.x * rollSpeed;

            // Правый стик: рыскание
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            targetYaw = rightStick.x * yawSpeed;

            // Смешиваем ручной крен и автоматический наклон от рыскания
            targetRoll = stickRoll - targetYaw * yawRollInfluence;
            
            // R2: управление тягой
            //forwardSpeed = maxSpeed * (Gamepad.current.rightTrigger.ReadValue() - Gamepad.current.leftTrigger.ReadValue());
            float trigger = Gamepad.current.rightTrigger.ReadValue() - Gamepad.current.leftTrigger.ReadValue();
            forwardSpeed = Mathf.Lerp(minSpeed, maxSpeed, trigger);
        }
        else
        {
            // При отключении геймпада плавно возвращаем управление в нейтраль
            targetPitch = 0f;
            targetRoll = 0f;
            targetYaw = 0f;
        }
		if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame && !isTurningAround)
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
