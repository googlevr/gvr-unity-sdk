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

#import "GvrAudioAppController.h"

extern "C" {

// We have to manually register the Unity Audio Effect plugin.
struct UnityAudioEffectDefinition;
typedef int (*UnityPluginGetAudioEffectDefinitionsFunc)(
    struct UnityAudioEffectDefinition*** descptr);
extern void UnityRegisterAudioPlugin(
    UnityPluginGetAudioEffectDefinitionsFunc getAudioEffectDefinitions);
extern int UnityGetAudioEffectDefinitions(UnityAudioEffectDefinition*** definitionptr);

}  // extern "C"

@implementation GvrAudioAppController

- (UnityView *)createUnityView {
  UnityRegisterViewControllerListener(self);
  UnityRegisterAudioPlugin(UnityGetAudioEffectDefinitions);
  return [super createUnityView];
}

- (UIViewController *)unityViewController {
  return UnityGetGLViewController();
}

@end

IMPL_APP_CONTROLLER_SUBCLASS(GvrAudioAppController)
