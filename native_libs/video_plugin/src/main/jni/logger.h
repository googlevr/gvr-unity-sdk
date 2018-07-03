/* Copyright 2016 Google Inc. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#ifndef VR_GVR_DEMOS_VIDEO_PLUGIN_LOGGER_H_
#define VR_GVR_DEMOS_VIDEO_PLUGIN_LOGGER_H_

#define LOGD(category, ...) \
  ((void)__android_log_print(ANDROID_LOG_DEBUG, category, __VA_ARGS__))

#undef LOGE
#define LOGE(category, ...) \
  ((void)__android_log_print(ANDROID_LOG_ERROR, category, __VA_ARGS__))

#undef LOGI
#define LOGI(category, ...) \
  ((void)__android_log_print(ANDROID_LOG_INFO, category, __VA_ARGS__))

#undef LOGW
#define LOGW(category, ...) \
  ((void)__android_log_print(ANDROID_LOG_WARN, category, __VA_ARGS__))

#endif  // VR_GVR_DEMOS_VIDEO_PLUGIN_LOGGER_H_
