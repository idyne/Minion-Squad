using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FateGames;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Minion : MonoBehaviour, IPooledObject, IMoveable
{
    [SerializeField] private float stuffCheckRadius = 5;
    [SerializeField] private LayerMask stuffLayermask = 0;
    [SerializeField] private Slot<Minion, Police> slot;
    [SerializeField] private float normalSpeed = 3.5f;
    [SerializeField] private float carryingSpeed = 2.5f;
    private static Truck truck = null;
    private static PoliceCar policeCar = null;
    private MinionState state = MinionState.ON_AIR;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform _transform;
    private Vector3 destination = Vector3.zero;
    private BoxCollider boxCollider;
    private Animator anim = null;
    private Stuff targetStuff = null;
    private Slot<Stuff, Minion> targetSlot = null;
    private Slot<Police, Minion> targetPoliceSlot = null;
    private Police chasingBy = null;
    private bool inTruck = false;
    private Minion targetMinion = null;
    public ProjectileMotion.Motion Motion;
    private float stateTime = -1;

    public MinionState State { get => state; }
    public Police ChasingBy { get => chasingBy; set => chasingBy = value; }
    public Slot<Minion, Police> Slot { get => slot; }
    public Transform Transform { get => _transform; }
    public Slot<Police, Minion> TargetPoliceSlot { get => targetPoliceSlot; set => targetPoliceSlot = value; }
    public Rigidbody Rigidbody { get => rb; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        _transform = transform;
        if (!truck)
            truck = FindObjectOfType<Truck>();
        if (!policeCar)
            policeCar = FindObjectOfType<PoliceCar>();
    }

    private void Update()
    {
        if (state == MinionState.HOLDING_STUFF && Time.time > stateTime + 10 && Vector3.Distance(_transform.position, truck.Transform.position) > 30)
        {
            ChangeState(MinionState.RETURNING);
        }
        else if (state == MinionState.GOING_TO_RESCUE)
        {
            if (targetMinion.gameObject.activeSelf && targetMinion.state == MinionState.BEING_RESCUED)
            {
                agent.SetDestination(targetMinion.Transform.position);
            }
            else
            {
                ChangeState(MinionState.RETURNING);
            }
        }
        else if (state == MinionState.ARRESTED || state == MinionState.BEING_RESCUED)
        {
            agent.SetDestination(policeCar.MinionDestinationTransform.position);
        }
        else if (agent.enabled && state == MinionState.RETURNING)
        {
            agent.SetDestination(truck.transform.position);
        }
    }
    public void ChangeState(MinionState newState)
    {
        bool canChangeState;
        switch (newState)
        {
            case MinionState.AVALIABLE:
                canChangeState = state == MinionState.ON_AIR;
                break;
            case MinionState.GOING_TO_STUFF:
                canChangeState = state == MinionState.AVALIABLE;
                break;
            case MinionState.HOLDING_STUFF:
                canChangeState = state == MinionState.GOING_TO_STUFF || state == MinionState.CARRYING_STUFF;
                break;
            case MinionState.CARRYING_STUFF:
                canChangeState = state == MinionState.HOLDING_STUFF;
                break;
            case MinionState.ARRESTED:
                canChangeState = state == MinionState.HOLDING_STUFF || state == MinionState.CARRYING_STUFF;
                break;
            case MinionState.BEING_RESCUED:
                canChangeState = state == MinionState.ARRESTED;
                break;
            case MinionState.CARRYING_POLICE:
                canChangeState = state == MinionState.GOING_TO_RESCUE || state == MinionState.BEING_RESCUED;
                break;
            case MinionState.GOING_TO_RESCUE:
                canChangeState = state == MinionState.AVALIABLE;
                break;
            case MinionState.RETURNING:
                canChangeState = state == MinionState.AVALIABLE || state == MinionState.HOLDING_STUFF || state == MinionState.CARRYING_STUFF || state == MinionState.CARRYING_POLICE || state == MinionState.GOING_TO_RESCUE;
                break;
            case MinionState.RETURN_ANIMATING:
                canChangeState = state == MinionState.RETURNING;
                break;
            default:
                canChangeState = true;
                break;
        }
        if (canChangeState)
        {
            state = newState;
            stateTime = Time.time;
            switch (state)
            {
                case MinionState.ON_AIR:
                    Enter_ON_AIR_State();
                    break;
                case MinionState.AVALIABLE:
                    Enter_AVALIABLE_State();
                    break;
                case MinionState.GOING_TO_STUFF:
                    Enter_GOING_TO_STAFF_State();
                    break;
                case MinionState.HOLDING_STUFF:
                    Enter_HOLDING_STUFF_State();
                    break;
                case MinionState.CARRYING_STUFF:
                    Enter_CARRYING_STUFF_State();
                    break;
                case MinionState.ARRESTED:
                    Enter_ARRESTED_State();
                    break;
                case MinionState.BEING_RESCUED:
                    Enter_BEING_RESCUED_State();
                    break;
                case MinionState.CARRYING_POLICE:
                    Enter_CARRYING_POLICE_State();
                    break;
                case MinionState.GOING_TO_RESCUE:
                    Enter_GOING_TO_RESCUE_State();
                    break;
                case MinionState.RETURNING:
                    Enter_RETURNING_State();
                    break;
                case MinionState.RETURN_ANIMATING:
                    Enter_RETURN_ANIMATING_State();
                    break;
            }
        }
        else
            Debug.Log(string.Format("Invalid Minion state transition: {0}=>{1}", state, newState), this);
    }

    private void Enter_ON_AIR_State()
    {
        agent.enabled = false;
        boxCollider.enabled = false;
        anim.enabled = false;
        targetStuff = null;
        targetSlot = null;
        targetMinion = null;
        chasingBy = null;
        rb.isKinematic = false;
        Rigidbody.AddForce(Motion.Force, ForceMode.Impulse);
        Rigidbody.AddTorque(Motion.Force, ForceMode.Impulse);
        LeanTween.delayedCall(Motion.Time + Random.Range(0.2f, 0.5f), () =>
        {
            _transform.LeanMoveY(0.1f, 0.2f);
            _transform.LeanRotate(Vector3.zero, 0.2f).setOnComplete(() =>
            {
                ChangeState(MinionState.AVALIABLE);
            });
        });
    }

    private void Enter_AVALIABLE_State()
    {
        agent.enabled = false;
        boxCollider.enabled = false;
        anim.enabled = false;
        targetStuff = null;
        targetSlot = null;
        targetMinion = null;
        chasingBy = null;
        rb.isKinematic = true;
        if (!SearchTask())
            ChangeState(MinionState.RETURNING);
    }

    private void Enter_GOING_TO_STAFF_State()
    {
        boxCollider.enabled = true;
        agent.enabled = true;
        anim.enabled = true;
        targetMinion = null;
        agent.speed = normalSpeed;
        agent.SetDestination(destination);
    }

    private void Enter_HOLDING_STUFF_State()
    {
        boxCollider.enabled = true;
        agent.enabled = false;
        anim.enabled = true;
        targetMinion = null;
        _transform.parent = targetSlot.SlotTransform;
        _transform.SetPositionAndRotation(targetSlot.SlotTransform.position, targetSlot.SlotTransform.rotation);
        targetStuff.TakePosition(this);
    }

    private void Enter_CARRYING_STUFF_State()
    {
        boxCollider.enabled = true;
        agent.enabled = false;
        anim.enabled = true;
        targetMinion = null;
        if (!(chasingBy && chasingBy.TargetMinion != this))
            chasingBy = null;
    }

    private void Enter_ARRESTED_State()
    {
        agent.enabled = true;
        boxCollider.enabled = true;
        anim.enabled = false;
        agent.speed = carryingSpeed;
        targetSlot.Abandon();
        targetStuff = null;
        targetSlot = null;
        targetMinion = null;
        Debug.Log("dene " + targetSlot, this);
        _transform.parent = null;
    }

    private void Enter_BEING_RESCUED_State()
    {
        agent.enabled = true;
        boxCollider.enabled = true;
        anim.enabled = false;
        agent.speed = carryingSpeed;
        targetStuff = null;
        targetSlot = null;
        targetMinion = null;
    }

    private void Enter_CARRYING_POLICE_State()
    {
        agent.enabled = false;
        boxCollider.enabled = false;
        anim.enabled = true;
        targetStuff = null;
        chasingBy = null;
        targetPoliceSlot.Occupy(this);
        _transform.parent = targetPoliceSlot.SlotTransform;
        _transform.SetPositionAndRotation(targetPoliceSlot.SlotTransform.position, targetPoliceSlot.SlotTransform.rotation);
    }

    private void Enter_GOING_TO_RESCUE_State()
    {
        agent.enabled = true;
        boxCollider.enabled = true;
        anim.enabled = true;
        agent.speed = normalSpeed;
        targetStuff = null;
        targetSlot = null;
        targetMinion.ChangeState(MinionState.BEING_RESCUED);
    }

    private void Enter_RETURNING_State()
    {
        agent.enabled = true;
        boxCollider.enabled = true;
        anim.enabled = true;
        agent.speed = normalSpeed;
        _transform.parent = null;
        targetStuff = null;
        targetSlot = null;
        targetMinion = null;
        if (chasingBy)
        {
            chasingBy.ChangeState(Police.PoliceState.RETURNING);
            chasingBy = null;
        }
        if (!inTruck)
            agent.SetDestination(truck.transform.position);
        else
            GetOnTruck();
    }

    private void Enter_RETURN_ANIMATING_State()
    {

    }

    private bool SearchTask()
    {
        bool isTaskAssigned = false;
        if (state == MinionState.AVALIABLE)
        {
            Collider[] colliders = Physics.OverlapSphere(_transform.position, stuffCheckRadius, stuffLayermask);
            Collider[] minionColliders = colliders.Where((collider) => collider.CompareTag("Minion")).ToArray();
            if (minionColliders.Length > 0)
            {
                float minDistance = int.MaxValue;
                Minion minDistanceMinion = null;
                for (int i = 0; i < minionColliders.Length; i++)
                {
                    Collider collider = minionColliders[i];
                    Minion minion = collider.GetComponent<Minion>();
                    if (minion.state != MinionState.ARRESTED) continue;
                    Transform colliderTransform = collider.transform;
                    float distance = Vector3.SqrMagnitude(colliderTransform.position - _transform.position);
                    if (distance < minDistance)
                    {
                        minDistanceMinion = minion;
                    }
                }
                if (minDistanceMinion)
                {
                    targetMinion = minDistanceMinion;
                    ChangeState(MinionState.GOING_TO_RESCUE);
                    isTaskAssigned = true;
                }
            }
            if (!isTaskAssigned)
            {
                float minDistance = int.MaxValue;
                Stuff minDistanceStuff = null;
                Slot<Stuff, Minion> minDistanceSlot = null;
                for (int i = 0; i < colliders.Length; i++)
                {
                    Collider collider = colliders[i];
                    Transform colliderTransform = collider.transform;
                    if (colliderTransform.CompareTag("Stuff"))
                    {
                        Stuff stuff = collider.GetComponent<Stuff>();
                        float distance = Vector3.SqrMagnitude(colliderTransform.position - _transform.position);
                        if (distance < minDistance)
                        {
                            Slot<Stuff, Minion> slot = stuff.GetAvaliableSlot();
                            if (slot != null)
                            {
                                minDistance = distance;
                                minDistanceStuff = stuff;
                                minDistanceSlot = slot;
                            }
                        }
                    }
                }
                if (minDistanceStuff)
                {
                    minDistanceSlot.Occupy(this);
                    destination = minDistanceSlot.SlotTransform.position;
                    targetStuff = minDistanceStuff;
                    targetSlot = minDistanceSlot;
                    ChangeState(MinionState.GOING_TO_STUFF);
                    isTaskAssigned = true;
                }
            }
        }
        else
        {
            Debug.Log("Minion is not avaliable to search a task!", this);
        }
        return isTaskAssigned;
    }

    public void GetOnTruck()
    {
        truck.NumberOfMinionsInTruck++;
        truck.RemoveMinion(this);
    }

    public void GetOnPoliceCar()
    {
        truck.RemoveMinion(this);
        truck.LostMinion();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = true;
        if (state == MinionState.GOING_TO_STUFF && other.CompareTag("Stuff"))
        {
            Stuff stuff = other.GetComponent<Stuff>();
            if (stuff == targetStuff)
            {
                ChangeState(MinionState.HOLDING_STUFF);
            }
        }
        else if (state == MinionState.RETURNING && inTruck)
        {
            GetOnTruck();
        }
        else if ((state == MinionState.ARRESTED || state == MinionState.BEING_RESCUED) && other.CompareTag("Police Car"))
        {
            policeCar.Bounce(1.3f);
            Police police = slot.OccupiedBy;
            slot.Abandon();
            police.ChangeState(Police.PoliceState.RETURNING);
            police.EnterToTheVehicle(policeCar.Transform);
            EnterToTheVehicle(policeCar.Transform);
            GetOnPoliceCar();
        }
        else if (state == MinionState.GOING_TO_RESCUE && other.CompareTag("Minion"))
        {
            Minion minion = other.GetComponent<Minion>();
            if (minion == targetMinion)
            {
                Police police = minion.chasingBy;
                police.ChangeState(Police.PoliceState.BEING_CARRIED);
                targetPoliceSlot = police.GetAvaliableSlot();
                ChangeState(MinionState.CARRYING_POLICE);
                minion.targetPoliceSlot = police.GetAvaliableSlot();
                minion.ChangeState(MinionState.CARRYING_POLICE);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = false;
    }

    public void OnObjectSpawn()
    {
        agent.enabled = false;
        boxCollider.enabled = false;
        anim.enabled = false;
        targetStuff = null;
        targetSlot = null;
        targetMinion = null;
        chasingBy = null;
        rb.isKinematic = false;
        inTruck = false;
    }

    public void Abandon()
    {
        _transform.parent = null;
    }
    public void EnterToTheVehicle(Transform target)
    {
        boxCollider.enabled = false;
        agent.enabled = false;
        _transform.LeanScale(Vector3.zero, 0.4f).setOnComplete(() => _transform.localScale = Vector3.one);
        ProjectileMotion.SimulateProjectileMotion(_transform, target.position, 0.5f, () =>
        {
            gameObject.SetActive(false);
        });
    }

    public enum MinionState { ON_AIR, AVALIABLE, GOING_TO_STUFF, HOLDING_STUFF, CARRYING_STUFF, CARRYING_POLICE, ARRESTED, GOING_TO_RESCUE, BEING_RESCUED, RETURNING, RETURN_ANIMATING }
}
