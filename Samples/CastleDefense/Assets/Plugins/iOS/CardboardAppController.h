// Copyright 2015 Google Inc. All rights reserved.
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

#import "UnityAppController.h"
#import "UnityAppController+Rendering.h"
#import "UnityAppController+ViewHandling.h"

#import "UnityAppController+UnityInterface.h"
#include "PluginBase/UnityViewControllerListener.h"

@interface CardboardAppController : UnityAppController<UnityViewControllerListener>

- (UnityView *)createUnityView;

- (UIViewController *)unityViewController;

// Override this method in a subclass to hook your own finishActivityAndReturn
// functionality.
- (void)finishActivityAndReturn:(BOOL)backTo2D;

@end
