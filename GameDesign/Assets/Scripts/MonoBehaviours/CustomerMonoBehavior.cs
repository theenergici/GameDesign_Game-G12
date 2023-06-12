using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NPCMovementManager))]
public class CustomerMonoBehavior : MonoBehaviour, Clickable
{
    [SerializeField] public int id;
    [ReadOnly][SerializeField] private string currentState;
    [SerializeField] public float baseSpeed;
    [ReadOnly][SerializeField] public float currentSpeed;
    [SerializeField] public float maxTimeToReachWaypoint = 15f;
    [ReadOnly][SerializeField] public int clickCunt = 0;
    [SerializeField] public int requiredClicks = 1;
    [SerializeField] public float clickTime;
    [SerializeField] public bool wearsMask;
    [SerializeField] public int pointValue;
    [ReadOnly][SerializeField] public bool isFrozen = false;
    [SerializeField] public float frozenTime = 0f;
    private TaserManager taserManager = TaserManager.Instance;
    protected FiniteStateMachine<CustomerMonoBehavior> fsm;
    [SerializeField] protected NPCMovementManager movementManager;
    [SerializeField] protected Animator animator;
    public bool onGoingAnimation { get; private set; }
    private GameObject _mask;
    public string defaultLayer { get { return "Default"; } }

    public AudioSource audioSource;

    public AudioClip shot;
    public AudioClip taser;
    public AudioClip missHit;
    public AudioClip cough;

    void Start()
    {
        onGoingAnimation = false;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) Debug.LogError($"Missing Animator component: {name}");
        }
        if (movementManager == null)
        {
            movementManager = GetComponent<NPCMovementManager>();
            if (movementManager == null) Debug.LogError($"Missing MovementManager component: {name}");
        }
        _mask = transform.Find("Mask").gameObject;
        if (_mask == null) { Debug.LogError($"Error finding Mask of: {name}"); }
        fsm = new FiniteStateMachine<CustomerMonoBehavior>(this);
        movementManager.MaxTimeToReachTarget = maxTimeToReachWaypoint;
        maskNPC(wearsMask);
        changeSpeed();
        audioSource = GetComponent<AudioSource>();

        //ideal if setting the FSM is the last function. just to make sure the other parameters are set if they are to be changed by the states.

        setFSM();


        
    }

    void Update()
    {
        currentState = fsm._currentState.Name;
        fsm.Tik();
    }

    #region Changing Methods
    protected virtual void setFSM()
    {

        State frozen = new Frozen("Frozen", this, animator);
        State moving = new MovingState("Moving", this, movementManager);

        fsm.AddTransition(frozen, moving, () => !isFrozen);
        fsm.AddTransition(moving, frozen, () => isFrozen);

        fsm.SetState(moving);
    }
    protected virtual void onHitBehaviour()
    {
        wearsMask = true;
        audioSource.PlayOneShot(shot, 0.7f);
        PointsManager.Instance.TriggerEvent_IncrementPoints(pointValue);
        maskNPC();
    }
    protected virtual void onFreezeBehaviour()
    {
        StartCoroutine(StartFreeze(frozenTime));
    }
    protected virtual void DodgeHitBehaviour()
    {
        audioSource.PlayOneShot(missHit, 0.7f);
        StartCoroutine(DoTriggerAnimation("SmallHit"));
    }

    #endregion

    public void Click(ClickType clickType)
    {
        onGoingAnimation = false;
        if (clickType == ClickType.LEFT_CLICK)
        {
            clickCunt++;
            if (!wearsMask && clickCunt >= requiredClicks)
            {
                
                onHitBehaviour();
            }
            else if (wearsMask)
            {
                
                DodgeHitBehaviour();
                PointsManager.Instance.TriggerEvent_IncrementPoints(-1 * pointValue);
            }
            else
            {
                //TODO Do here whatever feed back for click that are not the masking ones (like dinosaurs)
                //maybe play a sound or maybe add another effect to signal you hit him.




            }
        }
        if (clickType == ClickType.RIGHT_CLICK)
        {
            if (taserManager.useTaser())
            {
                onFreezeBehaviour();
            }
        }

    }

    private IEnumerator StartFreeze(float duration)
    {
        if (!isFrozen){
            audioSource.PlayOneShot(taser, 0.7f);
			isFrozen = true;
            yield return new WaitForSeconds(duration);
            isFrozen = false;
        }
    }
    public void changeSpeed()
    {
        changeSpeed(baseSpeed);
    }
    public void changeSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
        animator.SetFloat("Speed", newSpeed);
        if (newSpeed <= 0.01)
        {
            movementManager.agent.isStopped = true;
        }
        else
        {
            movementManager.agent.isStopped = false;
            movementManager.agent.speed = newSpeed;
        }

    }
    public void changeLayer(string LayerName)
    {

        int Layer = LayerMask.NameToLayer(LayerName);
        if (Layer < 0)
        {
            Debug.LogWarning($"Could not find the expected layer {LayerName}");
            Layer = LayerMask.NameToLayer(defaultLayer);
        }

        gameObject.layer = Layer;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = Layer;
        }


    }
    public void maskNPC(bool value)
    {
        if (value) maskNPC(); else unmaskNPC();
    }
    public void maskNPC()
    {
        wearsMask = true;
        _mask.SetActive(true);
        tag = "Masked";


        //TODO determine from which side. -> probably has to be done by the clicking , manager. -> for now default hit is set

        StartCoroutine(DoTriggerAnimation("GotHit"));
    }

    public void doTriggerAnimation(string name)
    {
        StartCoroutine(DoTriggerAnimation(name));
    }
    protected IEnumerator DoTriggerAnimation(string name)
    {
        onGoingAnimation = true;
        changeSpeed(0);
        animator.SetTrigger(name);
        yield return new WaitWhile(() => onGoingAnimation);
        changeSpeed();
    }
    public void animationFinished()
    {
        onGoingAnimation = false;
    }
    public void unmaskNPC()
    {
        tag = "Untagged";
        wearsMask = false;
        _mask.SetActive(false);
    }

}
