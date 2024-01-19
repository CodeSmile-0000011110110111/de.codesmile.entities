// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestFixtures;
using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.PerformanceTesting;

namespace CodeSmile.Tests
{
	public class MeasureBaselines : EntitiesTestFixture
	{
		[Test, Performance] public void Measure_DefaultWorldUpdate() => MeasureWorldUpdate();

		[TestCase(100), TestCase(1000), TestCase(10000), TestCase(100000), Performance]
		public void Measure_CreateEntities(Int32 entitiesCount)
		{
			CreateWorld();

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
			CreateWorld();

			Measure.Method(() =>
				{
					var ecb = new EntityCommandBuffer(Allocator.TempJob);
					for (var i = 0; i < entitiesCount; i++)
						ecb.CreateEntity();

					ecb.Playback(EM);
					ecb.Dispose();
				})
				.DynamicMeasurementCount()
				.Run();
		}
	}
}
