//-----------------------------------------------------------------------
// <copyright file="AssemblyVisibility.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.CompilerServices;

// This attribute makes the runtime assembly entities marked 'internal' visible to the tests
// located in the editor assembly.  This file is located under Tests so it can be easily removed
// along with other test-related code.
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

// Making internals visibile to NSubstitute's mock generation assembly.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2,PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

// Making internals visible to other testing assemblies for good measure, not stricly needed.  These actually
// are strongly named assemblies and require public keys that are currently absent.
[assembly: InternalsVisibleTo("nunit.core")]
[assembly: InternalsVisibleTo("nunit.core.interfaces")]
[assembly: InternalsVisibleTo("NSubstitute")]
