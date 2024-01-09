﻿// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestFixtures;
using NUnit.Framework;
using Unity.Entities;
using Unity.PerformanceTesting;

namespace CodeSmileEditor.Tests
{
	public class EntitiesTestFixtureTests : EntitiesTestFixture
	{
		[Test] public void Fixture_DefaultWorld_UpdateDoesNotThrow() => World.Update();

		[Test] public void Fixture_EmptyWorld_UpdateDoesNotThrow() => EmptyWorld.Update();

		[Test, Performance] public void Fixture_DefaultWorld_MeasureDoesNotThrow() => MeasureWorldUpdate();

		[Test, Performance] public void Fixture_EmptytWorld_MeasureDoesNotThrow()
		{
			CreateEmptyWorld();
			MeasureWorldUpdate();
		}

		[Test] public void TestFixtureSystem_ManualUpdate_DidUpdateOnce()
		{
			var handle = World.CreateSystem<MockSystem>();
			handle.Update(World.Unmanaged);

			Assert.AreEqual(1, MockSystem.UpdateCount);
		}

		[Test] public void TestFixtureSystem_WorldUpdate_DidUpdateOnce()
		{
			World.CreateSystem<MockSystem>();
			World.Update();

			Assert.AreEqual(1, MockSystem.UpdateCount);
		}
	}
}
