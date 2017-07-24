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
using System;
using System.Collections;

/// Interface for a pool of objects used by ObjectPoolManager to manage a
/// collection of Object Pools. Description of Object Pools design pattern is
/// described at https://en.wikipedia.org/wiki/Object_pool_pattern.
public interface IObjectPool : IDisposable {
  /// The numver of objects that are currently allocated in the pool.
  int NumAllocatedObjects { get; }

  /// Returns true if the pool currently has no objects in it.
  bool IsPoolEmpty { get; }

  /// Returns true if the NumAllocatedObjects is equal to the capaciy of the pool.
  bool IsPoolFull { get; }

  /// Clears all of the allocated objects from the pool.
  void Clear();

  /// Allocates amount objects in the pool.
  void Allocate(int amount);
}
