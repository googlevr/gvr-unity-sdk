// Copyright 2016 Google Inc. All rights reserved.
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

#if UNITY_ANDROID || UNITY_EDITOR
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace GvrPointerTests {

  [TestFixture]
  internal class GvrPointerInputModuleTest {
    private GvrPointerInputModuleImpl module;

    private GvrBasePointer pointer;
    private GvrPointerScrollInput scrollInput;
    private EventSystem eventSytem;
    private IGvrInputModuleController moduleController;
    private IGvrEventExecutor eventExecutor;
    private List<RaycastResult> resultCache;

    private RaycastResult raycastResult;
    private GameObject hitObject;
    private Vector3 hitPos;

    private RaycastResult dummyRaycastResult;

    [Flags]
    private enum EventFlags {
      None = 0,
      Enter = 1,
      Exit = 1 << 1,
      Hover = 1 << 2,
      Down = 1 << 3,
      Up = 1 << 4,
      Click = 1 << 5,
      InitializeDrag = 1 << 6,
      BeginDrag = 1 << 7,
      Drag = 1 << 8,
      EndDrag = 1 << 9,
      Scroll = 1 << 10
    }

    private struct VerifyEventsArgs {
      public EventFlags eventFlags;
      public bool isPointerInteractive;
    }

    [SetUp]
    public void Setup() {
      SubstituteComponent.BeginComponentContext();

      // Create input module implementation.
      module = new GvrPointerInputModuleImpl();

      // Create Event System.
      GameObject eventSystemObj = new GameObject("EventSystem");
      eventSytem = eventSystemObj.AddComponent<EventSystem>();

      // Create mock pointer.
      pointer = SubstituteComponent.For<GvrBasePointer>(eventSystemObj);
      pointer.PointerTransform.Returns(eventSytem.transform);
      pointer.IsAvailable.Returns(true);
      module.Pointer = pointer;

      // Create mock scroll input.
      scrollInput = new GvrPointerScrollInput();
      module.ScrollInput = scrollInput;

      // Create mock module controller.
      moduleController = Substitute.For<IGvrInputModuleController>();
      moduleController.eventSystem.Returns(eventSytem);
      resultCache = new List<RaycastResult>();
      moduleController.RaycastResultCache.Returns(resultCache);
      module.ModuleController = moduleController;

      // Create mock event executor.
      eventExecutor = Substitute.For<IGvrEventExecutor>();
      module.EventExecutor = eventExecutor;

      // Create dummy objects to use for hit detection.
      raycastResult = new RaycastResult();
      hitObject = new GameObject("HitObject");
      hitPos = new Vector3(1.0f, 2.0f, 3.0f);

      dummyRaycastResult = new RaycastResult();
    }

    [TearDown]
    public void TearDown() {
      SubstituteComponent.EndComponentContext();
    }

    [Test]
    public void EnterHoverExit() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();

      ProcessMiss();

      verifyArgs.eventFlags = EventFlags.None;
      VerifyEvents(verifyArgs);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Enter;
      VerifyEvents(verifyArgs);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Hover;
      VerifyEvents(verifyArgs);

      ProcessHit();

      VerifyEvents(verifyArgs);

      ProcessMiss();

      verifyArgs.eventFlags = EventFlags.Exit;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void EnterDestroy() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Enter;
      VerifyEvents(verifyArgs);

      GameObject.DestroyImmediate(hitObject);
      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Exit;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void Interactive() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();
      verifyArgs.isPointerInteractive = true;

      eventExecutor.GetEventHandler<IPointerClickHandler>(hitObject).Returns(hitObject);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Enter;
      VerifyEvents(verifyArgs);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Hover;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void Click() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();
      verifyArgs.isPointerInteractive = true;

      pointer.TriggerDown.Returns(true);
      pointer.Triggering.Returns(true);
      eventExecutor.GetEventHandler<IPointerClickHandler>(hitObject).Returns(hitObject);
      eventExecutor.ExecuteHierarchy<IPointerClickHandler>(hitObject,
        module.CurrentEventData, ExecuteEvents.pointerClickHandler).Returns(hitObject);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Down | EventFlags.Enter;
      VerifyEvents(verifyArgs);

      pointer.TriggerDown.Returns(false);
      pointer.Triggering.Returns(false);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Up | EventFlags.Click | EventFlags.Hover;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void Up() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();
      verifyArgs.isPointerInteractive = true;

      pointer.TriggerDown.Returns(true);
      pointer.Triggering.Returns(true);
      eventExecutor.GetEventHandler<IPointerClickHandler>(hitObject).Returns(hitObject);
      eventExecutor.ExecuteHierarchy<IPointerClickHandler>(hitObject,
        module.CurrentEventData, ExecuteEvents.pointerClickHandler).Returns(hitObject);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Down | EventFlags.Enter;
      VerifyEvents(verifyArgs);

      pointer.TriggerDown.Returns(false);
      pointer.Triggering.Returns(false);

      ProcessMiss();

      verifyArgs.eventFlags = EventFlags.Up | EventFlags.Exit;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void Drag() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();
      verifyArgs.isPointerInteractive = true;

      pointer.TriggerDown.Returns(true);
      pointer.Triggering.Returns(true);
      eventExecutor.GetEventHandler<IDragHandler>(hitObject).Returns(hitObject);
      eventExecutor.GetEventHandler<IPointerClickHandler>(hitObject).Returns(hitObject);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.InitializeDrag | EventFlags.Enter | EventFlags.Down;
      VerifyEvents(verifyArgs);

      pointer.TriggerDown.Returns(false);

      eventSytem.transform.Rotate(new Vector3(0.0f, 30.0f, 0.0f));
      raycastResult.worldPosition = hitPos + new Vector3(0.0f, 1000.0f, 0.0f);
      raycastResult.screenPosition = new Vector3(0.0f, 500.0f, 0.0f);
      moduleController.FindFirstRaycast(resultCache).Returns(raycastResult);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.BeginDrag | EventFlags.Drag | EventFlags.Hover;
      VerifyEvents(verifyArgs);

      eventSytem.transform.Rotate(new Vector3(0.0f, 30.0f, 0.0f));

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Drag | EventFlags.Hover;
      VerifyEvents(verifyArgs);

      pointer.Triggering.Returns(false);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.EndDrag | EventFlags.Up | EventFlags.Hover | EventFlags.Click;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void Scroll() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();

      // Test starting to touch the object
      pointer.TouchDown.Returns(true);
      pointer.IsTouching.Returns(true);
      eventExecutor.GetEventHandler<IScrollHandler>(hitObject).Returns(hitObject);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Enter;
      VerifyEvents(verifyArgs);


      // Test starting to scroll the object.
      pointer.TouchDown.Returns(false);
      pointer.TouchPos.Returns(new Vector2(0.5f, 0.5f));

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Hover | EventFlags.Scroll;
      VerifyEvents(verifyArgs);

      // Test that scrolling continues after release due to inertia.
      pointer.TouchUp.Returns(true);
      pointer.IsTouching.Returns(false);

      ProcessMiss();

      verifyArgs.eventFlags = EventFlags.Exit | EventFlags.Scroll;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void ScrollNoInertia() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();

      scrollInput.inertia = false;

      // Test starting to touch the object
      pointer.TouchDown.Returns(true);
      pointer.IsTouching.Returns(true);
      eventExecutor.GetEventHandler<IScrollHandler>(hitObject).Returns(hitObject);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Enter;
      VerifyEvents(verifyArgs);


      // Test starting to scroll the object.
      pointer.TouchDown.Returns(false);
      pointer.TouchPos.Returns(new Vector2(0.5f, 0.5f));

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Hover | EventFlags.Scroll;
      VerifyEvents(verifyArgs);

      // Test that scrolling stops after release because inertia is off.
      pointer.TouchUp.Returns(true);
      pointer.IsTouching.Returns(false);

      ProcessMiss();

      verifyArgs.eventFlags = EventFlags.Exit;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void ScrollSettings() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();

      GvrScrollSettings scrollSettings = hitObject.AddComponent<GvrScrollSettings>();
      scrollSettings.inertiaOverride = false;

      // Test starting to touch the object
      pointer.TouchDown.Returns(true);
      pointer.IsTouching.Returns(true);
      eventExecutor.GetEventHandler<IScrollHandler>(hitObject).Returns(hitObject);

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Enter;
      VerifyEvents(verifyArgs);


      // Test starting to scroll the object.
      pointer.TouchDown.Returns(false);
      pointer.TouchPos.Returns(new Vector2(0.5f, 0.5f));

      ProcessHit();

      verifyArgs.eventFlags = EventFlags.Hover | EventFlags.Scroll;
      VerifyEvents(verifyArgs);

      // Test that scrolling stops after release because inertia is off.
      pointer.TouchUp.Returns(true);
      pointer.IsTouching.Returns(false);

      ProcessMiss();

      verifyArgs.eventFlags = EventFlags.Exit;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void ClearPointer() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();

      module.Pointer = null;

      VerifyEvents(verifyArgs);

      module.Pointer = pointer;

      ProcessHit();

      pointer.ClearReceivedCalls();
      eventExecutor.ClearReceivedCalls();

      module.Pointer = null;
      module.Process();

      verifyArgs.eventFlags = EventFlags.Exit;
      VerifyEvents(verifyArgs);
    }

    [Test]
    public void DisablePointer() {
      VerifyEventsArgs verifyArgs = new VerifyEventsArgs();

      ProcessHit();

      pointer.ClearReceivedCalls();
      eventExecutor.ClearReceivedCalls();

      pointer.IsAvailable.Returns(false);
      module.Process();

      verifyArgs.eventFlags = EventFlags.Exit;
      VerifyEvents(verifyArgs);
    }

    private void ProcessHit() {
      // Setup Raycast Result.
      raycastResult.gameObject = hitObject;
      raycastResult.worldPosition = hitPos;

      // Set Return.
      moduleController.FindFirstRaycast(resultCache).Returns(raycastResult);

      // Process.
      module.Process();
    }

    private void ProcessMiss() {
      // Setup Raycast Result.
      raycastResult.gameObject = null;
      raycastResult.worldPosition = Vector3.zero;

      // Set Return.
      moduleController.FindFirstRaycast(resultCache).Returns(raycastResult);

      // Process.
      module.Process();
    }

    private void VerifyEvents(VerifyEventsArgs args) {
      CheckEnter(args);
      CheckHover(args);
      CheckExit(args);
      CheckDown(args);
      CheckUp(args);
      CheckClick(args);
      CheckInitializeDrag(args);
      CheckBeginDrag(args);
      CheckDrag(args);
      CheckEndDrag(args);
      CheckScroll(args);

      pointer.ClearReceivedCalls();
      eventExecutor.ClearReceivedCalls();
    }

    private void CheckEnter(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.Enter);

      if (received) {
        pointer.Received(1).OnPointerEnter(raycastResult, args.isPointerInteractive);
      } else {
        pointer.DidNotReceiveWithAnyArgs().OnPointerEnter(dummyRaycastResult, false);
      }

      CheckExecuteEvent(ExecuteEvents.pointerEnterHandler, received, hitObject);
    }

    private void CheckHover(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.Hover);

      if (received) {
        pointer.Received(1).OnPointerHover(raycastResult, args.isPointerInteractive);
      } else {
        pointer.DidNotReceiveWithAnyArgs().OnPointerHover(dummyRaycastResult, false);
      }

      CheckExecuteHierarchyEvent(GvrExecuteEventsExtension.pointerHoverHandler, received, hitObject);
    }

    private void CheckExit(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.Exit);

      if (received) {
        pointer.Received(1).OnPointerExit(hitObject);
      } else {
        pointer.DidNotReceiveWithAnyArgs().OnPointerExit(null);
      }

      CheckExecuteEvent(ExecuteEvents.pointerExitHandler, received, hitObject);
    }

    private void CheckDown(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.Down);

      if (received) {
        pointer.Received(1).OnPointerClickDown();
      } else {
        pointer.DidNotReceiveWithAnyArgs().OnPointerClickDown();
      }

      CheckExecuteHierarchyEvent(ExecuteEvents.pointerDownHandler, received, hitObject);
    }

    private void CheckUp(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.Up);

      if (received) {
        pointer.Received(1).OnPointerClickUp();
      } else {
        pointer.DidNotReceiveWithAnyArgs().OnPointerClickUp();
      }

      CheckExecuteEvent(ExecuteEvents.pointerUpHandler, received, hitObject);
    }

    private void CheckClick(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.Click);
      CheckExecuteEvent(ExecuteEvents.pointerClickHandler, received, hitObject);
    }

    private void CheckInitializeDrag(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.InitializeDrag);
      CheckExecuteEvent(ExecuteEvents.initializePotentialDrag, received, hitObject);
    }

    private void CheckBeginDrag(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.BeginDrag);
      CheckExecuteEvent(ExecuteEvents.beginDragHandler, received, hitObject);
    }

    private void CheckDrag(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.Drag);
      CheckExecuteEvent(ExecuteEvents.dragHandler, received, hitObject);
    }

    private void CheckEndDrag(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.EndDrag);
      CheckExecuteEvent(ExecuteEvents.endDragHandler, received, hitObject);
    }

    private void CheckScroll(VerifyEventsArgs args) {
      bool received = CheckFlag(args.eventFlags, EventFlags.Scroll);
      CheckExecuteHierarchyEvent(ExecuteEvents.scrollHandler, received, hitObject);
    }

    private void CheckExecuteEvent<T>(ExecuteEvents.EventFunction<T> functor, bool received, GameObject target)
      where T : IEventSystemHandler {
      if (received) {
        eventExecutor.Received(1).Execute(target, module.CurrentEventData, functor);
      } else {
        eventExecutor.DidNotReceiveWithAnyArgs().Execute(null, null, functor);
      }
    }

    private void CheckExecuteHierarchyEvent<T>(ExecuteEvents.EventFunction<T> functor, bool received, GameObject target)
      where T : IEventSystemHandler {
      if (received) {
        eventExecutor.Received(1).ExecuteHierarchy(target, module.CurrentEventData, functor);
      } else {
        eventExecutor.DidNotReceiveWithAnyArgs().ExecuteHierarchy(null, null, functor);
      }
    }

    private bool CheckFlag(EventFlags flags, EventFlags evt) {
      return (flags & evt) == evt;
    }
  }
}
#endif  // UNITY_ANDROID || UNITY_EDITOR
