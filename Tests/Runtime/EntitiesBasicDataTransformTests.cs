// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestFixtures;
using NUnit.Framework;
using System;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace CodeSmile.Tests
{
	public partial class EntitiesBasicDataTransformTests : EntitiesTestFixture
	{
		[Test] public void TestSimpleDataTransform()
		{
			CreateTestDataComponentEntities(1000, typeof(TestData));
			World.Update();

			//World.GetExistingSystem<TestDataTransformSystem>()
			Debug.Log(LogSystemsToString());
		}

		private void CreateTestDataComponentEntities(Int32 entitiesCount, params ComponentType[] testComponents)
		{
			CreateWorld();
			CreateEntitiesWithComponents(entitiesCount, testComponents);
			SetEntitiesComponentData(new TestData(){});
		}

		public struct TestData : IComponentData
		{
			public Boolean DidUpdate;
			public Entity OwningEntity;
			public Int32 ChunkIndexInQuery;
		}

		[BurstCompile, DisableAutoCreation]
		public partial struct TestDataTransformSystem : ISystem
		{
			[BurstCompile] public void OnCreate(ref SystemState state) => state.RequireForUpdate<TestData>();
			[BurstCompile] public void OnUpdate(ref SystemState state) => new TestDataTransformJob().ScheduleParallel();
		}

		[BurstCompile] public partial struct TestDataTransformJob : IJobEntity
		{
			private void Execute([ChunkIndexInQuery] Int32 chunkIndex, ref TestData testData, in Entity entity)
			{
				testData.DidUpdate = true;
				testData.OwningEntity = entity;
				testData.ChunkIndexInQuery = chunkIndex;
			}
		}
	}
}
