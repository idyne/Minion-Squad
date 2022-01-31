using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using TMPro;

public class Truck : MonoBehaviour
{
    [SerializeField] private int numberOfMinionsInTruck = 50;
    [SerializeField] private LayerMask throwLayermask;
    [SerializeField] private Transform spawnPositionTransform;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private Transform mesh = null;
    private int totalNumberOfMinions = 0;
    private Camera mainCamera;
    private float throwCooldown = 0.2f;
    private float throwTime = -10;
    private List<Minion> minionsOnField = new List<Minion>();
    private Transform _transform;

    public List<Minion> MinionsOnField { get => minionsOnField; }
    public Transform Transform { get => _transform; }
    public int NumberOfMinionsInTruck { get => numberOfMinionsInTruck; set => numberOfMinionsInTruck = value; }

    private void Awake()
    {
        mainCamera = Camera.main;
        _transform = transform;
        totalNumberOfMinions = numberOfMinionsInTruck;
    }


    private void Update()
    {
        if (GameManager.Instance.State == GameState.IN_GAME)
        {
            CheckInput();
            MoveForward();
        }
    }

    private void CheckInput()
    {
        if (Input.GetMouseButton(0))
        {
            ThrowMinion();
        }
    }

    private void MoveForward()
    {
        _transform.position = Vector3.MoveTowards(_transform.position, _transform.position + _transform.forward, Time.deltaTime * 1);
    }



    private void SetNumberText()
    {
        numberText.text = numberOfMinionsInTruck + "/" + totalNumberOfMinions;
    }

    private void ThrowMinion()
    {
        if (Time.time > throwTime + throwCooldown && 0 < numberOfMinionsInTruck)
        {
            throwTime = Time.time;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, throwLayermask))
            {
                if (hit.transform.CompareTag("Building")) return;
                numberOfMinionsInTruck--;
                Vector3 to = hit.point;
                float distance = Vector3.Distance(to, spawnPositionTransform.position);
                Transform minionTransform = ObjectPooler.Instance.SpawnFromPool("Minion", spawnPositionTransform.position, Quaternion.identity).transform;
                Minion minion = minionTransform.GetComponent<Minion>();
                minionsOnField.Add(minion);
                float time = Mathf.Sqrt(distance) / 3f;
                ProjectileMotion.Motion motion = minionTransform.CreateProjectileMotion(to, time);
                minion.Motion = motion;
                minion.ChangeState(Minion.MinionState.ON_AIR);
                SetNumberText();
            }
        }
    }

    public void DeactivateMinion(Minion minion)
    {
        numberOfMinionsInTruck++;
        minionsOnField.Remove(minion);
        minion.ChangeState(Minion.MinionState.INACTIVE);
        minion.gameObject.SetActive(false);
        SetNumberText();
    }

    public void LoseMinion()
    {
        totalNumberOfMinions--;
        SetNumberText();
    }
}
