using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.PoliceState
{
    public class CarryingMinion : State
    {
        public CarryingMinion(Police police, string name) : base(police, name)
        {
        }
        public override bool CanEnter()
        {
            return police.State == Police.PoliceState.GOING_TO_MINION;
        }

        public override void OnEnter()
        {
            police.CurrentSlot = police.TargetSlot;
            police.TargetSlot = null;
            police.Agent.enabled = false;
            police.CurrentSlot.Owner.TakePosition(police);
            police.CurrentSlot.Owner.ChangeState(Minion.MinionState.ARRESTED);
        }

        public override void OnExit()
        {
        }


        public override void OnTriggerEnter(Collider other)
        {

        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
        }
    }
}