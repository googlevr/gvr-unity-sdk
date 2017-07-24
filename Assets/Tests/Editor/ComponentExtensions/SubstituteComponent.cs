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

using System;
using UnityEngine;
using System.Collections;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.Proxies;
using NSubstitute.Proxies.DelegateProxy;
using NSubstitute.Routing;
using SubstituteExtensions;

/// Extends Substitute so that it can be used for generating mocks of Monobehaviours.
/// Normally, this is not possible because a Monobehaviour cannot be created using the 'new' keyword.
///
/// If the version of Substitute that Unity uses is upgraded, the internal implementation for this may need
/// to change. Currently, Unity hasn't changed Substitute since 2014.
public static class SubstituteComponent {
  private static ISubstitutionContext componentSubstitutionContext;
  private static ISubstitutionContext originalSubstitutionContext;

  /// Switches the SubstitutionContext to use the custom substitution context that supports monobehaviours.
  /// The same context MUST be used for the entirety of a test,
  /// so this should be called once at the start of the Setup function.
  public static void BeginComponentContext() {
    if (SubstitutionContext.Current == componentSubstitutionContext) {
      Debug.LogError("Cannot call BeginComponentContext. It has already begun.");
      return;
    }

    CreateContext();

    originalSubstitutionContext = SubstitutionContext.Current;
    SubstitutionContext.Current = componentSubstitutionContext;
  }

  /// Restores the original substitution context.
  /// Should be called in Teardown for any tests that have called BeginComponentContext
  public static void EndComponentContext() {
    if (SubstitutionContext.Current != componentSubstitutionContext) {
      Debug.LogError("Cannot call EndComponentContext before calling BeginComponentContext.");
      return;
    }

    SubstitutionContext.Current = originalSubstitutionContext;
  }

  /// Returns true if the current SubstitutionContext is set to the custom substitution context.
  public static bool IsUsingComponentContext() {
    return componentSubstitutionContext != null && SubstitutionContext.Current == componentSubstitutionContext;
  }

  /// Substitute a class that extends from Monobehaviour and add it to the GameObject passed in.
  /// Remember, all non-virtual members will actually be executed. Only virtual/abstract members
  /// can be recorded or have return values specified.
  public static T For<T>(GameObject gameObject)
    where T : MonoBehaviour {
    if (!IsUsingComponentContext()) {
      Debug.LogError("Cannot Substitute Component, BeginComponentContext must be called first.");
    }

    System.Type type = typeof(T);
    ISubstituteFactory substituteFactory = SubstitutionContext.Current.SubstituteFactory;
    ComponentSubstituteFactory componentFactory = substituteFactory as ComponentSubstituteFactory;
    object result = componentFactory.CreateComponent(type, gameObject);

    return (T)result;
  }

  /// Substitute a class that extends from Monobehaviour and add it to the GameObject passed in.
  /// Remember, all non-virtual members will actually be executed. Only virtual/abstract members
  /// can be recorded or have return values specified.
  /// Additionally, call OnEnable and Awake on the component after it is added to the GameObject.
  public static T ForInvokeLifecycle<T>(GameObject gameObject)
    where T : MonoBehaviour {
    T result = SubstituteComponent.For<T>(gameObject);

    if (result == null) {
      return null;
    }

    ComponentHelpers.CallOnEnable(gameObject);
    ComponentHelpers.CallAwake(gameObject);

    return result;
  }

  private static void CreateContext() {
    if (componentSubstitutionContext != null) {
      return;
    }

    var callRouterFactory = new CallRouterFactory();
    var dynamicProxyFactory = new ComponentDynamicProxyFactory();
    var delegateFactory = new DelegateProxyFactory();
    var proxyFactory = new ComponentProxyFactory(delegateFactory, dynamicProxyFactory);
    var callRouteResolver = new CallRouterResolver();
    var substituteFactory = new ComponentSubstituteFactory(callRouterFactory, proxyFactory, callRouteResolver);
    componentSubstitutionContext = new SubstitutionContext(substituteFactory);
    substituteFactory._context = componentSubstitutionContext;
  }
}
