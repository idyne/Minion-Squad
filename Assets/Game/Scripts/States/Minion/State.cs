using UnityEngine;
namespace States.MinionState
{
    public abstract class State
    {
        protected string name;
        protected Minion minion;

        public string Name { get => name; }

        public State(Minion minion, string name)
        {
            this.minion = minion;
            this.name = name;
        }
        public abstract void OnEnter();
        public abstract void Update();
        public abstract void OnExit();
        public abstract void OnTriggerEnter(Collider other);
        public abstract bool CanEnter();

        public abstract void OnTriggerExit(Collider other);
    }
}