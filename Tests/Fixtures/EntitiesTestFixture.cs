﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using NUnit.Framework;
using System;
using System.Reflection;
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

		public World World => m_World ?? CreateDefaultWorld();
		public World EmptyWorld => m_World ?? CreateEmptyWorld();

		public EntityManager EM => m_EntityManager;

		[SetUp] public void Setup() {}

		[TearDown] public void TearDown() => DestroyWorld();

		public void MeasureWorldUpdate(Int32 iterations = 1) => Measure.Method(() =>
			{
				World.Update();
				m_EntityManager.CompleteAllTrackedJobs();
			})
			.WarmupCount(2)
			.DynamicMeasurementCount()
			.IterationsPerMeasurement(Mathf.Max(1, iterations))
			.Run();

		public World CreateEmptyWorld() => CreateWorld(true);
		public World CreateDefaultWorld() => CreateWorld(false);

		private World CreateWorld(Boolean emptyWorld)
		{
			if (m_PreviousWorld != null)
				throw new InvalidOperationException("CreateWorld called more than once");

			SetDefaultPlayerLoop();

			m_PreviousWorld = World.DefaultGameObjectInjectionWorld;
			m_World = World.DefaultGameObjectInjectionWorld = emptyWorld
				? new World("Test World: Empty")
				: DefaultWorldInitialization.Initialize("Test World: Default");
			m_World.UpdateAllocatorEnableBlockFree = true;

			m_EntityManager = m_World.EntityManager;
			m_DebugEntityManager = new EntityManager.EntityManagerDebug(m_EntityManager);

			ClearSystemIds();
			EnableJobsDebugger();
			EntitiesJournaling.Clear();

			return m_World;
		}

		private void EnableJobsDebugger()
		{
			// Many ECS tests will only pass if the Jobs Debugger enabled;
			// force it enabled for all tests, and restore the original value at teardown.
			m_WasJobsDebuggerEnabled = JobsUtility.JobDebuggerEnabled;
			JobsUtility.JobDebuggerEnabled = true;
		}

		private void RestoreJobsDebugger() => JobsUtility.JobDebuggerEnabled = m_WasJobsDebuggerEnabled;

		protected void DestroyWorld()
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

			m_World = null;
			m_PreviousWorld = null;
			m_EntityManager = default;
			m_DebugEntityManager = default;
		}

		private void ClearSystemIds() => typeof(JobsUtility)
			.GetMethod("ClearSystemIds", BindingFlags.Static | BindingFlags.NonPublic)
			.Invoke(null, null);
	}
}