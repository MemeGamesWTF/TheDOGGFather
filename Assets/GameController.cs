using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class GameController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public GameObject[] pointObjectPrefabs;  // Array of point prefabs
    public GameObject[] loseObjectPrefabs;   // Array of lose prefabs
    public GameObject hitEffectPrefab; 
    public float spawnInterval = 1f;
    public float fallSpeed = 2f; // Adjustable speed for falling objects

    [Header("Score Settings")]
    public TextMeshProUGUI scoreText;
    private int score;

    [Header("Timer Settings")]
    public Slider timeSlider;
    public float gameDuration = 30f; // Total time in seconds
    private float remainingTime;

    [Header("UI Settings")]
    public GameObject gameOverUI;
    public GameObject startPanel;
    public Button startButton;
    public Button restartButton;

    private bool isGameOver;
    private bool isGameStarted;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] hitClips; // Array of hit audio clips
    public AudioClip hitout;


    [DllImport("__Internal")]
  private static extern void SendScore(int score, int game);

    void Start()
    {
        // Initial setup
        Time.timeScale = 0; // Pause the game until the player starts
        isGameOver = false;
        isGameStarted = false;
        score = 0;
        remainingTime = gameDuration;

        // UI setup
        timeSlider.maxValue = gameDuration;
        timeSlider.value = gameDuration;
        UpdateScoreUI();

        // Button listeners
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);
    }

    void Update()
    {
        if (!isGameStarted || isGameOver) return;

        // Reduce timer gradually
        remainingTime -= Time.deltaTime;
        timeSlider.value = remainingTime;

        // Check if the timer has run out
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            GameOver();
        }

        // Handle mouse click
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void SpawnObject()
    {
        if (!isGameStarted || isGameOver) return;

        // Track available spawn points
        System.Collections.Generic.List<Transform> availableSpawnPoints = new System.Collections.Generic.List<Transform>(spawnPoints);

        // Ensure that each object spawns in a unique spot
        foreach (Transform spawnPoint in spawnPoints)
        {
            // Check if any object is already spawned here (a temporary solution based on the spawn logic)
            RaycastHit2D hit = Physics2D.Raycast(spawnPoint.position, Vector2.zero, 0.1f);
            if (hit.collider != null)
            {
                availableSpawnPoints.Remove(spawnPoint); // Remove from available spots if occupied
            }
        }

        // If no available spawn points, exit
        if (availableSpawnPoints.Count == 0) return;

        // Randomly choose an available spawn point
        Transform chosenSpawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];

        // Adjust the probability of spawning point vs. negative point
        GameObject prefabToSpawn;
        if (Random.value < 0.7f) // 70% chance for point prefab
        {
            prefabToSpawn = pointObjectPrefabs[Random.Range(0, pointObjectPrefabs.Length)];
        }
        else // 30% chance for negative point prefab
        {
            prefabToSpawn = loseObjectPrefabs[Random.Range(0, loseObjectPrefabs.Length)];
        }

        // Instantiate the selected prefab at the spawn point
        GameObject spawnedObject = Instantiate(prefabToSpawn, chosenSpawnPoint.position, Quaternion.identity);

        // Ensure the object has the correct tag
        if (System.Array.IndexOf(pointObjectPrefabs, prefabToSpawn) >= 0 && System.Array.IndexOf(pointObjectPrefabs, prefabToSpawn) <= 6)
        {
            spawnedObject.tag = "Point";
        }
        else
        {
            spawnedObject.tag = "NegPoint";
        }

        // Add a FallingObject script dynamically and assign fall speed
        FallingObject fallingObject = spawnedObject.AddComponent<FallingObject>();
        fallingObject.fallSpeed = fallSpeed;

        // Destroy the object after 6 seconds
        Destroy(spawnedObject, 6f);
    }


    void HandleMouseClick()
{
    // Cast a ray from the camera to the mouse position
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

    if (hit.collider != null)
    {
        GameObject clickedObject = hit.collider.gameObject;

        // Check the tag and decide whether to add points or end the game
        if (clickedObject.CompareTag("Point"))
        {
            AddPoint();
            Destroy(clickedObject);

            // Spawn a new prefab for 1 second
            SpawnHitEffect(hit.point);
        }
        else if (clickedObject.CompareTag("NegPoint"))
        {
            Destroy(clickedObject);
            ReduceTimerToZero();
        }
    }
}

void SpawnHitEffect(Vector3 spawnPosition)
{
    // Instantiate the hit effect prefab at the clicked position
    GameObject effect = Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);

    // Play a random hit sound effect
    if (hitClips.Length > 0)
    {
        AudioClip randomClip = hitClips[Random.Range(0, hitClips.Length)];
        audioSource.PlayOneShot(randomClip);
    }

    // Destroy the effect after 0.2 seconds
    Destroy(effect, 0.2f);
}


    void ReduceTimerToZero()
    {
        remainingTime = 0;
        timeSlider.value = 0;
        GameOver();
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    public void AddPoint()
    {
        if (isGameOver) return;

        score++;
        UpdateScoreUI();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        audioSource.PlayOneShot(hitout);
        isGameOver = true;
        gameOverUI.SetActive(true);
        SendScore(score, 42);
        Time.timeScale = 0; // Pause the game
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        Time.timeScale = 1; // Start the game
        isGameStarted = true;
        InvokeRepeating(nameof(SpawnObject), 0f, spawnInterval);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload the current scene
    }
}
