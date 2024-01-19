// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Burst;
using Unity.Entities;

namespace CodeSmile.TestFixtures
{
	[BurstCompile, DisableAutoCreation]
	public partial struct MockSystem : ISystem, ISystemStartStop
	{
		private static Boolean s_DidCreate;
		private static Boolean s_DidDestroy;
		private static Int32 s_StartCount;
		private static Int32 s_StopCount;
		private static Int32 s_UpdateCount;
		public static Boolean DidCreate => s_DidCreate;
		public static Boolean DidDestroy => s_DidDestroy;
		public static Int32 UpdateCount => s_UpdateCount;

		public void OnStartRunning(ref SystemState state) => s_StartCount++;
		public void OnStopRunning(ref SystemState state) => s_StopCount++;

		private void OnCreate(ref SystemState state)
		{
			s_DidCreate = true;
			s_DidDestroy = false;
			s_StartCount = s_UpdateCount = s_StopCount = 0;
		}

		private void OnDestroy(ref SystemState state) => s_DidDestroy = true;

		private void OnUpdate(ref SystemState state) => s_UpdateCount++;
	}
}
