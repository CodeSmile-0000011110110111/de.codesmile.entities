// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestFixtures;
using NUnit.Framework;
using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.PerformanceTesting;

namespace CodeSmile.Tests
{
	public class MeasureReadWriteComponentData : EntitiesTestFixture
	{
		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_SystemBase(Int32 entitiesCount)
		{
			CreateReadWriteComponentEntities(entitiesCount, typeof(ComponentReadWriteSystemBaseSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_Job(Int32 entitiesCount)
		{
			CreateReadWriteComponentEntities(entitiesCount, typeof(ComponentReadWriteJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_BurstedJob(Int32 entitiesCount)
		{
			CreateReadWriteComponentEntities(entitiesCount, typeof(ComponentReadWriteBurstedJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_ParallelJob(Int32 entitiesCount)
		{
			CreateReadWriteComponentEntities(entitiesCount, typeof(ComponentReadWriteParallelJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_BurstedParallelJob(Int32 entitiesCount)
		{
			CreateReadWriteComponentEntities(entitiesCount,
				typeof(ComponentReadWriteBurstedParallelJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_BurstedParallelCallFuncJob(Int32 entitiesCount)
		{
			CreateReadWriteComponentEntities(entitiesCount,
				typeof(ComponentReadWriteBurstedParallelCallFuncJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_EntityManagerSetComponentData(Int32 entitiesCount)
		{
			CreateReadWriteComponentEntities(entitiesCount, typeof(EntityManagerSetDataTagComponent));

			Measure.Method(() =>
				{
					foreach (var entity in EM.GetAllEntities())
					{
						EM.SetComponentData(entity, new Int4x4Component
						{
							Value = new int4x4(new int4(0xff), new int4(0xff), new int4(0xff), new int4(0xff)),
						});
					}
					EM.CompleteAllTrackedJobs();
				})
				.WarmupCount(2)
				.DynamicMeasurementCount()
				.IterationsPerMeasurement(1)
				.Run();

			AssertReadWriteComponentTest();
		}

		private void CreateReadWriteComponentEntities(Int32 entitiesCount, Type tagComponent)
		{
			CreateDefaultWorld();
			CreateEntitiesWithComponents(entitiesCount, typeof(IntComponent), typeof(Int4x4Component), tagComponent);
			foreach (var entity in EM.GetAllEntities())
				EM.SetComponentData(entity, new IntComponent { Value = 0xff });
		}

		private void AssertReadWriteComponentTest()
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
	}

	public struct EntityManagerSetDataTagComponent : IComponentData {}

	public partial struct ComponentReadWriteJob : IJobEntity
	{
		private void Execute(in IntComponent readComponent, ref Int4x4Component writeComponent)
		{
			var i4 = new int4(readComponent.Value);
			writeComponent.Value = new int4x4(i4, i4, i4, i4);
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	public partial struct ComponentReadWriteBurstedJob : IJobEntity
	{
		private void Execute(in IntComponent readComponent, ref Int4x4Component writeComponent)
		{
			var i4 = new int4(readComponent.Value);
			writeComponent.Value = new int4x4(i4, i4, i4, i4);
		}
	}

	[BurstCompile(CompileSynchronously = true)]
	public partial struct ComponentReadWriteBurstedCallFuncJob : IJobEntity
	{
		private void Execute(in IntComponent readComponent, ref Int4x4Component writeComponent) =>
			Funcs.ToInt4x4(readComponent.Value, ref writeComponent.Value);
	}

	[BurstCompile(CompileSynchronously = true)]
	public static class Funcs
	{
		[BurstCompile(CompileSynchronously = true), MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ToInt4x4(Int32 input, ref int4x4 output)
		{
			var i4 = new int4(input);
			output = new int4x4(i4, i4, i4, i4);
		}
	}

	[UpdateInGroup(typeof(SimulationSystemGroup))]
	internal partial struct ComponentReadWriteJobSystem : ISystem
	{
		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteJob().Schedule();

		public struct TagComponent : IComponentData {}
	}

	[UpdateInGroup(typeof(SimulationSystemGroup))]
	internal partial struct ComponentReadWriteParallelJobSystem : ISystem
	{
		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteJob().ScheduleParallel();

		public struct TagComponent : IComponentData {}
	}

	[BurstCompile(CompileSynchronously = true), UpdateInGroup(typeof(SimulationSystemGroup))]
	internal partial struct ComponentReadWriteBurstedJobSystem : ISystem
	{
		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteBurstedJob().Schedule();

		public struct TagComponent : IComponentData {}
	}

	[BurstCompile(CompileSynchronously = true), UpdateInGroup(typeof(SimulationSystemGroup))]
	internal partial struct ComponentReadWriteBurstedParallelJobSystem : ISystem
	{
		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteBurstedJob().ScheduleParallel();

		public struct TagComponent : IComponentData {}
	}

	[BurstCompile(CompileSynchronously = true), UpdateInGroup(typeof(SimulationSystemGroup))]
	internal partial struct ComponentReadWriteBurstedParallelCallFuncJobSystem : ISystem
	{
		public struct TagComponent : IComponentData {}

		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteBurstedCallFuncJob().ScheduleParallel();
	}

	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public partial class ComponentReadWriteSystemBaseSystem : SystemBase
	{
		protected override void OnCreate() => RequireForUpdate<TagComponent>();

		protected override void OnUpdate()
		{
			foreach (var (readComponent, writeComponent) in
			         SystemAPI.Query<RefRO<IntComponent>, RefRW<Int4x4Component>>())
			{
				var i4 = new int4(readComponent.ValueRO.Value);
				writeComponent.ValueRW.Value = new int4x4(i4, i4, i4, i4);
			}
		}

		public struct TagComponent : IComponentData {}
	}
}
