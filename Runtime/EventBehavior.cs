using UnityEngine;

namespace NRVS.Events
{
    [CreateAssetMenu(fileName = "Event Behavior_ New", menuName = "Events/Event Behavior")]
    public class EventBehavior : ScriptableObject
    {
        [Header("Developer Settings")]

        [SerializeField, TextArea] 
        string developerNote;

        [SerializeField] 
        protected bool debugInvokes;

        public virtual void Invoke()
        {
            if (debugInvokes)
                Debug.Log($"Event {name}: Invoked");

            if (Ref.TryGet<EventManager>(out var eventManager))
                eventManager.Invoke(this);
            else
                Debug.LogWarning($"Event Behavior: No Event Manager found to invoke event {name}");
        }
    }

    public abstract class EventBehavior<T> : EventBehavior
    {
        [Tooltip("Default value to use when invoking without a parameter")]
        public T defaultValue;

        public override void Invoke() => Invoke(defaultValue);

        public void Invoke(T value)
        {
            if (debugInvokes)
                Debug.Log($"Event {name}: Invoked with parameter {value}");

            if (Ref.TryGet<EventManager>(out var eventManager))
                eventManager.Invoke(this, value);
            else
                Debug.LogWarning($"Event Behavior: No Event Manager found to invoke event {name}");
        }
    }
}
