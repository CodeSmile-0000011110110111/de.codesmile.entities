// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestFixtures;
using NUnit.Framework;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using UnityEngine;

namespace CodeSmile.Tests
{
	public partial class EntitiesMeasureTests : EntitiesTestFixture
	{
		[Test, Performance] public void Measure_DefaultWorldUpdate() => MeasureWorldUpdate();

		[Test, Performance] public void Measure_EmptytWorldUpdate()
		{
			CreateEmptyWorld();
			MeasureWorldUpdate();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), Performance]
		public void Measure_CreateEntities(Int32 entitiesCount)
		{
			CreateEmptyWorld();

			Measure.Method(() =>
				{
					for (var i = 0; i < entitiesCount; i++)
						EM.CreateEntity();
				})
				.DynamicMeasurementCount()
				.Run();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), Performance]
		public void Measure_CreateEntities_WithCommandBuffer(Int32 entitiesCount)
		{
			CreateEmptyWorld();

			Measure.Method(() =>
				{
					var ecb = new EntityCommandBuffer(Allocator.TempJob);
					for (var i = 0; i < entitiesCount; i++)
						ecb.CreateEntity();

					ecb.Playback(EM);
				})
				.DynamicMeasurementCount()
				.Run();
		}

		// [TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		// public void Measure_ReadWriteComponentData_ScheduleJob(Int32 entitiesCount)
		// {
		// 	SetupReadWriteComponentTest(entitiesCount);
		//
		// 	MeasureWorldUpdate();
		//
		// 	VerifyReadWriteComponentTest();
		// }

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_ScheduleParallelJob(Int32 entitiesCount)
		{
			SetupReadWriteComponentTest(entitiesCount, true);

			MeasureWorldUpdate();

			VerifyReadWriteComponentTest();
		}


		private void SetupReadWriteComponentTest(Int32 entitiesCount, bool parallel)
		{
			CreateDefaultWorld();
			CreateEntitiesWithComponents(entitiesCount, typeof(IntComponent), typeof(Int4x4Component));
			foreach (var entity in EM.GetAllEntities())
				EM.SetComponentData(entity, new IntComponent { Value = 0xff });
		}
		private void VerifyReadWriteComponentTest()
		{
			foreach (var entity in EM.GetAllEntities())
			{
				if (EM.HasComponent<Int4x4Component>(entity))
				{
					// just checking one
					var result = EM.GetComponentData<Int4x4Component>(entity);
					Assert.AreEqual(0xff, result.Value.c0.x);
					Assert.AreEqual(0xff, result.Value.c1.y);
					Assert.AreEqual(0xff, result.Value.c2.z);
					Assert.AreEqual(0xff, result.Value.c3.w);
					break;
				}
			}
		}

		[BurstCompile(CompileSynchronously = true)]
		public partial struct ComponentReadWriteTestJob : IJobEntity
		{
			private void Execute(in IntComponent readComponent, ref Int4x4Component writeComponent)
			{
				var i4 = new int4(readComponent.Value);
				writeComponent.Value = new int4x4(i4, i4, i4, i4);
			}
		}

		[BurstCompile(CompileSynchronously = true)]
		internal partial struct ComponentReadWriteParallelTestSystem : ISystem
		{
			[BurstCompile(CompileSynchronously = true)]
			public void OnUpdate(ref SystemState state)
			{
					new ComponentReadWriteTestJob().ScheduleParallel();
			}
		}

		// [BurstCompile(CompileSynchronously = true)]
		// internal partial struct ComponentReadWriteTestSystem : ISystem
		// {
		// 	[BurstCompile(CompileSynchronously = true)]
		// 	public void OnUpdate(ref SystemState state)
		// 	{
		// 			new ComponentReadWriteTestJob().Schedule();
		// 	}
		// }
	}
}
