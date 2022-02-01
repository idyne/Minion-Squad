using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.PoliceState
{
    public class GoingToMinion : State
    {
        public GoingToMinion(Police police, string name) : base(police, name)
        {
        }
        public override bool CanEnter()
        {
            return police.State == Police.PoliceState.INACTIVE || police.State == Police.PoliceState.RETURNING;
        }

        public override void OnEnter()
        {
            police.Anim.enabled = true;
            if (police.OverlapMinions.Contains(police.TargetSlot.Owner))
                police.ChangeState(Police.PoliceState.CARRYING_MINION);
            else
            {
                police.Agent.enabled = true;
                police.Agent.speed = police.NormalSpeed;
            }
        }

        public override void OnExit()
        {
        }


        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Minion"))
            {
                Minion minion = other.GetComponent<Minion>();
                if (minion == police.TargetSlot.Owner)
                {
                    if (minion.State != Minion.MinionState.RETURN_ANIMATING)
                        police.ChangeState(Police.PoliceState.CARRYING_MINION);
                    else
                        police.ChangeState(Police.PoliceState.RETURNING);

                }
            }
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
            //TODO bu kontrol kesin deðil
            if (police.TargetSlot.Owner.gameObject.activeSelf && police.TargetSlot.Owner.State != Minion.MinionState.RETURN_ANIMATING)
                police.Agent.SetDestination(police.TargetSlot.Owner.Transform.position);
            else
                police.ChangeState(Police.PoliceState.RETURNING);
        }
    }
}