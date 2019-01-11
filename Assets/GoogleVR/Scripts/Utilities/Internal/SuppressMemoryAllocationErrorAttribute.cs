//-----------------------------------------------------------------------
// <copyright file="SuppressMemoryAllocationErrorAttribute.cs" company="Google">
// Copyright 2018 Google Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
// This module is used internally by Google for assessing memory allocations in public methods.
// Feel free to disregard all [SuppressMemoryAllocationErrorAttribute] declarations on methods.
namespace Gvr.Internal
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A custom Attribute class to annotate functions that are allowed to allocate memory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class SuppressMemoryAllocationErrorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:GoogleVR.Internal.SuppressMemoryAllocationErrorAttribute"/> class.
        /// </summary>
        public SuppressMemoryAllocationErrorAttribute()
        {
            this.IsWarning = false;
            this.Reason = "Unknown";
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show a warning instead of an error.
        /// </summary>
        public bool IsWarning { get; set; }

        /// <summary>
        /// Gets or sets the reason for suppressing the memory allocation error.
        /// </summary>
        public string Reason { get; set; }
    }
}
