﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NetGore.Tests
{
    [TestFixture]
    public class GrhIndexTests
    {
        [Test]
        public void CreateTest()
        {
            new GrhIndex(5);
            new GrhIndex(GrhIndex.MinValue);
            new GrhIndex(GrhIndex.MaxValue);
            Assert.Throws<ArgumentOutOfRangeException>(() => new GrhIndex((int)GrhIndex.Invalid));
        }

        [Test]
        public void IsInvalidTest()
        {
            Assert.IsTrue(GrhIndex.Invalid.IsInvalid);
            Assert.IsTrue(new GrhIndex().IsInvalid);
        }

        [Test]
        public void EqualsTest()
        {
            Assert.AreEqual(new GrhIndex(5), new GrhIndex(5));
            Assert.IsTrue(new GrhIndex(5) == new GrhIndex(5));
            Assert.IsTrue(new GrhIndex(5).Equals(new GrhIndex(5)));
        }

        [Test]
        public void NotEqualTest()
        {
            Assert.AreNotEqual(new GrhIndex(5), new GrhIndex(6));
            Assert.IsTrue(new GrhIndex(5) != new GrhIndex(6));
            Assert.IsFalse(new GrhIndex(5).Equals(new GrhIndex(6)));
        }
    }
}