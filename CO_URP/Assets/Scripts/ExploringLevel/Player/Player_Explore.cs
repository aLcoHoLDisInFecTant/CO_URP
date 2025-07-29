using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class Player_Explore : MonoBehaviour, IControllable, IResettable, ICommandTranslator
{
    public PlayerStateMachine_Explore StateMachine { get; private set; }
    public Animator Animator { get; private set; }
    //public CharacterController CharacterController { get; private set; }

    public Rigidbody Rb { get; private set; }

    [SerializeField] public MovementPreviewController previewController; // 箭头控制器

    [SerializeField] public GrappleLauncher grappleLauncher;

    [SerializeField] public BoomerangLauncher boomerangLauncher;

    [SerializeField] private PlayerData_Explore playerData;

    [Header("摄像机控制")]
    public Transform cameraTransform;

    private bool isControllable = true;

    public PlayerData_Explore Data => playerData;
    // 输入队列
    public Queue<ECommand> inputQueue = new Queue<ECommand>();

    private int inputQueueLimit = 3; // 最多保留 3 个方向指令

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("grapple")]
    public Vector3 GrapplePoint;
    public float SwingForce = 3f;
    public Transform child;

    [Header("Boomerang")]
    public bool isBoomerangCharging = false;
    public bool isBoomerangFlying = false;
    public GameObject boomerangModel;

    void Awake()
    {
        child = transform.Find("lasso");
        if (child != null) child.gameObject.SetActive(false);
        //CharacterController = GetComponent<CharacterController>();
        Rb = GetComponent<Rigidbody>();
        Rb.freezeRotation = true;
        Rb.constraints = RigidbodyConstraints.FreezeRotation;
        StateMachine = new PlayerStateMachine_Explore(this);
        Animator = GetComponent<Animator>();
        boomerangModel.gameObject.SetActive(false);
        // Initialize boomerang launcher
        boomerangLauncher = GetComponent<BoomerangLauncher>();
        if (boomerangLauncher == null)
        {
            boomerangLauncher = gameObject.AddComponent<BoomerangLauncher>();
        }

        // Set boomerang prefab reference
        /*
        if (boomerangPrefab != null)
        {
            boomerangLauncher.boomerangPrefab = boomerangPrefab;
        }
        */
    }

    void Start()
    {

        StateMachine.SetState(StateMachine.PreviewState);
        GameSessionExplore.Instance.AddCommandTranslator(this);
        //Animator = new PlayerAnimator(GetComponent<Animator>());
        
    }

    void Update()
    {
        //Debug.Log("playercontrol" + isControllable);
        //Debug.Log("currentState" + StateMachine.CurrentState);

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (!isControllable && StateMachine.CurrentState != StateMachine.RiddleState)
        {
            StateMachine.SetState(StateMachine.RiddleState);
        }
        if (!isBoomerangCharging && !isBoomerangFlying &&
            (inputQueue.Contains(ECommand.SLIDERIGHT) || inputQueue.Contains(ECommand.SLIDELEFT)))
        {
            StateMachine.SetState(StateMachine.BoomerangState);
            isBoomerangCharging = true;
        }
        StateMachine.Tick();
    }

    void FixedUpdate()
    {
        StateMachine.FixedTick();
    }

    public void OnBoomerangReturned()
    {
        isBoomerangFlying = false;
        isBoomerangCharging = false;
        Debug.Log("Boomerang returned to player");
    }


    public void SetControllable(bool controllable)
    {
        isControllable = controllable;

        if (controllable)
        {
            StateMachine.SetState(StateMachine.PreviewState);
            //GameSessionExplore.Instance.AddCommandTranslator(this);
            Debug.Log("接入");
        }
        else
        {
            StateMachine.SetState(StateMachine.RiddleState);
            //GameSessionExplore.Instance.RestrictInputs(InputConstants.InGameCommands, true);
        }
    }


    public ICommandTranslator GetCommandTranslator()
    {
        return this;
    }

    public void ResetToDefault()
    {
        StateMachine.SetState(null);
        //PlayerStatictics.ResetToDefault();

        // Reset boomerang state
        isBoomerangCharging = false;
        isBoomerangFlying = false;
        if (boomerangLauncher != null)
        {
            boomerangLauncher.ForceStopBoomerang();
        }

        Physics.SyncTransforms();
    }

    public void TranslateCommand(ECommand command, PressedState state) 
    {
        if (command is ECommand.LEFT or ECommand.RIGHT or ECommand.UP or ECommand.DOWN or ECommand.SLIDELEFT or ECommand.SLIDERIGHT)
        {
            if (state.IsPressed)
            {
                // 避免重复加入
                if (!inputQueue.Contains(command))
                {
                    if (inputQueue.Count >= inputQueueLimit)
                        inputQueue.Dequeue(); // 移除最旧输入
                    inputQueue.Enqueue(command);
                }
            }

            if (state.IsReleased)
            {
                // 松开后从队列移除
                var list = inputQueue.ToList();
                list.Remove(command);
                inputQueue = new Queue<ECommand>(list);
            }
        }

        foreach (ECommand input in inputQueue) {
            Debug.Log("inputQueue build" + input);
        }
        
    }
    public void OnReturnedFromPuzzle()
    {
        isControllable = true;
        StateMachine.SetState(StateMachine.PreviewState);
    }

}

public interface IControllable
{
    void SetControllable(bool controllable);

    ICommandTranslator GetCommandTranslator();
}