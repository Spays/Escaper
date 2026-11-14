
using UnityEngine;

public class ShipEngineAudio : MonoBehaviour
{
    [Header("Корабль")]
    public AirplaneController ship;
    public float maxSpeed = 400f;

    [Header("Источники звука")]
    public AudioSource core;
    public AudioSource energy;
    public AudioSource hull;
    public AudioSource dust;

    [Header("Громкость")]
    [Range(0f, 1f)] public float coreVolume = 0.8f;
    [Range(0f, 1f)] public float energyVolume = 0.5f;
    [Range(0f, 1f)] public float hullVolume = 0.3f;

    [Header("Высота тона")]
    public float coreMinPitch = 0.7f;
    public float coreMaxPitch = 1.5f;
    public float energyMinPitch = 0.8f;
    public float energyMaxPitch = 1.8f;

    [Header("Пыль")]
    public AudioClip[] dustClips;
    [Range(0f, 1f)] public float dustDensity = 0.5f;
    public float dustInterval = 0.8f;

    [Header("Плавность")]
    public float smooth = 3f;

    private float dustTimer;

    void Start()
    {
        maxSpeed = ship.maxSpeed;
        StartLoop(core);
        StartLoop(energy);
        StartLoop(hull);
    }

    void StartLoop(AudioSource source)
    {
        if (source == null) return;

        source.loop = true;
        source.volume = 0f;
        source.Play();
    }

    void Update()
    {
        if (ship == null) return;

        float speed = Mathf.Clamp01(ship.forwardSpeed / maxSpeed);

        // Основной двигатель
        if (core != null)
        {
            core.volume = Mathf.Lerp(
                core.volume,
                speed * coreVolume,
                Time.deltaTime * smooth
            );

            core.pitch = Mathf.Lerp(
                core.pitch,
                Mathf.Lerp(coreMinPitch, coreMaxPitch, speed),
                Time.deltaTime * smooth
            );
        }

        // Энергетический звук
        if (energy != null)
        {
            energy.volume = Mathf.Lerp(
                energy.volume,
                speed * energyVolume,
                Time.deltaTime * smooth
            );

            energy.pitch = Mathf.Lerp(
                energy.pitch,
                Mathf.Lerp(energyMinPitch, energyMaxPitch, speed),
                Time.deltaTime * smooth
            );
        }

        // Вибрация корпуса зависит от скорости
        if (hull != null)
        {
            hull.volume = Mathf.Lerp(
                hull.volume,
                speed * hullVolume,
                Time.deltaTime * smooth
            );
        }

        // Случайные удары пыли
        UpdateDust(speed);
    }

    void UpdateDust(float speed)
    {
        if (dust == null || dustClips.Length == 0) return;

        if (speed < 0.1f || dustDensity <= 0f) return;

        dustTimer -= Time.deltaTime;

        if (dustTimer <= 0f)
        {
            AudioClip clip = dustClips[
                Random.Range(0, dustClips.Length)
            ];

            dust.pitch = Random.Range(0.8f, 1.3f);

            dust.PlayOneShot(
                clip,
                Random.Range(0.2f, 0.7f) * dustDensity * speed
            );

            dustTimer = dustInterval / Mathf.Max(speed * dustDensity, 0.1f);
        }
    }
}
