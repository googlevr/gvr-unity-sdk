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
#ifndef VR_GVR_DEMOS_VIDEO_PLUGIN_JNIHELPER_H_
#define VR_GVR_DEMOS_VIDEO_PLUGIN_JNIHELPER_H_

#include <jni.h>

namespace gvrvideo {

// JNI pointer management and caching across threads.
class JNIHelper {
 public:
  static void Initialize(JavaVM *vm, const char *className);

  static const JNIHelper &Get();

  JNIEnv *Env() const;

  // Custom implementation of FindClass which uses the class loader found on the
  // main thread when initializing.  This makes class lookups on other threads
  // work as expected.
  jclass FindClass(const char *className) const;

  jobject CallStaticObjectMethod(jclass clz, jmethodID methodId, ...) const;

  void CallStaticVoidMethod(jclass clz, jmethodID methodId, ...) const;

  jobject CallObjectMethod(jobject obj, jmethodID methodId, ...) const;

  jboolean CallBooleanMethod(jobject obj, jmethodID jmethodId, ...) const;

  jint CallIntMethod(jobject obj, jmethodID jmethodId, ...) const;

  jfloat CallFloatMethod(jobject obj, jmethodID jmethodId, ...) const;

  jlong CallLongMethod(jobject obj, jmethodID jmethodId, ...) const;

  jbyte* CallByteArrayMethod(jobject obj, jmethodID jmethodId, int* outSize, ...) const;

  void CallVoidMethod(jobject obj, jmethodID jmethodId, ...) const;

  // Returns a const char* that needs to be released by this class.
  const char *CallStringMethod(jobject obj, jmethodID methodId, ...) const;

  // Releases the memory used by the string passed in.  This string is assumed
  // to have been returned from CallStringMethod.
  void ReleaseString(const char *string) const;

 protected:
  void InitClassloader(const char *className);

 private:
  JNIHelper(JavaVM *vm);

  ~JNIHelper();

  JavaVM *vm_;
  jobject classLoader_;
  jmethodID findClassMethod_;

  static JNIHelper *pInstance;
};
}  // namespace gvrvideo
#endif  // VR_GVR_DEMOS_VIDEO_PLUGIN_JNIHELPER_H_
