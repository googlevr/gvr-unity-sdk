//-----------------------------------------------------------------------
// <copyright file="PositionSwapper.cs" company="Google Inc.">
// Copyright 2016 Google Inc. All rights reserved.
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
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleVR.VideoDemo
{
    using UnityEngine;

    /// <summary>
    /// Sets the position of the transform to a position specifed in a list.
    /// </summary>
    public class PositionSwapper : MonoBehaviour
    {
        private int currentIndex = -1;

        public Vector3[] Positions = new Vector3[0];

        public void SetConstraint(int index)
        {
        }

        public void SetPosition(int index)
        {
            currentIndex = index % Positions.Length;
            transform.localPosition = Positions[currentIndex];
        }

        #if UNITY_EDITOR
        private static void SaveToIndex(UnityEditor.MenuCommand mc, int index)
        {
            PositionSwapper ps = mc.context as PositionSwapper;
            while (ps.Positions.Length <= index)
            {
                UnityEditor.ArrayUtility.Add<Vector3>(ref ps.Positions, Vector3.zero);
            }

            ps.Positions[index] = ps.transform.localPosition;
        }

        private static void LoadIndex(UnityEditor.MenuCommand mc, int index)
        {
            PositionSwapper ps = mc.context as PositionSwapper;
            ps.SetPosition(index);
        }

        [UnityEditor.MenuItem("CONTEXT/PositionSwapper/SavePositionToIndex0")]
        private static void SaveToIndex0(UnityEditor.MenuCommand mc)
        {
            SaveToIndex(mc, 0);
        }

        [UnityEditor.MenuItem("CONTEXT/PositionSwapper/SavePositionToIndex1")]
        private static void SaveToIndex1(UnityEditor.MenuCommand mc)
        {
            SaveToIndex(mc, 1);
        }

        [UnityEditor.MenuItem("CONTEXT/PositionSwapper/SavePositionToIndex2")]
        private static void SaveToIndex2(UnityEditor.MenuCommand mc)
        {
            SaveToIndex(mc, 2);
        }

        [UnityEditor.MenuItem("CONTEXT/PositionSwapper/SavePositionToIndex3")]
        private static void SaveToIndex3(UnityEditor.MenuCommand mc)
        {
            SaveToIndex(mc, 3);
        }

        [UnityEditor.MenuItem("CONTEXT/PositionSwapper/LoadPosition0")]
        private static void LoadPosition0(UnityEditor.MenuCommand mc)
        {
            LoadIndex(mc, 0);
        }

        [UnityEditor.MenuItem("CONTEXT/PositionSwapper/LoadPosition1")]
        private static void LoadPosition1(UnityEditor.MenuCommand mc)
        {
            LoadIndex(mc, 1);
        }

        [UnityEditor.MenuItem("CONTEXT/PositionSwapper/LoadPosition2")]
        private static void LoadPosition2(UnityEditor.MenuCommand mc)
        {
            LoadIndex(mc, 2);
        }

        [UnityEditor.MenuItem("CONTEXT/PositionSwapper/LoadPosition3")]
        private static void LoadPosition3(UnityEditor.MenuCommand mc)
        {
            LoadIndex(mc, 3);
        }
#endif // UNITY_EDITOR
    }
}
