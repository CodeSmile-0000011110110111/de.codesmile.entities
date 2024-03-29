﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.LowLevel;

namespace CodeSmile.TestFixtures
{
	public abstract class EntitiesTestFixture
	{
		private World m_PreviousWorld;
		private World m_World;

		private EntityManager m_EntityManager;
		private EntityManager.EntityManagerDebug m_DebugEntityManager;
		private PlayerLoopSystem m_PreviousPlayerLoop;
		private Boolean m_WasJobsDebuggerEnabled;

		protected World World => m_World ?? CreateWorld();

		protected EntityManager EM => m_EntityManager;

		[SetUp] public void Setup() {}

		[TearDown] public void TearDown() => DestroyWorld();

		protected String LogSystemsToString()
		{
			var systems = World.Systems;
			var sb = new StringBuilder($"Component Systems: {systems.Count}\n");

			var groups = new List<ComponentSystemGroup>();
			foreach (var system in systems)
			{
				if (system is ComponentSystemGroup group)
					groups.Add(group);
			}

			foreach (var group in groups)
			{
				sb.AppendLine(group.GetType().Name);
				foreach (var system in group.ManagedSystems)
					sb.AppendLine($"    {system}");
			}

			return sb.ToString();
		}

		protected void MeasureWorldUpdate(Int32 iterations = 1) => Measure.Method(() =>
			{
				World.Update();
				m_EntityManager.CompleteAllTrackedJobs();
			})
			.WarmupCount(2)
			.DynamicMeasurementCount()
			.IterationsPerMeasurement(Mathf.Max(1, iterations))
			.Run();

		//protected World CreateEmptyWorld(params Type[] systems) => CreateWorld(true, systems);
		//protected World CreateDefaultWorld(params Type[] systems) => CreateWorld(false, systems);

		protected World CreateWorld(params Type[] simulationSystems)
		{
			if (m_World != null)
				throw new InvalidOperationException("CreateWorld: World already created");

			SetDefaultPlayerLoop();
			InitWorld();
			InitSystems(simulationSystems);
			SetEntityManagerReferences();
			ClearSystemIds();
			EnableJobsDebugger();
			EntitiesJournaling.Clear();

			return m_World;
		}

		private void DestroyWorld()
		{
			if (m_World != null && m_World.IsCreated)
			{
				DestroyAllSystems();
				m_DebugEntityManager.CheckInternalConsistency();
				RestorePreviousWorld();
			}

			ClearSystemIds();
			RestorePlayerLoop();
			RestoreJobsDebugger();
		}

		private void InitSystems(Type[] systems)
		{
			if (systems == null)
				return;

			var simGroup = World.GetExistingSystemManaged<SimulationSystemGroup>();
			foreach (var systemType in systems)
			{
				if (systemType == null)
					continue;

				var systemHandle = m_World.GetOrCreateSystem(systemType);
				simGroup.AddSystemToUpdateList(systemHandle);
			}
		}

		private void InitWorld()
		{
			m_PreviousWorld = World.DefaultGameObjectInjectionWorld;
			m_World = World.DefaultGameObjectInjectionWorld = DefaultWorldInitialization.Initialize("TEST: Default World");
			m_World.UpdateAllocatorEnableBlockFree = true;
		}

		private void SetEntityManagerReferences()
		{
			m_EntityManager = m_World.EntityManager;
			m_DebugEntityManager = new EntityManager.EntityManagerDebug(m_EntityManager);
		}

		private void EnableJobsDebugger()
		{
			// Many ECS tests will only pass if the Jobs Debugger enabled;
			// force it enabled for all tests, and restore the original value at teardown.
			m_WasJobsDebuggerEnabled = JobsUtility.JobDebuggerEnabled;
			JobsUtility.JobDebuggerEnabled = true;
		}

		private void RestoreJobsDebugger() => JobsUtility.JobDebuggerEnabled = m_WasJobsDebuggerEnabled;

		private void SetDefaultPlayerLoop()
		{
			// unit tests preserve the current player loop to restore later, and start from a blank slate.
			m_PreviousPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
		}

		private void RestorePlayerLoop() => PlayerLoop.SetPlayerLoop(m_PreviousPlayerLoop);

		private void DestroyAllSystems()
		{
			while (m_World.Systems.Count > 0)
			{
				m_World.DestroySystemManaged(m_World.Systems[0]);
			}
		}

		private void RestorePreviousWorld()
		{
			m_World.Dispose();
			World.DefaultGameObjectInjectionWorld = m_PreviousWorld;

			m_World = m_PreviousWorld = null;
			m_EntityManager = default;
			m_DebugEntityManager = default;
		}

		private void ClearSystemIds() => typeof(JobsUtility)
			.GetMethod("ClearSystemIds", BindingFlags.Static | BindingFlags.NonPublic)
			.Invoke(null, null);

		protected void CreateEntitiesWithComponents(Int32 entitiesCount, params ComponentType[] components)
		{
			var archetype = EM.CreateArchetype(components);

			var ecb = new EntityCommandBuffer(Allocator.Temp);
			for (var i = 0; i < entitiesCount; i++)
				ecb.CreateEntity(archetype);

			ecb.Playback(EM);
			ecb.Dispose();
		}

		protected void SetEntitiesComponentData<T>(Func<Entity, T> getDataFunc) where T : unmanaged, IComponentData
		{
			if (getDataFunc == null)
				return;

			foreach (var entity in EM.GetAllEntities())
			{
				if (EM.HasComponent<T>(entity))
				{
					var data = getDataFunc.Invoke(entity);
					EM.SetComponentData(entity, data);
				}
			}
		}

		protected void ForEachComponentData<T>(Action<Entity, T> forEachAction) where T : unmanaged, IComponentData
		{
			foreach (var entity in EM.GetAllEntities())
			{
				if (EM.HasComponent<T>(entity))
					forEachAction.Invoke(entity, EM.GetComponentData<T>(entity));
			}
		}
	}
}
