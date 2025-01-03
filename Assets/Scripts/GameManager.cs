using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject instructionsCanvas;
    [SerializeField] TMP_Text timerText;
    [SerializeField] GameObject BGM;
    [SerializeField] Transform timerPosAfterWin;
    [SerializeField] GameObject gameWinMenu;
    [SerializeField] GameObject hudCanvas;

    public bool gameIsOn = false;
    private GameObject escapee;
    private Animator escapeeAnimator;
    private TMP_Text noteCounter;

    public static GameManager Instance { get; private set; }  // Singleton instance

    private float startTimer = 0;
    private bool instructionsSeen = false;
    private bool gameHasStarted = false;
    private float gameTimer;
    private int noteCount;

    void Awake()
    {
        // Singleton pattern to make sure only one GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Listen to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Unsubscribe from the sceneLoaded event when the object is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(BGM);
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGame();
        Scene activeScene = SceneManager.GetActiveScene();

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            instructionsCanvas = GameObject.FindWithTag("InstructionsCanvas");
            if (!instructionsSeen && instructionsCanvas != null)
            {
                instructionsCanvas.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            if (Input.anyKeyDown && !gameIsOn && !gameHasStarted)
            {
                gameHasStarted = true;
                if (instructionsCanvas)
                {
                    instructionsCanvas.SetActive(false);
                }
                instructionsSeen = true;
                if (escapee)
                {
                    escapee.GetComponent<Escapee>().ChangeAnimation("EscStart", 0);
                }
                GetComponent<AudioSource>().Play();
                StartCoroutine(StartGame());
            }

            if (gameIsOn && timerText != null)
            {
                gameTimer += Time.deltaTime;
                timerText.text = gameTimer.ToString("F2") + "s";
            }

            if (gameIsOn && hudCanvas != null && hudCanvas.transform.Find("NoteCounter"))
            {
                noteCounter = hudCanvas.transform.Find("NoteCounter").transform.GetChild(1).GetComponent<TMP_Text>();
                noteCounter.text = ($"x {noteCount}");
            }
        }
    }

    IEnumerator StartGame()
    {
        while (startTimer < 1.3f)
        {
            startTimer += Time.deltaTime;
            yield return null;
        }

        if (!hudCanvas)
        {
            hudCanvas = GameObject.FindGameObjectWithTag("HUDCanvas");
        }

        gameIsOn = true;
        SetCrowdNoises();
    }

    public void SetCrowdNoises()
    {
        if (BGM)
        {
            BGM.GetComponent<BackgroundMusicShuffle>().UpdateCrowdNoises();
        }
    }

    public void HandleGameWin()
    {
        if (gameIsOn)
        {
            gameIsOn = false;
        }

        if (timerText != null)
        {
            // Scale up the timer text (you already did this)
            timerText.transform.localScale *= 2;

            // Get the RectTransform component of the timerText
            RectTransform timerRect = timerText.GetComponent<RectTransform>();

            Vector3 localPos = Vector3.zero;

            if (timerPosAfterWin)
            {
                // Convert the world position of timerPosAfterWin to local position relative to the canvas
                localPos = timerText.transform.parent.InverseTransformPoint(timerPosAfterWin.position);
            }
            else
            {
                if (hudCanvas)
                {
                    timerPosAfterWin = hudCanvas.transform.Find("WinPositionForTimer");
                }
                else
                {
                    timerPosAfterWin = GameObject.FindGameObjectWithTag("HUDCanvas")?.transform.Find("WinPositionForTimer");
                }

                if (timerPosAfterWin == null)
                {
                    print("timerPosAfterWin is null, please check the name or ensure the object is active in the hierarchy");
                }
                else
                {
                    localPos = timerText.transform.parent.InverseTransformPoint(timerPosAfterWin.position);
                }
            }

            if (localPos != null)
            {
                // Set the local position of the timer text based on the calculated local position
                timerRect.anchoredPosition = new Vector2(localPos.x, localPos.y);
            }

            // Optional: Adjust anchors and pivots to ensure it stays aligned properly
            timerRect.anchorMin = new Vector2(0.5f, 0.5f);  // Centered anchor (you can customize this)
            timerRect.anchorMax = new Vector2(0.5f, 0.5f);  // Same as anchorMin for centering
            timerRect.pivot = new Vector2(0.5f, 0.5f);
        }

        if (gameWinMenu != null)
        {
            gameWinMenu.SetActive(true);
        }
    }


    public void PlayAgain()
    {
        if (!BGM)
        {
            BGM = GameObject.FindGameObjectWithTag("BGM");
        }
        BGM.GetComponent<BackgroundMusicShuffle>().UpdateCrowdNoises();
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void ResetGame()
    {
        gameIsOn = false;
        gameHasStarted = false;
        gameTimer = 0;
        startTimer = 0;
        instructionsSeen = false;
        noteCount = 0;

        if (instructionsCanvas)
        {
            instructionsCanvas.SetActive(true); // To show instructions when the scene is reloaded
        }
        else if (!instructionsCanvas && SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            instructionsCanvas = GameObject.FindGameObjectWithTag("InstructionsCanvas");
            instructionsCanvas.SetActive(true); // To show instructions when the scene is reloaded
        }

        if (gameWinMenu)
        {
            gameWinMenu.SetActive(false);
        }
        else if (!gameWinMenu && SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            if (hudCanvas)
            {
                gameWinMenu = hudCanvas.transform.Find("GameWinMenu").gameObject;
            }

            else
            {
                gameWinMenu = GameObject.FindGameObjectWithTag("HUDCanvas").transform.Find("GameWinMenu").gameObject;
            }
                
            gameWinMenu.SetActive(false);
        }
        if (timerText)
        {
            timerText.transform.localScale = Vector3.one; // Reset the timer scale
            timerText.text = "0.00"; // Reset the timer display
        }
        else if (!timerText && SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            if (hudCanvas)
            {
                timerText = hudCanvas.transform.Find("Timer").gameObject.GetComponent<TMP_Text>();
            }

            else
            {
                timerText = GameObject.FindGameObjectWithTag("HUDCanvas").transform.Find("Timer").gameObject.GetComponent<TMP_Text>();
            }

            timerText.transform.localScale = Vector3.one; // Reset the timer scale
            timerText.text = "0.00"; // Reset the timer display
        }

        if (escapee)
        {
            escapee.GetComponent<Escapee>().ChangeAnimation("Idle", 0); // Reset the escapee animation
        }
        else if (!escapee && SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            escapee = FindObjectOfType<Escapee>().gameObject;
        }
    }

    public void AddNote()
    {
        noteCount++;
    }

    public void LoseNotes()
    {
        noteCount = 0;
    }

}
