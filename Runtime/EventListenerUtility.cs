using UnityEngine;
using UnityEngine.Events;

namespace NRVS.Events
{
    /// <summary>
    /// Listens to a EventBehavior and triggers a UnityEvent.
    /// </summary>
    public class EventListenerUtility : MonoBehaviour
    {
        [field: SerializeField]
        public EventBehavior EventBehavior { get; private set; }

        [SerializeField] 
        public UnityEvent onEventInvoked;

        private bool _registered;

        private void OnEnable()
        {
            if (EventBehavior == null)
            {
                Debug.LogWarning($"{nameof(EventListenerUtility)}: No EventBehavior assigned on {name}.");
                return;
            }

            if (Ref.TryGet<EventManager>(out var eventManager))
            {
                eventManager.Register(EventBehavior, onEventInvoked);
                _registered = true;
            }
            else
            {
                Debug.LogWarning($"Event Listener Utility: No Event Manager found to register listener for event {EventBehavior.name}");
            }
        }

        private void OnDisable()
        {
            if (_registered && Ref.TryGet<EventManager>(out var eventManager) && EventBehavior != null)
            {
                eventManager.Unregister(EventBehavior, onEventInvoked);
                _registered = false;
            }
        }
    }

    /// <summary>
    /// Listens to a EventBehavior<T> and triggers a UnityEvent<T>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EventListenerUtility<T> : MonoBehaviour
    {
        [field: SerializeField, Tooltip("The event behavior to listen for")]
        public EventBehavior<T> eventBehavior { get; private set; }

        [SerializeField]
        public UnityEvent<T> onEventInvoked;

        private bool _registered;

        private void OnEnable()
        {
            if (eventBehavior == null)
            {
                Debug.LogWarning($"{nameof(EventListenerUtility<T>)}: No EventBehavior<{typeof(T).Name}> assigned on {name}.");
                return;
            }

            if (Ref.TryGet<EventManager>(out var eventManager))
            {
                eventManager.Register(eventBehavior, onEventInvoked);
                _registered = true;
            }
            else
            {
                Debug.LogWarning($"Event Listener Utility<{typeof(T).Name}>: No Event Manager found to register listener for event {eventBehavior.name}");
            }
        }

        private void OnDisable()
        {
            if (_registered && Ref.TryGet<EventManager>(out var eventManager) && eventBehavior != null)
            {
                eventManager.Unregister(eventBehavior, onEventInvoked);
                _registered = false;
            }
        }
    }
}
