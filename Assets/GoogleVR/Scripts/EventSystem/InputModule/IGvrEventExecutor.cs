//-----------------------------------------------------------------------
// <copyright file="IGvrEventExecutor.cs" company="Google Inc.">
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

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Provides an interface for executing events for _IEventSystemHandler_.
/// </summary>
public interface IGvrEventExecutor
{
    /// <summary>Execute the event of type T : IEventSystemHandler on the game object.</summary>
    /// <remarks>The event will be executed on all components on the game
    /// object that can handle it.
    /// </remarks>
    /// <param name="target">Target game object.</param>
    /// <param name="eventData">Data associated with the executing event.</param>
    /// <param name="functor">Function to execute on the game object components.</param>
    bool Execute<T>(GameObject target,
                    BaseEventData eventData,
                    ExecuteEvents.EventFunction<T> functor)
    where T : IEventSystemHandler;

    /// <summary>Recurse up the hierarchy calling Execute<T>; until
    /// there is a game object that can handle the event.
    /// </summary>
    /// <remarks>See https://docs.unity3d.com/2017.4/Documentation/ScriptReference/EventSystems.ExecuteEvents.ExecuteHierarchy.html
    /// </remarks>
    /// <param name="root">Start game object for search.</param>
    /// <param name="eventData">Data associated with the executing event.</param>
    /// <param name="callbackFunction">Function to execute on the game object components.</param>
    /// <returns>GameObject Game object that handled the event.</returns>
    GameObject ExecuteHierarchy<T>(GameObject root,
                                   BaseEventData eventData,
                                   ExecuteEvents.EventFunction<T> callbackFunction)
    where T : IEventSystemHandler;

    /// <summary>Traverse the object hierarchy starting at root, and return the
    /// game object which implements the event handler of type T.</summary>
    /// @note Traversal is performed upwards from the target object, not down.
    GameObject GetEventHandler<T>(GameObject root)
    where T : IEventSystemHandler;
}
