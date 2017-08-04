﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Elastic.ProcessManagement.Std;
using Xunit;
using FluentAssertions;

namespace Elastic.ProcessManagement.Tests
{
	public class EventBasedObservableProcessTestCases : TestsBase
	{
		[Fact]
		public void SingleLineNoEnter()
		{
			var seen = new List<string>();
			var process = new EventBasedObservableProcess(TestCaseArguments(nameof(SingleLineNoEnter)));
			process.Subscribe(c=>seen.Add(c.Line));
			process.WaitForCompletion(WaitTimeout);

			seen.Should().NotBeEmpty().And.HaveCount(1, string.Join(Environment.NewLine, seen));
			seen[0].Should().Be(nameof(SingleLineNoEnter));
		}

		[Fact]
		public void SingleLine()
		{
			var seen = new List<string>();
			var process = new EventBasedObservableProcess(TestCaseArguments(nameof(SingleLine)));
			process.Subscribe(c=>seen.Add(c.Line), e=>throw e);
			process.WaitForCompletion(WaitTimeout);

			seen.Should().NotBeEmpty().And.HaveCount(1, string.Join(Environment.NewLine, seen));
			seen[0].Should().Be(nameof(SingleLine));
		}

		[Fact]
		public void SingleLineSlowReadLine()
		{
			var seen = new List<string>();
			var process = new EventBasedObservableProcess(TestCaseArguments(nameof(SingleLine)))
			{
				SimulateToSlowBeginReadLine = true
			};
			process.Subscribe(c=>seen.Add(c.Line), e=>throw e);
			process.WaitForCompletion(WaitTimeout);

			seen.Should().NotBeEmpty().And.HaveCount(1, string.Join(Environment.NewLine, seen));
			seen[0].Should().Be(nameof(SingleLine));
		}
	}
}
