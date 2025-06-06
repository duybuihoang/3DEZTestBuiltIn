using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : Entity
{
    [SerializeField] private Animator anim;
    [SerializeField] private CharacterController controller;


    private StateMachine stateMachine;

    private DamageReceiver receiver;
    private DamageSender sender;

    Dictionary<string, bool> Predicates = new Dictionary<string, bool>();
    private string currentAction;

    [SerializeField] private GameObject target;
    [SerializeField] private float attackDistance = 0.7f;

    private Vector3 backwardDirection = new Vector3(-1, 0, 1);
    private Vector3 forwardDirection  = new Vector3(1, 0, -1);

    [Header("Moving Data")]
    [SerializeField] private float moveSpeed = 1f;

    private float maxDelayTime = 2f;
    private float minDelayTime = 2f;
    private float delayedTime;
    private float actionTime;

    string[] attacks = { "HeadPunch", "KidneyPunchLeft", "KidneyPunchRight", "Stomach Punch" };


    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        sender = GetComponentInChildren<DamageSender>();
        receiver = GetComponentInChildren<DamageReceiver>();
        stateMachine = new StateMachine();

        Predicates.Add("Waiting", true);
        Predicates.Add("Idle", false);

        Predicates.Add("HeadPunch", false);
        Predicates.Add("KidneyPunchLeft", false);
        Predicates.Add("KidneyPunchRight", false);
        Predicates.Add("Stomach Punch", false);

        Predicates.Add("Big Jump", false);

        Predicates.Add("Head Hit", false);
        Predicates.Add("Stomach Hit", false);
        Predicates.Add("Kidney Hit", false);
        Predicates.Add("Knock Out", false);

        EnemyIdleState idleState = new EnemyIdleState(this, anim);
        EnemyWaitingState waitingState = new EnemyWaitingState(this, anim);

        EnemyHeadPunchState headPunchState = new EnemyHeadPunchState(this, anim);
        EnemyKidneyPunchLeftState kidneyPunchLeftState = new EnemyKidneyPunchLeftState(this, anim);
        EnemyKidneyPunchRightState kidneyPunchRightState = new EnemyKidneyPunchRightState(this, anim);
        EnemyStomachPunchState stomachPunchState = new EnemyStomachPunchState(this, anim);
        EnemyBigJumpState bigJumpState = new EnemyBigJumpState(this, anim);

        EnemyHeadHitState headHitState = new EnemyHeadHitState(this, anim);
        EnemyKidneyHitState kidneyHitState = new EnemyKidneyHitState(this, anim);
        EnemyStomachHitState stomachHitState = new EnemyStomachHitState(this, anim);
        EnemyKnockOutState knockOutState = new EnemyKnockOutState(this, anim);

        At(waitingState, idleState, new FuncPredicate(() => !Predicates["Waiting"]));
        Any(waitingState, new FuncPredicate(() => Predicates["Waiting"]));


        At(idleState, headPunchState, new FuncPredicate(() => Predicates["HeadPunch"]));
        At(headPunchState, idleState, new FuncPredicate(() => !Predicates["HeadPunch"]));

        At(idleState, kidneyPunchLeftState, new FuncPredicate(() => Predicates["KidneyPunchLeft"]));
        At(kidneyPunchLeftState, idleState, new FuncPredicate(() => !Predicates["KidneyPunchLeft"]));

        At(idleState, kidneyPunchRightState, new FuncPredicate(() => Predicates["KidneyPunchRight"]));
        At(kidneyPunchRightState, idleState, new FuncPredicate(() => !Predicates["KidneyPunchRight"]));

        At(idleState, stomachPunchState, new FuncPredicate(() => Predicates["Stomach Punch"]));
        At(stomachPunchState, idleState, new FuncPredicate(() => !Predicates["Stomach Punch"]));

        At(idleState, bigJumpState, new FuncPredicate(() => Predicates["Big Jump"]));
        At(bigJumpState, idleState, new FuncPredicate(() => !Predicates["Big Jump"]));

        Any(stomachHitState, new FuncPredicate(() => Predicates["Stomach Hit"]));
        Any(headHitState, new FuncPredicate(() => Predicates["Head Hit"]));
        Any(kidneyHitState, new FuncPredicate(() => Predicates["Kidney Hit"]));
        Any(knockOutState, new FuncPredicate(() => Predicates["Knock Out"]));


        At(stomachHitState, idleState, new FuncPredicate(() => !Predicates["Stomach Hit"]));
        At(headHitState, idleState, new FuncPredicate(() => !Predicates["Head Hit"]));
        At(kidneyHitState, idleState, new FuncPredicate(() => !Predicates["Kidney Hit"]));


        stateMachine.SetState(waitingState);

        target = GameManager.Instance.GetCurrentPlayer();

    }

    private void Start()
    {
        actionTime = Time.time;
        delayedTime = Random.Range(minDelayTime, maxDelayTime);
    }

    private void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    private void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    private void Update()
    {
        stateMachine.Update();

        HeadHitCheck();
        StomachHitCheck();
        KidneyHitCheck();
        KnockOutCheck();
        BehaviourCheck();

    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    private void HeadHitCheck() => Predicates["Head Hit"] = receiver.JustGotDamage && !receiver.IsDead() && receiver.AttackAnimation == "Head Hit";
    private void StomachHitCheck() => Predicates["Stomach Hit"] = receiver.JustGotDamage && !receiver.IsDead() && receiver.AttackAnimation == "Stomach Hit";
    private void KidneyHitCheck() => Predicates["Kidney Hit"] = receiver.JustGotDamage && !receiver.IsDead() && receiver.AttackAnimation == "Kidney Hit";

    private void KnockOutCheck() => Predicates["Knock Out"] = receiver.IsDead();
    public void ResetState(string state)
    {
        if (string.IsNullOrEmpty(state))
        {
            Debug.LogWarning("Please Provide the state name in the animationClip");
            return;
        }

        Predicates[state] = false;
        currentAction = "";
        receiver.JustGotDamage = false;
    }



    private void BehaviourCheck()
    {
        if (Time.time >= actionTime + delayedTime)
        {
            actionTime = Time.time;
            delayedTime = Random.Range(minDelayTime, maxDelayTime);
            if (target != null)
            {
                if (Vector3.Distance(this.transform.position, target.transform.position) <= 0.7f)
                {
                    DoAction(attacks[Random.Range(0, attacks.Length)]);
                }
                else
                {
                    DoAction("Big Jump");
                }
            }
        }
    } 



    public void Jump()
    {
        if (Vector3.Distance(this.transform.position, target.transform.position) <= 0.7f) return;
        controller.Move(forwardDirection * moveSpeed * Time.deltaTime);
    }

    private void DoAction(string name)
    {
        Predicates[name] = true;
        currentAction = name;
    }

    public void SendDamage()
    {
        string hitAnimation = "";
        int damage = 0;
        switch (currentAction)
        {
            case "HeadPunch":
                hitAnimation = "Head Hit";
                damage = 1;
                break;
            case "KidneyPunchLeft":
                hitAnimation = "Kidney Hit";
                damage = 2;
                break;
            case "KidneyPunchRight":
                hitAnimation = "Kidney Hit";
                damage = 3;
                break;
            case "Stomach Punch":
                hitAnimation = "Stomach Hit";
                damage = 2;
                break;
            default:
                break;
        }
        sender.Send(damage, hitAnimation);
    }

    public void ResetRotation()
    {
    }

    private void KnockOut()
    {
        GameManager.Instance.OnCharacterKnockOut(this.gameObject);
        Destroy(gameObject);
    }

    public void EnterArena()
    {
        ResetDict();
        Predicates["Waiting"] = false;
    }

    public void ExitArena()
    {
        ResetDict();
        Predicates["Waiting"] = true;
    }

    public void ResetDict()
    {
        List<string> keys = new List<string>(Predicates.Keys);
        foreach (var key in keys)
        {
            Predicates[key] = false;
        }
    }


    public void SetTarget()
    {
        target = GameManager.Instance.GetCurrentPlayer();
    }

    public void SetDelayTime(float max, float min)
    {
        maxDelayTime = max;
        minDelayTime = min;
    }
}
