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

using UnityEngine;
using System.Collections;
using System;
using NSubstitute.Core;
using NSubstitute.Proxies;

namespace SubstituteExtensions {
  /// This class is used as the ProxyFactory when using SubstituteComponent.For
  /// Adds the function GenerateComponentProxy to ProxyFactory.
  public class ComponentProxyFactory : ProxyFactory {
    ComponentDynamicProxyFactory componentFactory;

    public ComponentProxyFactory(IProxyFactory delegateFactory, ComponentDynamicProxyFactory dynamicProxyFactory)
      : base(delegateFactory, dynamicProxyFactory) {
      componentFactory = dynamicProxyFactory;
    }

    public object GenerateComponentProxy(ICallRouter callRouter, Type type, GameObject gameObject) {
      return componentFactory.GenerateComponentProxy(callRouter, type, gameObject);
    }
  }
}
