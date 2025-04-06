using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Useful.Math
{
    /// <summary>
    /// Provides a way to tween discrete keyframes of entities for drawing.
    /// </summary>
    /// <example>
    /// <code>
    /// Lerper&lt;Entity&gt; lerper_ = new();
    ///
    /// void Initialize()
    /// {
    ///     lerper_.OnEntityDraw += HandleEntityDraw;
    /// }
    ///
    /// void HandleEntityDraw(Entity previous, Entity current, float t)
    /// {
    ///     previous.DrawTweenedTo(current, t); // Draw an intermediate state from previous to current e.g. Lerp(previous, current, t)
    /// }
    ///
    /// void Simulate()
    /// {
    ///     const float simulationDelta = //...
    ///     // State update code here
    /// 
    ///     Entity entity = // ...
    ///     int entityId = entity.Id;
    ///
    ///     lerper_.AddEntity(entityId, entity);
    ///
    ///     // Do this for all relevant entities
    ///
    ///     lerper_.NextFrame(simulationDelta); // Finish frame production
    /// }
    ///
    /// void Draw(float delta)
    /// {
    ///     lerper_.Draw(delta);
    /// 
    ///     // Do other rendering.
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// <pare>
    /// <see cref="Lerper{T}"/> works by keeping a frame queue (queue of frames which were not yet displayed by drawing),
    /// and modifying playback speed to keep the queue non-empty. For this it keeps a weighted average of the queue size over time.
    /// Needed speed coefficient is deduced from this average. To disregard old information an exponential weight function is used for calculating
    /// the average. To change the weight function distribution see <see cref="Lerper{T}.WindowFunctionMedian"/>. To change the target average queue size see <see cref="Lerper{T}.FrameCountTarget"/>.
    /// </pare>
    /// <pare>
    /// Threading model: it is safe to call <see cref="AddEntity"/> and <see cref="NextFrame"/> in a single thread and <see cref="Draw"/> concurrently in a different thread.
    /// There can be a single synchronized producer of data to draw, and a single synchronized consumer which draws the produced data, i.e. a simulation thread and a draw thread.
    /// </pare>
    /// </remarks>
    /// <typeparam name="T">The type of the state to be interpolated.</typeparam>
    public sealed class Lerper<T>
    {
        struct Frame
        {
            public Frame(Dictionary<int, T> idToEntity, float length)
            {
                IdToEntity = idToEntity;
                Length = length;
            }

            public readonly Dictionary<int, T> IdToEntity;
            public float Length;
        }

        readonly Queue<Frame> _frames = new();

        Frame _collectedFrame = new(new(), 0); // Frame which is currently being constructed.

        Frame _currentFrame = new(new(), 0); // The current frame we are lerping from.

        /// <summary>
        /// Collect entity state for the current frame generation.
        /// </summary>
        /// <param name="id">Unique id of the entity.</param>
        /// <param name="value">State of the entity.</param>
        public void AddEntity(int id, T value) => _collectedFrame.IdToEntity.Add(id, value);

        /// <summary>
        /// End current frame generation. Puts the collected frame onto the interpolation queue. Starts collection of the next frame.
        /// </summary>
        /// <param name="length">The time this collected frame is supposed to take.</param>
        public void NextFrame(float length)
        {
            _collectedFrame.Length = length;
            _frames.Enqueue(_collectedFrame);
            DictionaryPool<int, T>.Get(out var dict);
            _collectedFrame = new(dict, 0);
        }

        // Seconds we spend drawing the current frame
        float _currentFrameTime;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Lerper()
        {
            WindowFunctionMedian = 0.05f;
        }

        /// <summary>
        /// The median of the average weight windowing function i.e. the amount of time which accounts for the recent 50 % of the weight function.
        /// </summary>
        public float WindowFunctionMedian
        {
            get => MathF.Log(2, _windowFuncBase);
            init
            {
                if (!float.IsFinite(value) || value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be a positive real number.");
                _windowFuncBase = MathF.Pow(2, 1 / value);
            }
        }

        /// <summary>
        /// The target average of number of frames in the frame queue.
        /// </summary>
        public float FrameCountTarget { get; init; } = 1.5f;

        // Previously calculated average number of frames in the queue
        float _previousAverage; 

        // Previous count of frames in the queue
        int _previousCount;

        readonly float _windowFuncBase;

        float GetAverage(float delta)
        {
            int frameCount = _previousCount;
            _previousCount = _frames.Count;

            float weight = MathF.Pow(_windowFuncBase, delta);

            float updatedAverage = (_previousAverage + frameCount * (weight - 1)) / weight;

            _previousAverage = updatedAverage;
            return updatedAverage;
        }

        float GetSpeed(float delta) => GetAverage(delta) / FrameCountTarget;

        (float t, Frame? targetFrame) UpdateFrameOffset(float delta)
        {
            // Moves the lerper by given delta, switches to next frame if available.

            _currentFrameTime += delta * GetSpeed(delta);

            while (true)
            {
                if (!_frames.TryPeek(out Frame targetFrame))
                {
                    return (0, null);
                }
            
                if (_currentFrameTime < targetFrame.Length)
                {
                    float progression = _currentFrameTime / targetFrame.Length;
                    return (progression, targetFrame);
                }
            
                _currentFrameTime -= targetFrame.Length;
            
                _currentFrame.IdToEntity.Clear();
                DictionaryPool<int, T>.Release(_currentFrame.IdToEntity);
                _currentFrame = targetFrame;
                _frames.TryDequeue(out _);
            }
        }

        void DrawProper(float t, Dictionary<int, T> idToTarget, EntityDraw onEntityDraw)
        {
            foreach ((int id, T from) in _currentFrame.IdToEntity)
            {
                if (!idToTarget.TryGetValue(id, out T target))
                    continue;

                onEntityDraw(from, target, t);
            }
        }

        void DrawImproper(float t, EntityDraw onEntityDraw)
        {
            foreach ((_, T from) in _currentFrame.IdToEntity)
                onEntityDraw(from, from, t);
        }

        /// <summary>
        /// Draw with given time progression.
        /// </summary>
        /// <param name="delta">The amount of time passed since the last draw.</param>
        public void Draw(float delta)
        {
            var result = UpdateFrameOffset(delta);
            if (OnEntityDraw is not { } onEntityDraw)
                return;

            if (result is (_, { } target))
                DrawProper(result.t, target.IdToEntity, onEntityDraw);
            else
                DrawImproper(result.t, onEntityDraw);
        }

        /// <summary>
        /// Signal to draw an entity.
        /// </summary>
        /// <param name="previous">Entity state for frame which just exited the queue.</param>
        /// <param name="current">Entity state for frame which is at the end of the queue.</param>
        /// <param name="t">Weight in range <c>[0,1]</c> defining the transition from <paramref name="previous"/> to <paramref name="current"/>.
        /// 0 means full <paramref name="previous"/>, 1 full <paramref name="current"/>.</param>
        /// <remarks>
        /// If the queue is empty <paramref name="previous"/> equals <paramref name="current"/>.
        /// </remarks>
        public delegate void EntityDraw(T previous, T current, float t);

        /// <summary>
        /// Called on each draw call for each valid entity.
        /// </summary>
        /// <remarks>
        /// This event is raised only within the corresponding <see cref="Draw"/> call.
        /// </remarks>
        public event EntityDraw OnEntityDraw;
    }
}
