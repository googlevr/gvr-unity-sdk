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
using System.Linq;
using NSubstitute.Exceptions;
using NSubstitute.Core;
using UnityEngine;

namespace SubstituteExtensions {
  /// This class is the SubstituteFactory used when using SubstituteComponent.For
  ///
  /// It is a modified version of SubstituteFactory found here:
  /// https://bitbucket.org/Unity-Technologies/nsubstitute/src/360e7eb8d942b7c0002f567baf46a970e5153da9/Source/NSubstitute/Core/SubstituteFactory.cs?at=1.7.2-unity&fileviewer=file-view-default
  ///
  /// This version adds the method CreateComponent which is used
  /// to add a substitute component to a Unity GameObject.
  ///
  /// It copies SubstituteFactory instead of inheriting from it because _context must be set
  /// in the constructor in the origial version which is not possible to do when creating
  /// a custom SubstitutionContext.
  /// The SubstitutionContext also required the SubstituteFactory in the constructor,
  /// which creates a circular dependency.
  public class ComponentSubstituteFactory : ISubstituteFactory {
    public ISubstitutionContext _context;

    readonly ICallRouterFactory _callRouterFactory;
    readonly ComponentProxyFactory _proxyFactory;
    readonly ICallRouterResolver _callRouterResolver;

    public ComponentSubstituteFactory(ICallRouterFactory callRouterFactory,
      ComponentProxyFactory proxyFactory, ICallRouterResolver callRouterResolver) {
      _callRouterFactory = callRouterFactory;
      _proxyFactory = proxyFactory;
      _callRouterResolver = callRouterResolver;
    }

    /// <summary>
    /// Create a substitute for the given types.
    /// </summary>
    /// <param name="typesToProxy"></param>
    /// <param name="constructorArguments"></param>
    /// <returns></returns>
    public object Create(Type[] typesToProxy, object[] constructorArguments) {
      return Create(typesToProxy, constructorArguments, SubstituteConfig.OverrideAllCalls);
    }

    /// <summary>
    /// Create a substitute component for the given type and add it to the GameObject
    /// </summary>
    /// <param name="type"></param>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public object CreateComponent(Type type, GameObject gameObject) {
      var callRouter = _callRouterFactory.Create(_context, SubstituteConfig.OverrideAllCalls);
      var proxy = _proxyFactory.GenerateComponentProxy(callRouter, type, gameObject);
      _callRouterResolver.Register(proxy, callRouter);
      return proxy;
    }

    /// <summary>
    /// Create an instance of the given types, with calls configured to call the base implementation
    /// where possible. Parts of the instance can be substituted using
    /// <see cref="SubstituteExtensions.Returns{T}(T,T,T[])">Returns()</see>.
    /// </summary>
    /// <param name="typesToProxy"></param>
    /// <param name="constructorArguments"></param>
    /// <returns></returns>
    public object CreatePartial(Type[] typesToProxy, object[] constructorArguments) {
      var primaryProxyType = GetPrimaryProxyType(typesToProxy);
      if (primaryProxyType.IsSubclassOf(typeof(Delegate)) || !primaryProxyType.IsClass) {
        throw new CanNotPartiallySubForInterfaceOrDelegateException(primaryProxyType);
      }
      return Create(typesToProxy, constructorArguments, SubstituteConfig.CallBaseByDefault);
    }

    private object Create(Type[] typesToProxy, object[] constructorArguments,
      SubstituteConfig config) {
      var callRouter = _callRouterFactory.Create(_context, config);
      var primaryProxyType = GetPrimaryProxyType(typesToProxy);
      var additionalTypes = typesToProxy.Where(x => x != primaryProxyType).ToArray();
      var proxy = _proxyFactory.GenerateProxy(callRouter, primaryProxyType, additionalTypes,
        constructorArguments);
      _callRouterResolver.Register(proxy, callRouter);
      return proxy;
    }

    private Type GetPrimaryProxyType(Type[] typesToProxy) {
      if (typesToProxy.Any(x => x.IsSubclassOf(typeof(Delegate))))
        return typesToProxy.First(x => x.IsSubclassOf(typeof(Delegate)));
      if (typesToProxy.Any(x => x.IsClass))
        return typesToProxy.First(x => x.IsClass);
      return typesToProxy.First();
    }

    public ICallRouter GetCallRouterCreatedFor(object substitute) {
      return _callRouterResolver.ResolveFor(substitute);
    }
  }
}