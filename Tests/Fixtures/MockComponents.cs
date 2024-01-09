// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Entities;
using Unity.Mathematics;

namespace CodeSmile.TestFixtures
{
	public struct IntComponent : IComponentData
	{
		public Int32 Value;
	}
	public struct Int2Component : IComponentData
	{
		public int2 Value;
	}
	public struct Int3Component : IComponentData
	{
		public int3 Value;
	}
	public struct Int4Component : IComponentData
	{
		public int4 Value;
	}
	public struct Int4x4Component : IComponentData
	{
		public int4x4 Value;
	}

	public struct FloatComponent : IComponentData
	{
		public Single Value;
	}

	public struct Float2Component : IComponentData
	{
		public float2 Value;
	}

	public struct Float3Component : IComponentData
	{
		public float3 Value;
	}

	public struct Float4Component : IComponentData
	{
		public float4 Value;
	}

	public struct Float4x4Component : IComponentData
	{
		public float4x4 Value;
	}

	/// <summary>
	/// Size: 128 Bytes, exactly the max. size of component data per entity in Entities 1.0
	/// </summary>
	public struct OptimalSizeComponent : IComponentData
	{
		public int4x4 IntMatrix;		// 64 Bytes
		public float4x4 FloatMatrix;	// 64 Bytes
	}

	/// <summary>
	/// Size: 256 Bytes, twice the max. size of component data per entity in Entities 1.0
	/// </summary>
	public struct DoubleOptimalSizeComponent : IComponentData
	{
		public int4x4 IntMatrix;		// 64 Bytes
		public float4x4 FloatMatrix;	// 64 Bytes
		public double4x4 DoubleMatrix;	// 128 Bytes
	}

	public struct TagAComponent : IComponentData{}
	public struct TagBComponent : IComponentData{}
	public struct TagCComponent : IComponentData{}
}
