using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
/*
[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(Animator))]
public class Player_Explore : MonoBehaviour, IControllable, IResettable, ICommandTranslator
{
    public PlayerStateMachine_Explore StateMachine { get; private set; }
    public PlayerAnimator_Explore Animator { get; private set; }
    public CharacterController CharacterController { get; private set; }

    [SerializeField] public MovementPreviewController previewController; // 箭头控制器

    [SerializeField] private PlayerData_Explore playerData;

    private bool isControllable = true;

    public PlayerData_Explore Data => playerData;

    // 输入队列
    public Queue<ECommand> inputQueue = new Queue<ECommand>();

    private int inputQueueLimit = 3; // 最多保留 3 个方向指令


    void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
        StateMachine = new PlayerStateMachine_Explore(this);
    }

    void Start()
    {
        
        StateMachine.SetState(StateMachine.PreviewState);
        GameSessionExplore.Instance.AddCommandTranslator(this);
        //Animator = new PlayerAnimator(GetComponent<Animator>());
        
    }

    void Update()
    {
        Debug.Log("playercontrol" + isControllable);
        Debug.Log("currentState" + StateMachine.CurrentState);
            if (!isControllable && StateMachine.CurrentState != StateMachine.RiddleState)
            {
                StateMachine.SetState(StateMachine.RiddleState);
            }
        

        StateMachine.Tick();
    }

    void FixedUpdate()
    {
        //if (!isControllable) return;
        //StateMachine.Tick();
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
*/