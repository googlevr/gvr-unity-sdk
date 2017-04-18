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
using System.Collections.Generic;

/// Generic implementation for an object pool as described
/// at https://en.wikipedia.org/wiki/Object_pool_pattern.
/// Can be used to pool any class with a default constructor.
///
/// If you need to do more than just call a default constructor when
/// allocating an object of type T or returning an object to the pool,
/// then create a subclass of ObjectPool to specialize it for a particular
/// type of object. See _GameObjectPool_ as an example.
public class ObjectPool<T> : IObjectPool where T : class, new() {
  protected Stack<T> pool;
  protected int capacity;

  public int NumAllocatedObjects {
    get {
      return pool.Count;
    }
  }

  public bool IsPoolEmpty {
    get {
      return pool.Count == 0;
    }
  }

  public bool IsPoolFull {
    get {
      return pool.Count == capacity;
    }
  }

  protected ObjectPool() {
  }

  public ObjectPool(int capacity) : this(capacity, 0) {
  }

  public ObjectPool(int capacity, int preAllocateAmount) {
    Initialize(capacity, preAllocateAmount);
  }

  public T Borrow() {
    if (IsPoolEmpty) {
      return AllocateObject();
    }

    T obj = pool.Pop();
    OnBorrowed(obj);

    return obj;
  }

  public void Return(T obj) {
    // Don't return object if pool is already full.
    if (IsPoolFull) {
      OnUnableToReturn(obj);
      return;
    }

    pool.Push(obj);
    OnPooled(obj);
  }

  public void Clear() {
    pool.Clear();
  }

  public void Allocate(int amount) {
    int counter = 0;
    while (counter < amount && !IsPoolFull) {
      AddObject();
    }
  }

  public virtual void Dispose() {
  }

  protected void Initialize(int capacity, int preAllocateAmount) {
    if (capacity < 1) {
      Debug.LogWarning("Capacity must be at least 1.");
      capacity = 1;
    }

    pool = new Stack<T>(capacity);
    this.capacity = capacity;

    if (preAllocateAmount > capacity) {
      Debug.LogWarning("preAllocateAmount cannot be higher than capacity.");
      preAllocateAmount = capacity;
    }

    Allocate(preAllocateAmount);
  }

  protected virtual void OnBorrowed(T borrowedObject) {
  }

  protected virtual void OnPooled(T returnedObject) {
  }

  protected virtual void OnUnableToReturn(T returnedObject) {
  }

  protected virtual T AllocateObject() {
    return new T();
  }

  private void AddObject() {
    if (IsPoolFull) {
      Debug.LogWarning("Cannot addObject, pool is already full.");
      return;
    }

    T obj = AllocateObject();

    pool.Push(obj);
    OnPooled(obj);
  }
}
