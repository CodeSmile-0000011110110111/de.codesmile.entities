// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestFixtures;
using NUnit.Framework;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using UnityEngine;

namespace CodeSmile.Tests
{
	public class MeasureReadWriteComponentData : EntitiesTestFixture
	{
		private const Int32 TestValue = 0xff;

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_SystemBase(Int32 entitiesCount)
		{
			CreateReadWriteComponentTestEntities(entitiesCount, typeof(ComponentReadWriteSystemBaseSystem), typeof(ComponentReadWriteSystemBaseSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_Job(Int32 entitiesCount)
		{
			CreateReadWriteComponentTestEntities(entitiesCount, typeof(ComponentReadWriteJobSystem), typeof(ComponentReadWriteJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_BurstedJob(Int32 entitiesCount)
		{
			CreateReadWriteComponentTestEntities(entitiesCount, typeof(ComponentReadWriteBurstedJobSystem), typeof(ComponentReadWriteBurstedJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_ParallelJob(Int32 entitiesCount)
		{
			CreateReadWriteComponentTestEntities(entitiesCount, typeof(ComponentReadWriteParallelJobSystem), typeof(ComponentReadWriteParallelJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_BurstedParallelJob(Int32 entitiesCount)
		{
			CreateReadWriteComponentTestEntities(entitiesCount, typeof(ComponentReadWriteBurstedParallelJobSystem),
				typeof(ComponentReadWriteBurstedParallelJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_BurstedParallelCallFuncJob(Int32 entitiesCount)
		{
			CreateReadWriteComponentTestEntities(entitiesCount, typeof(ComponentReadWriteBurstedParallelCallFuncJobSystem),
				typeof(ComponentReadWriteBurstedParallelCallFuncJobSystem.TagComponent));
			MeasureWorldUpdate();
			AssertReadWriteComponentTest();
		}

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), TestCase(1000000), Performance]
		public void Measure_ReadWriteComponentData_EntityManagerGetSetComponentData(Int32 entitiesCount)
		{
			CreateReadWriteComponentTestEntities(entitiesCount, null, typeof(EntityManagerSetDataTagComponent));

			Measure.Method(() =>
				{
					foreach (var entity in EM.GetAllEntities())
					{
						var value = EM.GetComponentData<IntComponent>(entity).Value;
						EM.SetComponentData(entity, new Int4x4Component
						{
							Value = new int4x4(new int4(value), new int4(value), new int4(value), new int4(value)),
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

		private void CreateReadWriteComponentTestEntities(Int32 entitiesCount, Type system, params ComponentType[] components)
		{
			CreateWorld(system);
			components = components.Append(typeof(IntComponent)).Append(typeof(Int4x4Component)).ToArray();
			CreateEntitiesWithComponents(entitiesCount, components.ToArray());
			SetEntitiesComponentData(new IntComponent { Value = TestValue });
		}

		private void AssertReadWriteComponentTest()
		{
			foreach (var entity in EM.GetAllEntities())
			{
				if (EM.HasComponent<Int4x4Component>(entity))
				{
					var result = EM.GetComponentData<Int4x4Component>(entity);
					Assert.AreEqual(TestValue, result.Value.c0.x);
					Assert.AreEqual(TestValue, result.Value.c1.y);
					Assert.AreEqual(TestValue, result.Value.c2.z);
					Assert.AreEqual(TestValue, result.Value.c3.w);

					// just checking one
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

	[DisableAutoCreation]
	internal partial struct ComponentReadWriteJobSystem : ISystem
	{
		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteJob().Schedule();

		public struct TagComponent : IComponentData {}
	}

	[DisableAutoCreation]
	internal partial struct ComponentReadWriteParallelJobSystem : ISystem
	{
		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteJob().ScheduleParallel();

		public struct TagComponent : IComponentData {}
	}

	[BurstCompile(CompileSynchronously = true), DisableAutoCreation]
	internal partial struct ComponentReadWriteBurstedJobSystem : ISystem
	{
		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteBurstedJob().Schedule();

		public struct TagComponent : IComponentData {}
	}

	[BurstCompile(CompileSynchronously = true), DisableAutoCreation]
	internal partial struct ComponentReadWriteBurstedParallelJobSystem : ISystem
	{
		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteBurstedJob().ScheduleParallel();

		public struct TagComponent : IComponentData {}
	}

	[BurstCompile(CompileSynchronously = true), DisableAutoCreation]
	internal partial struct ComponentReadWriteBurstedParallelCallFuncJobSystem : ISystem
	{
		public struct TagComponent : IComponentData {}

		[BurstCompile(CompileSynchronously = true)]
		public void OnCreate(ref SystemState state) => state.RequireForUpdate<TagComponent>();

		[BurstCompile(CompileSynchronously = true)]
		public void OnUpdate(ref SystemState state) => new ComponentReadWriteBurstedCallFuncJob().ScheduleParallel();
	}

	[DisableAutoCreation]
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
