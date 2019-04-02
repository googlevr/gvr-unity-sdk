#
# Copyright (C) 2019 The Android Open Source Project
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#

cmake_minimum_required(VERSION 3.4.1)
project(gvrvideo LANGUAGES C CXX)

# set up common compile options
set(CMAKE_CXX_FLAGS  "${CMAKE_CXX_FLAGS} -Wall -fno-exceptions -fno-rtti")

# now build app's shared lib
add_library(${PROJECT_NAME}
  SHARED
    frame_buffer.cc
    gvrvideo_unity.cc
    video_player_holder.cc
    video_quad_screen.cc
    glutils.cc
    jni_helper.cc
    video_player_impl.cc
    video_support_impl.cc
    ndk_helper/vecmath.cc
)

set_target_properties(${PROJECT_NAME}
  PROPERTIES
    CXX_STANDARD 11
    CXX_STANDARD_REQUIRED YES
    CXX_EXTENSIONS NO
)
target_include_directories(${PROJECT_NAME}
  PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}/ndk_helper
    ${CMAKE_CURRENT_SOURCE_DIR}/Unity)

# add lib dependencies
target_link_libraries(${PROJECT_NAME}
  PRIVATE
    android
    log
    EGL
    GLESv2
)
