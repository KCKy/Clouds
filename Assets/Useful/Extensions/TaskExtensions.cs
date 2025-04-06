using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Useful.Extensions
{
    /// <summary>
    /// Utility extension methods for tasks.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Non-blocking method, which assures the given task completes successfully, otherwise an error is logged.
        /// </summary>
        /// <param name="task">Task to assure.</param>
        /// <example>
        /// <code>
        /// var task = FooAsync(bar);
        /// task.AssureSuccess(); // This does not block, we do not care when the task finishes
        /// </code>
        /// In cases where it is not desirable to await a Task, calling <see cref="AssureSuccess"/> makes sure potential errors or cancellations are logged.
        /// FooAsync therefore does some side effect but returns no direct result.
        /// </example>
        public static void AssureSuccess(this Task task) => AssureSuccessInternal(task);

        /// <summary>
        /// Non-blocking method, which assures the given task completes successfully or is canceled, otherwise an error is logged. 
        /// </summary>
        /// <param name="task">Task to assure.</param>
        /// <example>
        /// <code>
        /// var task = FooAsync(bar);
        /// task.AssureNoFault(); // This does not block, we do not care when the task finishes
        /// </code>
        /// In cases where it is not desirable to await a Task, calling <see cref="AssureNoFault"/> makes sure potential exceptions are logged.
        /// FooAsync therefore does some side effect but returns no direct result.
        /// </example>
        public static void AssureNoFault(this Task task) => AssureNoFaultInternal(task);

        static async void AssureSuccessInternal(Task task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                Debug.Log(Cancelled);
            }
            catch (Exception)
            {
                Debug.Log(Exception);
            }
        }

        static async void AssureNoFaultInternal(Task task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException) { }
            catch (Exception)
            {
                Debug.Log(Exception);
            }
        }

        const string Exception = "Assured task faulted with exception.";
        const string Cancelled = "Assured task was cancelled.";
    }
}
