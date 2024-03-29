using FateGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class PoliceCar : MonoBehaviour
{
    [SerializeField] private int numberOfPolicesInPoliceCar = 5;
    [SerializeField] private int checkRange = 20;
    [SerializeField] private LayerMask minionLayerMask = 0;
    [SerializeField] private Transform spawnPositionTransform;
    [SerializeField] private Transform minionDestinationTransform;
    [SerializeField] private float spawnCooldown = 3f;
    [SerializeField] private Transform meshTransform = null;
    [SerializeField] private TextMeshProUGUI numberText;

    private int totalNumberOfPolices = 0;
    private Animator anim;
    private Transform _transform;
    private List<Police> policesOnField = new List<Police>();
    public Transform MinionDestinationTransform { get => minionDestinationTransform; }
    public List<Police> PolicesOnField { get => policesOnField; }
    public Transform Transform { get => _transform; set => _transform = value; }

    private void Awake()
    {
        _transform = transform;
        anim = GetComponent<Animator>();
        totalNumberOfPolices = numberOfPolicesInPoliceCar;
    }

    private void Start()
    {
        SetNumberText();
    }

    public void Come()
    {
        anim.SetTrigger("Come");
    }

    private void SearchTask()
    {
        StartCoroutine(SearchTaskCoroutine());
    }

    private void Update()
    {
        if (GameManager.Instance.State == GameState.IN_GAME)
        {
            MoveForward();
        }
    }

    private void SetNumberText()
    {
        numberText.text = numberOfPolicesInPoliceCar + "/" + totalNumberOfPolices;
    }


    private void MoveForward()
    {
        _transform.position = Vector3.MoveTowards(_transform.position, _transform.position + _transform.forward, Time.deltaTime * 1);
    }

    private Minion FindFurthestMinion(Collider[] colliders)
    {
        float maxDistance = 0;
        Minion maxDistanceMinion = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            Transform colliderTransform = collider.transform;
            float distance = Vector3.SqrMagnitude(colliderTransform.position - _transform.position);
            if (distance > maxDistance)
            {
                Minion minion = collider.GetComponent<Minion>();
                if ((minion.State == Minion.MinionState.HOLDING_STUFF || minion.State == Minion.MinionState.CARRYING_STUFF) && !minion.Slot.IsOccupied())
                {
                    maxDistance = distance;
                    maxDistanceMinion = minion;
                }
            }
        }
        return maxDistanceMinion;
    }

    private IEnumerator SearchTaskCoroutine()
    {
        print("Search Task");
        Collider[] colliders = Physics.OverlapSphere(_transform.position, checkRange, minionLayerMask);
        Minion maxDistanceMinion = FindFurthestMinion(colliders);
        if (maxDistanceMinion)
        {
            List<Police> returningPolices = policesOnField.Where((police) => police.State == Police.PoliceState.RETURNING).ToList();
            if (returningPolices.Count > 0)
            {
                float minDistance = int.MaxValue;
                Police minDistancePolice = null;
                for (int i = 0; i < returningPolices.Count; i++)
                {
                    Police police = returningPolices[i];
                    float distance = Vector3.SqrMagnitude(police.Transform.position - _transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minDistancePolice = police;
                    }
                }
                minDistancePolice.AssignTask(maxDistanceMinion.Slot);
            }
            else
                SpawnPolice(maxDistanceMinion);
        }
        yield return new WaitForSeconds(spawnCooldown);
        SearchTask();
    }
    private Police SpawnPolice(Minion target)
    {
        if (0 < numberOfPolicesInPoliceCar)
        {
            numberOfPolicesInPoliceCar--;
            Transform policeTransform = ObjectPooler.Instance.SpawnFromPool("Police", spawnPositionTransform.position, Quaternion.identity).transform;
            Police police = policeTransform.GetComponent<Police>();
            policesOnField.Add(police);
            police.AssignTask(target.Slot);
            SetNumberText();
            return police;
        }
        return null;
    }
    public void DeactivatePolice(Police police)
    {
        policesOnField.Remove(police);
        numberOfPolicesInPoliceCar++;
        police.ChangeState(Police.PoliceState.INACTIVE);
        SetNumberText();
    }

    public void LosePolice(Police police)
    {
        policesOnField.Remove(police);
        police.ChangeState(Police.PoliceState.INACTIVE);
        police.gameObject.SetActive(false);
        totalNumberOfPolices--;
        SetNumberText();
    }
}
