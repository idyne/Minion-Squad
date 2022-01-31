using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class GoingToStuff : State
    {
        public GoingToStuff(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.AVALIABLE;
        }

        // targetSlot Avaliable state'inde  atandý
        public override void OnEnter()
        {
            minion.Agent.enabled = true;
            minion.Animator.enabled = true;
            minion.Agent.speed = minion.NormalSpeed;
            minion.TargetSlot.Occupy(minion);
            if (minion.OverlapStuffs.Contains(minion.TargetSlot.Owner))
                minion.ChangeState(Minion.MinionState.HOLDING_STUFF);
            else
                minion.Agent.SetDestination(minion.TargetSlot.SlotTransform.position);
        }

        public override void OnExit()
        {
            minion.Agent.enabled = false;
            minion.CurrentSlot = minion.TargetSlot;
            minion.TargetSlot = null;
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Stuff"))
            {
                Stuff stuff = other.GetComponent<Stuff>();
                if (stuff == minion.TargetSlot.Owner)
                {
                    minion.ChangeState(Minion.MinionState.HOLDING_STUFF);
                }
            }
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
        }
    }
}