using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class OnAir : State
    {
        public OnAir(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.INACTIVE;
        }

        // Motion is set by Truck in ThrowMinion function
        public override void OnEnter()
        {
            minion.Rigidbody.AddForce(minion.Motion.Force, ForceMode.Impulse);
            minion.Rigidbody.AddTorque(minion.Motion.Force, ForceMode.Impulse);
            LeanTween.delayedCall(minion.Motion.Time + Random.Range(0.2f, 0.5f), () =>
            {
                minion.Transform.LeanMoveY(0.1f, 0.2f);
                minion.Transform.LeanRotate(Vector3.zero, 0.2f).setOnComplete(() =>
                {
                    minion.ChangeState(Minion.MinionState.AVALIABLE);
                });
            });
        }

        public override void OnExit()
        {
            minion.Motion = null;
            minion.Rigidbody.isKinematic = true;
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