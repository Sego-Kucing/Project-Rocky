using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class RockController : MonoBehaviour
{
    public enum RockState { Aiming, Charging, InAir, Stopped, Sinking }
    [Header("State")]
    public RockState currentState = RockState.Aiming;

    [Header("Aiming & Launch Settings")]
    [Tooltip("Kecepatan Rotasi saat mengarahkan batu kiri / kanan")]
    public float aimSpeed = 50f;
    [Tooltip("Batas Rotasi Angle")]
    public float maxAimAngle = 45f;

    [Tooltip("Tenaga minimal agar lemparan tidak terlalu pelan")]
    public float minLaunchPower = 20f;
    public float maxLaunchPower = 80f;
    [Tooltip("Seberapa cepat charge bar nya")]
    public float chargeSpeed = 1.5f;

    [Header("In-Air Settings")]
    public float inAirControlForce = 5f;
    public int maxMidAirBounces = 3;
    public float midAirBounceForce = 10f;

    [Header("Water Bounce Settings")]
    [Tooltip("Kecepatan Minimal Maju (X/Z) agar mantul di air")]
    public float minSpeedToSkip = 5f;
    [Tooltip("Seberapa banyak momentum yang hilang setiap nabrak air. (Ranga 0.1 - 1.0)")]
    public float velocityDampenPerBounce = 0.85f;
    [Tooltip("Kekuatan pantulan ke atas saat mengenai air")]
    public float waterBounceUpwardForce = 6f;

    [Header("Game Over Settings")]
    public string gameOverSceneName = "GameOver";
    [Tooltip("Waktu delay dari batu untuk pindah ke scene game overnya")]
    public float timeToFadeOut = 2f;

    [Tooltip("Masukkan object si speedline")]
    public GameObject speedLineEffect;

    // Initial Setup
    private Rigidbody rb;
    private Collider rockCollider;
    private float currentAimAngle = 0f;
    private float chargeTimer = 0f;
    private float currentPower = 0f;
    private int currentBouncesLeft;
    private bool isGameOverTriggered = false;

    // Cache Input untuk FixedUpdate
    private Vector2 inAirInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rockCollider = GetComponent<Collider>();
        currentBouncesLeft = maxMidAirBounces;

        rb.isKinematic = true;

        if (speedLineEffect != null)
        {
            speedLineEffect.SetActive(false);
        }
    }

    void Update()
    {
        HandleInput();
        CheckSinking();
    }

    void FixedUpdate()
    {
        if (currentState == RockState.InAir)
        {
            ApplyInAirMovement();
        }
    }

    // Private Method
    private void HandleInput()
    {
        switch(currentState)
        {
            case RockState.Aiming:
                float aimInput = Input.GetAxisRaw("Horizontal");
                currentAimAngle += aimInput * aimSpeed * Time.deltaTime;
                currentAimAngle = Mathf.Clamp(currentAimAngle, -maxAimAngle, maxAimAngle);

                // Rotasi Secara Visual dan Arah Launch
                transform.rotation = Quaternion.Euler(0, currentAimAngle, 0);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    currentState = RockState.Charging;
                    chargeTimer = 0f;
                }
                break;
            
            case RockState.Charging:
                chargeTimer += Time.deltaTime * chargeSpeed;
                // Hasil presentase
                float powerPercent = Mathf.PingPong(chargeTimer, 1f);
                currentPower = Mathf.Lerp(minLaunchPower, maxLaunchPower, powerPercent);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    LaunchRock();
                }
                break;
            
            case RockState.InAir:
                inAirInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                // Mid Air Logic Bounce
                if (Input.GetKeyDown(KeyCode.Space) && currentBouncesLeft > 0)
                {
                    ExecuteMidAirBounce();
                }
                break;
        }
    }

    private void LaunchRock()
    {
        currentState = RockState.InAir;

        rb.isKinematic = false;

        if (speedLineEffect != null)
        {
            speedLineEffect.SetActive(true);
        }

        Vector3 launchDirection = transform.forward + (Vector3.up * 0.2f);

        rb.AddForce(launchDirection.normalized * currentPower, ForceMode.VelocityChange);
    }

    private void ApplyInAirMovement()
    {
        Vector3 moveForce = new Vector3(inAirInput.x, 0, inAirInput.y) * inAirControlForce;
        rb.AddForce(moveForce, ForceMode.Acceleration);
    }

    private void ExecuteMidAirBounce()
    {
        currentBouncesLeft--;

        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = 0;
        rb.linearVelocity = currentVelocity;

        rb.AddForce(Vector3.up * midAirBounceForce, ForceMode.Impulse);
    }

    // Logic Air dan GameOver
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water") && currentState == RockState.InAir)
        {
            HandleWaterBounce();
        }
    }

    private void HandleWaterBounce()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude >= minSpeedToSkip)
        {
            // Kurangin speed ketika kena air
            Vector3 newVelocity = horizontalVelocity * velocityDampenPerBounce;

            // Dorongan
            newVelocity.y = waterBounceUpwardForce;

            // Terapkan Kecepatan Baru
            rb.linearVelocity = newVelocity;
        } 
        else
        {
            currentState = RockState.Sinking;
            rockCollider.isTrigger = true;
            rb.linearVelocity = rb.linearVelocity * 0.3f;

            if (speedLineEffect != null)
            {
                speedLineEffect.SetActive(false);
            }
        }
    }

    private void CheckSinking()
    {
        if (currentState == RockState.Sinking && !isGameOverTriggered)
        {
            isGameOverTriggered = true;
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.FadeAndLoadScene(gameOverSceneName, timeToFadeOut);
            }
            else
            {
                Invoke(nameof(LoadGameOver), timeToFadeOut);
            }
        }
    }

    private void LoadGameOver()
    {
        SceneManager.LoadScene(gameOverSceneName);
    }
}
