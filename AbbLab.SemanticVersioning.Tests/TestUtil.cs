using System;
using System.Collections.Generic;

namespace AbbLab.SemanticVersioning.Tests
{
    internal static class TestUtil
    {
        public const SemanticOptions PseudoStrict = (SemanticOptions)int.MinValue;
        public static IEnumerable<object?[]> CreateFixtures<T>(T[] array)
            => Array.ConvertAll(array, static i => new object?[1] { i });
    }
}
