// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestFixtures;
using NUnit.Framework;
using System;
using Unity.Burst;
using Unity.Entities;

namespace CodeSmile.Tests
{
	public partial class EntitiesBasicDataTransformTests : EntitiesTestFixture
	{
		[Test] public void TestData_ParallelUpdate_WritesEachComponentOnce()
		{
			CreateTestDataComponentEntities(32 * 128, typeof(TestDataTransformSystem), typeof(TestData));
			World.Update();

			ForEachComponentData<TestData>((entity, actual) =>
				Assert.AreEqual(new TestData { Owner = entity, UpdateCount = 1 }, actual));
		}

		private void CreateTestDataComponentEntities(Int32 entitiesCount, Type system, params ComponentType[] testComponents)
		{
			CreateWorld(system);
			CreateEntitiesWithComponents(entitiesCount, testComponents);
			//SetEntitiesComponentData(entity => new TestData());
		}

		public struct TestData : IComponentData
		{
			public Entity Owner;
			public Int32 UpdateCount;
			public override String ToString() => $"{nameof(TestData)}(Owner={Owner}, UpdateCount={UpdateCount})";
		}

		[BurstCompile, DisableAutoCreation]
		public partial struct TestDataTransformSystem : ISystem
		{
			[BurstCompile] public void OnCreate(ref SystemState state) => state.RequireForUpdate<TestData>();
			[BurstCompile] public void OnUpdate(ref SystemState state) => new TestDataTransformJob().ScheduleParallel();

			[BurstCompile] public partial struct TestDataTransformJob : IJobEntity
			{
				private void Execute(ref TestData testData, in Entity entity)
				{
					testData.Owner = entity;
					testData.UpdateCount++;
				}
			}
		}
	}
}
