using System;
using System.Collections.Generic;
namespace Imgix
{
    public class Validator
    {
        private static readonly double ONE_PERCENT = 0.01;

        /// <summary>
        /// Validate `begin` width value is at least zero.
        /// </summary>
        /// <param name="begin">Beginning width value of a width-range.</param>
        /// <exception cref="Exception">Throws, if `begin` is less than zero.</exception>
        public static void ValidateMinWidth(int begin)
        {
            if (begin < 0)
            {
                throw new Exception("`begin` width value must be greater than zero");
            }
        }

        /// <summary>
        /// Validate `end` width value is at greater than or equal to zero.
        /// </summary>
        /// <param name="end">Ending width value of a width-range.</param>
        /// <exception cref="Exception">Throws, if `end` is less than zero.</exception>
        public static void ValidateMaxWidth(int end)
        {
            if (end < 0)
            {
                throw new Exception("`end` width value must be greater than zero");
            }
        }

        /// <summary>
        /// Validate `begin` and `end` represent a valid width-range.
        ///
        /// This validator is the composition of `validateMinWidth` and
        /// `validateMaxWidth`. It also adds a final constraint that
        /// `begin` be less than or equal to `end`
        /// </summary>
        /// <param name="begin">Beginning width value of a width-range</param>
        /// <param name="end">Ending width value of a width-range.</param>
        /// <exception cref="Exception">Throws, if a width range `begin`s after it `end`s.</exception>
        public static void ValidateRange(int begin, int end)
        {
            // Validate the minimum width, `begin`.
            ValidateMinWidth(begin);
            // Validate the maximum width, `end`.
            ValidateMaxWidth(end);

            // Ensure that the range is valid, ie. `begin <= end`.
            if (end < begin)
            {
                throw new Exception("`begin` width value must be less than `end` width value");
            }
        }

        /// <summary>
        /// Validate `tol`erance is at least `ONE_PERCENT`.
        /// </summary>
        /// <param name="tol">Tolerable amount of image width variation.</param>
        /// <exception cref="Exception">Throws, if `tol` is less than `ONE_PERCENT`..</exception>
        public static void ValidateTolerance(double tol)
        {
            String msg = "`tol`erance value must be greater than, " +
                "or equal to one percent, ie. >= 0.01";

            if (tol < ONE_PERCENT)
            {
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Validate that `begin`, `end`, and `tol` represent a valid srcset range.
        /// </summary>
        /// <param name="begin">Beginning width value of a width-range.</param>
        /// <param name="end">Ending width value of a width-range.</param>
        /// <param name="tol">Tolerable amount of image width variation.</param>
        /// <exception cref="Exception">Throws, if validation of `begin`, `end`,
        /// or `tol` fails.</exception>
        public static void ValidateMinMaxTol(int begin, int end, double tol)
        {
            ValidateRange(begin, end);
            ValidateTolerance(tol);
        }

        /// <summary>
        /// Validate `widths` array contains only positive values.
        /// </summary>
        /// <param name="widths">Integer array of positive image width values.</param>
        /// <exception cref="Exception">Throws, if `widths` is empty,`null`,
        /// or contains negative values.</exception>
        public static void ValidateWidths(List<int> widths)
        {
            if (widths == null)
            {
                throw new Exception("`widths` array cannot be `null`");
            }

            if (widths.Count == 0)
            {
                throw new Exception("`widths` array cannot be empty");
            }

            foreach (int w in widths)
            {
                if (w < 0)
                {
                    throw new Exception("width values in `widths` cannot be negative");
                }
            }
        }
    }
}
