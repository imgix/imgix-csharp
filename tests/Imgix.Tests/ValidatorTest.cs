using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Imgix.Tests
{
    [TestFixture]
    public class ValidatorTest
    {
        private const int LessThanZero = -1;

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestValidateMinWidth()
        {
            // Test `ValidateMinWidth` throws when passed negative values.
            Validator.ValidateMinWidth(LessThanZero);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestValidateMaxWidth()
        {
            // Test `ValidateMaxWidth` throws when passed negative values.
            Validator.ValidateMaxWidth(LessThanZero);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestValidateRange()
        {
            // Test `ValidateRange` throws when passed mixed up
            // `begin` and `end` values.
            // Typically a typo'd call might look like:
            // `ValidateRange(400, 100)`
            // this validator seeks to prevent these kinds of errors.
            const int Begin = 400;
            const int End = 100;
            Validator.ValidateRange(Begin, End);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestValidateTolerance()
        {
            // Test `ValidateTolerance` throws when passed values
            // less than one percent.
            const double LessThanOnePercent = 0.001;
            Validator.ValidateTolerance(LessThanOnePercent);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestValidateNegativeWidthsThrows()
        {
            //Test `ValidateWidths` throws if a negative width value
            // has been found.
            int[] Widths = { 100, 200, 300, LessThanZero };
            Validator.ValidateWidths(Widths.ToList());
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestValidateWidthsNullArray()
        {
            // Test `ValidateWidths` throws if passed a `null` array.
            Validator.ValidateWidths(null);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestValidateToleranceEmptyList()
        {
            // Test `ValidateWidths` throws if passed an empty array.
            Validator.ValidateWidths(new List<int>());
        }
    }
}
