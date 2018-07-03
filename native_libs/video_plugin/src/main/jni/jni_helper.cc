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
#include <assert.h>
#include <cstring>

#include "jni_helper.h"

namespace gvrvideo {

JNIHelper *JNIHelper::pInstance = nullptr;

const JNIHelper &JNIHelper::Get() {
  assert(pInstance);
  return *pInstance;
}

void JNIHelper::Initialize(JavaVM *vm, const char *className) {
  JNIHelper *helper = new JNIHelper(vm);

  helper->InitClassloader(className);
}

JNIHelper::JNIHelper(JavaVM *vm) {
  vm_ = vm;
  if (!pInstance && vm) {
    pInstance = this;
  }
}

JNIHelper::~JNIHelper() {
  if (vm_ && classLoader_) {
    auto env = Env();
    env->DeleteGlobalRef(classLoader_);
  }
}

JNIEnv *JNIHelper::Env() const {
  JNIEnv *env;
  int status = vm_->GetEnv((void **)&env, JNI_VERSION_1_6);
  if (status < 0) {
    status = vm_->AttachCurrentThread(&env, 0);
    if (status < 0) {
      return nullptr;
    }
  }
  return env;
}

void JNIHelper::InitClassloader(const char *className) {
  auto env = Env();
  auto firstclass = env->FindClass(className);
  jclass classClass = env->GetObjectClass(firstclass);
  auto classLoaderClass = env->FindClass("java/lang/ClassLoader");
  auto getClassLoaderMethod = env->GetMethodID(classClass, "getClassLoader",
                                               "()Ljava/lang/ClassLoader;");
  classLoader_ = env->NewGlobalRef(
      env->CallObjectMethod(firstclass, getClassLoaderMethod));
  findClassMethod_ = env->GetMethodID(classLoaderClass, "findClass",
                                      "(Ljava/lang/String;)Ljava/lang/Class;");
}

jclass JNIHelper::FindClass(const char *className) const {
  auto env = Env();
  jstring name = env->NewStringUTF(className);
  jclass ret = static_cast<jclass>(
      env->CallObjectMethod(classLoader_, findClassMethod_, name));
  env->DeleteLocalRef(name);
  return ret;
}

jobject JNIHelper::CallObjectMethod(jobject obj, jmethodID methodId,
                                    ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  jobject ret = env->CallObjectMethodV(obj, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }
  return ret;
}

void JNIHelper::CallStaticVoidMethod(jclass clz, jmethodID methodId,
                                     ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  env->CallStaticVoidMethodV(clz, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }
}

jobject JNIHelper::CallStaticObjectMethod(jclass clz, jmethodID methodId,
                                          ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  jobject ret = env->CallStaticObjectMethodV(clz, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }
  return ret;
}

void JNIHelper::CallVoidMethod(jobject obj, jmethodID methodId, ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  env->CallVoidMethodV(obj, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }
}

jboolean JNIHelper::CallBooleanMethod(jobject obj, jmethodID methodId,
                                      ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  jboolean ret = env->CallBooleanMethodV(obj, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }
  return ret;
}

jint JNIHelper::CallIntMethod(jobject obj, jmethodID methodId, ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  jint ret = env->CallIntMethodV(obj, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }
  return ret;
}

jfloat JNIHelper::CallFloatMethod(jobject obj, jmethodID methodId, ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  jfloat ret = env->CallFloatMethodV(obj, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }
  return ret;
}

jlong JNIHelper::CallLongMethod(jobject obj, jmethodID methodId, ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  jlong ret = env->CallLongMethodV(obj, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }
  return ret;
}

jbyte* JNIHelper::CallByteArrayMethod(jobject obj, jmethodID methodId, int* outSize, ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  jbyteArray ret = (jbyteArray)env->CallObjectMethodV(obj, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }

  if (ret == nullptr) {
    *outSize = 0;
    return nullptr;
  }

  *outSize = env->GetArrayLength(ret);
  jbyte* bytes = new jbyte[*outSize];
  env->GetByteArrayRegion(ret, 0, *outSize, bytes);
  return bytes;
}

const char *JNIHelper::CallStringMethod(jobject obj,
                                        jmethodID methodId, ...) const {
  auto env = Env();
  va_list args;
  va_start(args, methodId);
  jstring s = (jstring)env->CallObjectMethodV(obj, methodId, args);
  if (env->ExceptionCheck()) {
    env->ExceptionDescribe();
  }
  if (s) {
    const char *str = env->GetStringUTFChars(s, 0);
    char *ret = new char[strlen(str) + 1];
    strcpy(ret, str);
    env->ReleaseStringUTFChars(s, str);
    return ret;
  }
  return nullptr;
}

void JNIHelper::ReleaseString(const char *string) const {
  if (string) {
    delete string;
  }
}
}  // namespace gvrvideo
