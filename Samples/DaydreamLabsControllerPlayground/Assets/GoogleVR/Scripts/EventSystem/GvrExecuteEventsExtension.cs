using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// This script extends the standard Unity EventSystem events with Gvr specific events.
public static class GvrExecuteEventsExtension {
  private static readonly ExecuteEvents.EventFunction<IGvrPointerHoverHandler> s_HoverHandler = Execute;

  private static void Execute(IGvrPointerHoverHandler handler, BaseEventData eventData) {
    handler.OnGvrPointerHover(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
  }

  public static ExecuteEvents.EventFunction<IGvrPointerHoverHandler> pointerHoverHandler {
    get { return s_HoverHandler; }
  }
}
