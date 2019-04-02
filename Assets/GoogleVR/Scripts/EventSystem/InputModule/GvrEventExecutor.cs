//-----------------------------------------------------------------------
// <copyright file="GvrEventExecutor.cs" company="Google Inc.">
// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Gvr.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Wraps `UnityEngine.EventSystems.ExecuteEvents`.</summary>
/// <remarks>Also, exposes event delegates to allow global handling of events.</remarks>
public class GvrEventExecutor : IGvrEventExecutor
{
    /// <summary>Stores delegates for events.</summary>
    private Dictionary<Type, EventDelegate> eventTable;

    /// <summary>Initializes a new instance of the <see cref="GvrEventExecutor" /> class.</summary>
    public GvrEventExecutor()
    {
        eventTable = new Dictionary<Type, EventDelegate>();
    }

    /// <summary>Delegate type for handling pointer events.</summary>
    /// <param name="target">The GameObject to add this behavior to.</param>
    /// <param name="eventData">The context data of the Event which triggered this call.</param>
    public delegate void EventDelegate(GameObject target, PointerEventData eventData);

    /// <summary>Adds or removes delegate functions for the `OnPointerClick` event.</summary>
    public event EventDelegate OnPointerClick
    {
        add { AddEventDelegate<IPointerClickHandler>(value); }
        remove { RemoveEventDelegate<IPointerClickHandler>(value); }
    }

    /// <summary>Adds or removes delegate functions for the `OnPointerDown` event.</summary>
    public event EventDelegate OnPointerDown
    {
        add { AddEventDelegate<IPointerDownHandler>(value); }
        remove { RemoveEventDelegate<IPointerDownHandler>(value); }
    }

    /// <summary>Adds or removes delegate functions for the `OnPointerUp` event.</summary>
    public event EventDelegate OnPointerUp
    {
        add { AddEventDelegate<IPointerUpHandler>(value); }
        remove { RemoveEventDelegate<IPointerUpHandler>(value); }
    }

    /// <summary>Adds or removes delegate functions for the `OnPointerEnter` event.</summary>
    public event EventDelegate OnPointerEnter
    {
        add { AddEventDelegate<IPointerEnterHandler>(value); }
        remove { RemoveEventDelegate<IPointerEnterHandler>(value); }
    }

    /// <summary>Adds or removes delegate functions for the `OnPointerExit` event.</summary>
    public event EventDelegate OnPointerExit
    {
        add { AddEventDelegate<IPointerExitHandler>(value); }
        remove { RemoveEventDelegate<IPointerExitHandler>(value); }
    }

    /// <summary>Adds or removes delegate functions for the `OnScroll` event.</summary>
    public event EventDelegate OnScroll
    {
        add { AddEventDelegate<IScrollHandler>(value); }
        remove { RemoveEventDelegate<IScrollHandler>(value); }
    }

    /// <summary>Execute the specified target, eventData and functor.</summary>
    /// <param name="target">The `GameObject` to execute this event behavior on.</param>
    /// <param name="eventData">The triggering EventData.</param>
    /// <param name="functor">The delegate call's implementation.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    /// <returns>Returns `true` if the method successfully executes, `false` otherwise.</returns>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public bool Execute<T>(GameObject target,
                           BaseEventData eventData,
                           ExecuteEvents.EventFunction<T> functor)
    where T : IEventSystemHandler
    {
        bool result = ExecuteEvents.Execute<T>(target, eventData, functor);
        CallEventDelegate<T>(target, eventData);

        return result;
    }

    /// <summary>Executes the hierarchy.</summary>
    /// <returns>The hierarchy.</returns>
    /// <param name="root">The top-level object this event should trigger.</param>
    /// <param name="eventData">The triggering EventData.</param>
    /// <param name="callbackFunction">Callback function.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public GameObject ExecuteHierarchy<T>(GameObject root,
                                          BaseEventData eventData,
                                          ExecuteEvents.EventFunction<T> callbackFunction)
    where T : IEventSystemHandler
    {
        GameObject result = ExecuteEvents.ExecuteHierarchy<T>(root, eventData, callbackFunction);
        CallEventDelegate<T>(root, eventData);

        return result;
    }

    /// <summary>Gets the event handler.</summary>
    /// <returns>The event handler.</returns>
    /// <param name="root">The top-level object this event should trigger.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public GameObject GetEventHandler<T>(GameObject root)
    where T : IEventSystemHandler
    {
        return ExecuteEvents.GetEventHandler<T>(root);
    }

    private void CallEventDelegate<T>(GameObject target, BaseEventData eventData)
    where T : IEventSystemHandler
    {
        Type type = typeof(T);

        EventDelegate eventDelegate;
        if (eventTable.TryGetValue(type, out eventDelegate))
        {
            PointerEventData pointerEventData = eventData as PointerEventData;
            if (pointerEventData == null)
            {
                Debug.LogError("Event data must be PointerEventData.");
                return;
            }

            eventDelegate(target, pointerEventData);
        }
    }

    private void AddEventDelegate<T>(EventDelegate eventDelegate)
    {
        Type type = typeof(T);

        EventDelegate existingDelegate;
        if (eventTable.TryGetValue(type, out existingDelegate))
        {
            eventTable[type] = existingDelegate + eventDelegate;
        }
        else
        {
            eventTable[type] = eventDelegate;
        }
    }

    private void RemoveEventDelegate<T>(EventDelegate eventDelegate)
    {
        Type type = typeof(T);

        EventDelegate existingDelegate;
        if (!eventTable.TryGetValue(type, out existingDelegate))
        {
            return;
        }

        eventDelegate = existingDelegate - eventDelegate;
        if (eventDelegate != null)
        {
            eventTable[type] = eventDelegate;
        }
        else
        {
            eventTable.Remove(type);
        }
    }
}
