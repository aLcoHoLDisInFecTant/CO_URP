using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionExplore : MonoBehaviour, IResettable
{
    [SerializeField] private Player_Explore currentPlayer;
    //[SerializeField] private Scoreboard scoreboard;
    [SerializeField] private EncoderInputBridgeV2 encoderInputBridge;
    public static GameSessionExplore Instance { get; private set; }
    private IInputTranslator inputTranslator;

    private bool isSessionPaused = false;
    private bool isInputAlreadyRestricted = false;

    private void Awake()
    {
        Instance = this;
        Init();
    }

    private void Start()
    {
        if (encoderInputBridge) 
        {
            encoderInputBridge.SetSessionZeroPoint();
        }
        
    }
    private void Update()
    {
        inputTranslator.Tick();
    }

    private void Init()
    {
        if (encoderInputBridge)
        {
            IBindingHolder<EncodeBinding> encodeHolder = new EncodeBindingHolder(encoderInputBridge);
            inputTranslator = new InputTranslator<EncodeBinding>(encodeHolder);
            Debug.Log("encodeBinding");
        }
        else 
        {
            IBindingHolder<KeyBinding> keyHolder = new KeyBindingHolder();
            inputTranslator = new InputTranslator<KeyBinding>(keyHolder);
            Debug.Log("keyBinding");
        }
        

    }

    public void AddCommandTranslator(ICommandTranslator translator)
    {
        inputTranslator.AddCommandTranslator(translator);
    }


    public void PauseSession(bool isPaused)
    {
        Time.timeScale = isPaused ? 0 : 1;
        if (!isSessionPaused && inputTranslator.IsTranslationResticted(InputConstants.InGameCommands))
        {
            isInputAlreadyRestricted = true;
            isSessionPaused = isPaused;
            return;
        }
        if (!inputTranslator.IsTranslationResticted(InputConstants.InGameCommands))
        {
            isInputAlreadyRestricted = false;
        }
        isSessionPaused = isPaused;
        if (isInputAlreadyRestricted)
        {
            return;
        }
        RestrictInputs(InputConstants.InGameCommands, isRestricted: isPaused);
    }

    public void RestrictInputs(List<ECommand> commands, bool isRestricted)
    {
        inputTranslator.RestictTranslation(commands, isRestricted);
    }

    /*
    public void UpdateScoreboard(ScoreboardEntry entry)
    {
        scoreboard.AddScoreboardEntry(entry);
    }
    */
    public void RestartSession()
    {
        SceneManager.LoadScene("RunningScene", LoadSceneMode.Single);
        ResetToDefault();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        ResetToDefault();
    }

    public void ResetToDefault()
    {
        PauseSession(false);
        currentPlayer.ResetToDefault();
    }

    public void RemoveCommandTranslator(ICommandTranslator translator)
    {
        inputTranslator.RemoveCommandTranslator(translator);
    }

}
