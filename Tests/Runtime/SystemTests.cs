// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.TestFixtures;
using NUnit.Framework;
using Unity.Entities;

namespace CodeSmile.Tests
{
	public class SystemTests : EntitiesTestFixture
	{
		[Test] public void Test()
		{
			World.CreateSystem<TestSystem>();
			World.Update();
		}
	}

	public partial struct TestSystem : ISystem
	{

	}
}
