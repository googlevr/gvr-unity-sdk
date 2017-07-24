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
using System.Reflection;
using System.Security.Permissions;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;
using NSubstitute.Core;
using NSubstitute.Exceptions;
using NSubstitute.Proxies.CastleDynamicProxy;
using UnityEngine;

namespace SubstituteExtensions {
  /// This class is used to generate proxy instances when using SubstituteComponent.For
  ///
  /// It is a modified version of CastleDynamicProxyFactory found here:
  /// https://bitbucket.org/Unity-Technologies/nsubstitute/src/360e7eb8d942b7c0002f567baf46a970e5153da9/Source/NSubstitute/Proxies/CastleDynamicProxy/CastleDynamicProxyFactory.cs?at=1.7.2-unity&fileviewer=file-view-default
  ///
  /// This version adds the method GenerateComponentProxy which is used to add
  /// a proxy component to a Unity GameObject.
  ///
  /// It copies CastleDynamicProxyFactory instead of inheriting from it
  /// because _proxyGeneratorand _allMethodsExceptCallRouterCallsHook
  /// would not be accessible from a subclass. Those variables are needed by GenerateComponentProxy.
  public class ComponentDynamicProxyFactory : IProxyFactory {
    readonly ProxyGenerator _proxyGenerator;
    readonly AllMethodsExceptCallRouterCallsHook _allMethodsExceptCallRouterCallsHook;

    public ComponentDynamicProxyFactory() {
      ConfigureDynamicProxyToAvoidReplicatingProblematicAttributes();

      _proxyGenerator = new ProxyGenerator();
      _allMethodsExceptCallRouterCallsHook = new AllMethodsExceptCallRouterCallsHook();
    }

    public object GenerateProxy(ICallRouter callRouter, Type typeToProxy,
      Type[] additionalInterfaces, object[] constructorArguments) {
      VerifyClassHasNotBeenPassedAsAnAdditionalInterface(additionalInterfaces);

      var interceptor = new CastleForwardingInterceptor(new CastleInvocationMapper(), callRouter);
      var proxyGenerationOptions = GetOptionsToMixinCallRouter(callRouter);
      var proxy = CreateProxyUsingCastleProxyGenerator(typeToProxy, additionalInterfaces,
        constructorArguments, interceptor, proxyGenerationOptions);
      interceptor.StartIntercepting();
      return proxy;
    }

    public object GenerateComponentProxy(ICallRouter callRouter, Type type, GameObject gameObject) {
      var interceptor = new CastleForwardingInterceptor(new CastleInvocationMapper(), callRouter);
      var proxyGenerationOptions = GetOptionsToMixinCallRouter(callRouter);

      // We can't instantiate the proxy using _proxyGenerator.CreateClassProxy
      // because components cannot be created using the 'new' operator. They must be added to a GameObject instead.
      // So instead, we use reflection to generate the proxy type from the ProxyGenerator
      // and then add it to the GameObect ourselves.
      MethodInfo method = _proxyGenerator.GetType().GetMethod("CreateClassProxyType",
                            BindingFlags.Instance |
                            BindingFlags.NonPublic);

      object[] args = new object[]{ type, null, proxyGenerationOptions };
      Type proxyType = (Type)method.Invoke(_proxyGenerator, args);

      // Add the proxy component type fo the GameObject.
      var proxy = gameObject.AddComponent(proxyType);

      // We still need to call the constructor generated for the proxy type so that the proxy has access
      // to the interceptors and other proxy options.
      // Use reflection to generate the argument list for the proxy class.
      MethodInfo argsMethod = _proxyGenerator.GetType().GetMethod("BuildArgumentListForClassProxy",
                                BindingFlags.Instance |
                                BindingFlags.NonPublic);

      // Get the constructor arguments for the proxy type.
      args = new object[]{ proxyGenerationOptions, new IInterceptor[] { interceptor } };
      List<object> arguments = (List<object>)argsMethod.Invoke(_proxyGenerator, args);

      // Now, we need to use reflection to manually find the correct constructor
      // to call for the proxy type and call it.
      ConstructorInfo[] constructors = proxyType.GetConstructors();
      foreach (ConstructorInfo constructor in constructors) {
        if (constructor.GetParameters().Length == arguments.Count) {
          constructor.Invoke(proxy, arguments.ToArray());
        }
      }

      interceptor.StartIntercepting();

      return proxy;
    }

    private object CreateProxyUsingCastleProxyGenerator(Type typeToProxy,
      Type[] additionalInterfaces,
      object[] constructorArguments,
      IInterceptor interceptor,
      ProxyGenerationOptions proxyGenerationOptions) {
      if (typeToProxy.IsInterface) {
        VerifyNoConstructorArgumentsGivenForInterface(constructorArguments);
        return _proxyGenerator.CreateInterfaceProxyWithoutTarget(typeToProxy, additionalInterfaces,
          proxyGenerationOptions, interceptor);
      }
      return _proxyGenerator.CreateClassProxy(typeToProxy, additionalInterfaces,
        proxyGenerationOptions, constructorArguments, interceptor);
    }

    private ProxyGenerationOptions GetOptionsToMixinCallRouter(ICallRouter callRouter) {
      var options = new ProxyGenerationOptions(_allMethodsExceptCallRouterCallsHook);
      options.AddMixinInstance(callRouter);
      return options;
    }

    private class AllMethodsExceptCallRouterCallsHook : AllMethodsHook {
      public override bool ShouldInterceptMethod(Type type, MethodInfo methodInfo) {
        return IsNotCallRouterMethod(methodInfo)
        && IsNotBaseObjectMethod(methodInfo)
        && base.ShouldInterceptMethod(type, methodInfo);
      }

      private static bool IsNotCallRouterMethod(MethodInfo methodInfo) {
        return methodInfo.DeclaringType != typeof(ICallRouter);
      }

      private static bool IsNotBaseObjectMethod(MethodInfo methodInfo) {
        return methodInfo.GetBaseDefinition().DeclaringType != typeof(object);
      }
    }

    private void VerifyNoConstructorArgumentsGivenForInterface(object[] constructorArguments) {
      if (constructorArguments != null && constructorArguments.Length > 0) {
        throw new SubstituteException("Can not provide constructor arguments" +
          " when substituting for an interface.");
      }
    }

    private void VerifyClassHasNotBeenPassedAsAnAdditionalInterface(Type[] additionalInterfaces) {
      if (additionalInterfaces != null && additionalInterfaces.Any(x => x.IsClass)) {
        throw new SubstituteException("Can not substitute for multiple classes." +
          " To substitute for multiple types " +
          "only one type can be a concrete class; other types can only be interfaces.");
      }
    }

    private static void ConfigureDynamicProxyToAvoidReplicatingProblematicAttributes() {
#pragma warning disable 618
      AttributesToAvoidReplicating.Add<SecurityPermissionAttribute>();
#pragma warning restore 618
      AttributesToAvoidReplicating.Add<ReflectionPermissionAttribute>();
      AttributesToAvoidReplicating.Add<PermissionSetAttribute>();
      AttributesToAvoidReplicating.Add<System.Runtime.InteropServices.MarshalAsAttribute>();
#if (NET4 || NET45)
      AttributesToAvoidReplicating.Add<System.Runtime.InteropServices.TypeIdentifierAttribute>();
#endif
      AttributesToAvoidReplicating.Add<UIPermissionAttribute>();
    }
  }
}