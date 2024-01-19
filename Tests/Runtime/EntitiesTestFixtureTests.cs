// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestFixtures;
using NUnit.Framework;
using Unity.Entities;
using UnityEngine;

namespace CodeSmile.Tests
{
	public class EntitiesTestFixtureTests : EntitiesTestFixture
	{
		[Test] public void Fixture_DefaultWorld_UpdateDoesNotThrow() => World.Update();

		[Test] public void MockSystem_ManualUpdate_SystemUpdatedOnce()
		{
			var handle = World.CreateSystem<MockSystem>();
			handle.Update(World.Unmanaged);

			Debug.Log(LogSystemsToString());
			Assert.AreEqual(1, MockSystem.UpdateCount);
		}

		[Test] public void MockSystem_WorldUpdate_SystemUpdatedOnce()
		{
			var handle = World.CreateSystem<MockSystem>();
			World.GetExistingSystemManaged<InitializationSystemGroup>().AddSystemToUpdateList(handle);
			World.Update();

			Debug.Log(LogSystemsToString());
			Assert.AreEqual(1, MockSystem.UpdateCount);
		}

		[Test] public void MockSystem_CreateWorldAndUpdate_SystemUpdatedOnce()
		{
			CreateWorld(typeof(MockSystem));
			World.Update();

			Debug.Log(LogSystemsToString());
			Assert.AreEqual(1, MockSystem.UpdateCount);
		}
	}
}
