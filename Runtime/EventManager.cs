using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NRVS.Events
{
    /// <summary>
    /// Central hub that registers listeners per EventBehavior and relays invocations.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        // Non-generic (void) listeners per EventBehavior
        private readonly Dictionary<EventBehavior, List<UnityEvent>> _voidListeners = new(64);

        // Generic listeners per EventBehavior<T>; value is List<UnityEvent<T>> stored as object
        private readonly Dictionary<EventBehavior, object> _typedListeners = new(64);

        void Awake() => Ref.Register<EventManager>(this);
        void OnDestroy() => Ref.Unregister<EventManager>();

        #region Registration (void)
        public void Register(EventBehavior behavior, UnityEvent unityEvent)
        {
            if (!behavior || unityEvent == null) return;

            if (!_voidListeners.TryGetValue(behavior, out var list))
            {
                list = new List<UnityEvent>(4);
                _voidListeners[behavior] = list;
            }

            if (!list.Contains(unityEvent))
                list.Add(unityEvent);
        }

        public void Unregister(EventBehavior behavior, UnityEvent unityEvent)
        {
            if (!behavior || unityEvent == null) return;
            if (_voidListeners.TryGetValue(behavior, out var list))
            {
                list.Remove(unityEvent);
                if (list.Count == 0) _voidListeners.Remove(behavior);
            }
        }
        #endregion

        #region Registration (generic)
        public void Register<T>(EventBehavior<T> behavior, UnityEvent<T> unityEvent)
        {
            if (!behavior || unityEvent == null) return;

            if (!_typedListeners.TryGetValue(behavior, out var boxed))
            {
                var newList = new List<UnityEvent<T>>(4);
                newList.Add(unityEvent);
                _typedListeners[behavior] = newList; // store strongly-typed list as object
                return;
            }

            var list = boxed as List<UnityEvent<T>>;
            if (list != null)
            {
                if (!list.Contains(unityEvent))
                    list.Add(unityEvent);
            }
            else
            {
                Debug.LogError($"EventManager: Type mismatch for event '{behavior.name}'. " +
                               $"Attempted to register UnityEvent<{typeof(T).Name}> on an existing different-typed list.");
            }
        }

        public void Unregister<T>(EventBehavior<T> behavior, UnityEvent<T> unityEvent)
        {
            if (!behavior || unityEvent == null) return;

            if (_typedListeners.TryGetValue(behavior, out var boxed))
            {
                var list = boxed as List<UnityEvent<T>>;
                if (list != null)
                {
                    list.Remove(unityEvent);
                    if (list.Count == 0) _typedListeners.Remove(behavior);
                }
            }
        }
        #endregion

        #region Invoke
        public void Invoke(EventBehavior eventBehavior)
        {
            if (!eventBehavior) return;

            if (_voidListeners.TryGetValue(eventBehavior, out var list))
            {
                // Iterate forward; compact nulls without extra allocs.
                for (int i = 0; i < list.Count; /* i++ inside */)
                {
                    var ev = list[i];
                    if (ev == null)
                    {
                        list.RemoveAt(i);
                        continue;
                    }
                    try { ev.Invoke(); }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    i++;
                }
                if (list.Count == 0) _voidListeners.Remove(eventBehavior);
            }
        }

        public void Invoke<T>(EventBehavior<T> eventBehavior, T value)
        {
            if (!eventBehavior) return;

            if (_typedListeners.TryGetValue(eventBehavior, out var boxed))
            {
                var list = boxed as List<UnityEvent<T>>;
                if (list == null)
                {
                    Debug.LogError($"EventManager: Type mismatch on invoke for '{eventBehavior.name}' " +
                                   $"with payload type {typeof(T).Name}.");
                    return;
                }

                for (int i = 0; i < list.Count; /* i++ inside */)
                {
                    var ev = list[i];
                    if (ev == null)
                    {
                        list.RemoveAt(i);
                        continue;
                    }
                    try { ev.Invoke(value); }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    i++;
                }
                if (list.Count == 0) _typedListeners.Remove(eventBehavior);
            }
        }
        #endregion
    }
}
