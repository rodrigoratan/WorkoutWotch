﻿namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;

    [TestFixture]
    public class MetronomeActionParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Test]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => MetronomeActionParser.GetParser(null));
        }

        [TestCase(
            "Metronome at 0s*, 1s, 1s, 2s-",
            new [] { 0, 1 * msInSecond, 1 * msInSecond, 2 * msInSecond },
            new [] { MetronomeTickType.Bell, MetronomeTickType.Click, MetronomeTickType.Click, MetronomeTickType.None })]
        [TestCase(
            "METRONOME At 0s*, 1s, 1s, 2s-",
            new [] { 0, 1 * msInSecond, 1 * msInSecond, 2 * msInSecond },
            new [] { MetronomeTickType.Bell, MetronomeTickType.Click, MetronomeTickType.Click, MetronomeTickType.None })]
        [TestCase(
            "Metronome  \t at   0s*   ,  \t  1s ,  \t 1s  ,  2s-",
            new [] { 0, 1 * msInSecond, 1 * msInSecond, 2 * msInSecond },
            new [] { MetronomeTickType.Bell, MetronomeTickType.Click, MetronomeTickType.Click, MetronomeTickType.None })]
        [TestCase(
            "Metronome at 0h 1m 2s, 2m 5s",
            new [] { 1 * msInMinute + 2 * msInSecond, 2 * msInMinute + 5 * msInSecond },
            new [] { MetronomeTickType.Click, MetronomeTickType.Click })]
        public void can_parse_correctly_formatted_input(string input, int[] expectedPeriodsBeforeMilliseconds, MetronomeTickType[] expectedTickTypes)
        {
            Assert.AreEqual(expectedPeriodsBeforeMilliseconds.Length, expectedTickTypes.Length);

            var result = MetronomeActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose)).Parse(input);
            Assert.NotNull(result);
            Assert.AreEqual(expectedPeriodsBeforeMilliseconds.Length, result.Ticks.Count);

            Assert.True(
                expectedPeriodsBeforeMilliseconds
                    .Select(x => TimeSpan.FromMilliseconds(x))
                    .SequenceEqual(
                        result
                            .Ticks
                            .Select(x => x.PeriodBefore)));
            Assert.True(
                expectedTickTypes
                    .SequenceEqual(
                        result
                            .Ticks
                            .Select(x => x.Type)));
        }

        [TestCase("  Metronome at 1s")]
        [TestCase("Metronome at 1s\n")]
        [TestCase("Metronome at")]
        [TestCase("Metronome at abc")]
        [TestCase("Metronomeat 1s")]
        [TestCase("Metronome\n at 1s")]
        [TestCase("Metronome at\n 1s")]
        [TestCase("Metronome at 1s,\n2s")]
        public void cannot_parse_incorrectly_formatted_input(string input)
        {
            var result = MetronomeActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}