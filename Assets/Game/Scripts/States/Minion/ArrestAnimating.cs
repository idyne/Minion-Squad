using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class ArrestAnimating : State
    {
        public ArrestAnimating(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.ARRESTED;
        }

        public override void OnEnter()
        {
            ReachPoliceCar();
        }

        public override void OnExit()
        {

        }

        public override void OnTriggerEnter(Collider other)
        {


        }

        private void ReachPoliceCar()
        {
            minion.MeshTransform.LeanScale(Vector3.zero, 0.2f).setOnComplete(() =>
            {
                Minion.Truck.LoseMinion(minion);
            });
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
        }
    }
}