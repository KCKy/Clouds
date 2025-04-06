using System;
using System.Collections.Generic;
using System.Linq;

namespace Useful.Math
{
    /// <summary>
    /// Used for calculating a statistic of last <see cref="Length"/> entries in a series.
    /// </summary>
    /// <typeparam name="T">The type of value, which will be kept track of.</typeparam>
    public sealed class IntegrateWindowed<T>
    {
        readonly Queue<T> _queue;

        /// <summary>
        /// Length of the computing window.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="length">The length of the window to calculate minimum from.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="length"/> is non-positive.</exception>
        public IntegrateWindowed(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "The length of the window must be positive.");

            Length = length;
            _queue = new(length);
        }

        /// <summary>
        /// The statistic to be calculated over the window.
        /// </summary>
        /// <remarks>
        /// Is minimum by default.
        /// </remarks>
        public Func<IEnumerable<T>, T> Statistic { get; set; } = value => value.Min()!;

        /// <summary>
        /// Add next value from the series.
        /// </summary>
        /// <param name="value">The current value.</param>
        /// <returns>The current statistic from the latest window of size <see cref="Length"/>.</returns>
        public T Add(T value)
        {
            _queue.Enqueue(value);
            if (_queue.Count > Length)
                _queue.Dequeue();
        
            return Statistic(_queue);
        }
    }

    /// <summary>
    /// Calculates time weighted average of a value.
    /// It is calculated over a period of length <see cref="ResetTime"/> after that the collection is reset and a new average starts being calculated.
    /// </summary>
    public class TimeWeightedAverage
    {
        float _last = float.NaN;
        float _sum;
        float _weight;

        /// <summary>
        /// The length of the averaged period.
        /// </summary>
        public float ResetTime { get; set; } = 1;

        /// <summary>
        /// Update the average calculation.
        /// </summary>
        /// <param name="delta">Time passed since the last update.</param>
        /// <param name="amount">The amount to be recorded for this update.</param>
        /// <returns>Time weighted average of the last completed period.</returns>
        public float Update(float delta, float amount)
        {
            _sum += amount * delta;
            _weight += delta;

            if (_weight > ResetTime)
            {
                _last = _sum / _weight;
                _sum = 0;
                _weight = 0;
            }

            return _last;
        }
    }

    /// <summary>
    /// Calculates an average for a value.
    /// It is calculated over a period of length <see cref="ResetTime"/> after that the collection is reset and a new average starts being calculated.
    /// </summary>
    public class Average
    {
        float _last = float.NaN;
        float _sum;
        float _weight;
        long _count;

        /// <summary>
        /// The length of the averaged period.
        /// </summary>
        public float ResetTime { get; set; } = 1;

        /// <summary>
        /// Update the average calculation.
        /// </summary>
        /// <param name="delta">Time passed since the last update.</param>
        /// <param name="amount">The amount to be recorded for this update.</param>
        /// <returns>Average of the last completed period.</returns>
        public float Update(float delta, float amount)
        {
            _sum += amount;
            _count++;
            _weight += delta;

            if (_weight > ResetTime)
            {
                _last = _sum / _count;
                _sum = 0;
                _count = 0;
                _weight = 0;
            }

            return _last;
        }
    }
}
