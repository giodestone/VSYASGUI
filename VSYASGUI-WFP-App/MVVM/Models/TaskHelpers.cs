using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using VSYASGUI_CommonLib;
using VSYASGUI_CommonLib.ResponseObjects;

namespace VSYASGUI_WFP_App.MVVM.Models
{
    /// <summary>
    /// Contains some helpers for creating request tasks for <see cref="ApiConnection"/>.
    /// </summary>
    internal static class TaskHelpers
    {
        /// <summary>
        /// Whether a specified task is running, and the <see cref="ApiConnection.Instance"/> is not null.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static bool IsTaskRunning(Task? task)
        {
            return ApiConnection.Instance != null && (task != null && !task.IsCompleted);
        }

        /// <summary>
        /// Try to make a request in a wrapped way with <see cref="ApiConnection.RequestApiInfoJson{TExpectedResponse}(RequestBase, CancellationToken)"/>.
        /// </summary>
        /// <typeparam name="TResponse">Type of the expected response.</typeparam>
        /// <param name="baseTask">Where the launched task gets stored.</param>
        /// <param name="cancellationTokenSource">Cancellation token source. Will be initialised if null.</param>
        /// <param name="request">The request to send. Will auto-populate <see cref="RequestBase.ApiKey"/> with <see cref="ApiConnection.RequestApiInfoJson{TExpectedResponse}(RequestBase, CancellationToken)"/>.</param>
        /// <param name="continuationFunction">Function to call when the request completes. Invoked by the main thread's dispatcher, as defined by <see cref="Application.Current"/>.</param>
        /// <returns><c>true</c> if the request was made, <c>false</c> if it cannot be made either due to an exception or outcome of <see cref="IsTaskRunning(Task)"/>.</returns>
        public static Error TryMakeRequest<TResponse>(ref Task<ApiResponse<TResponse>>? baseTask, CancellationTokenSource? cancellationTokenSource, ApiRequest request, Action<ApiResponse<TResponse>> continuationFunction) where TResponse : ResponseBase, new()
        {
            if (TaskHelpers.IsTaskRunning(baseTask))
                return Error.RequestAlreadyInProgress;

            try
            {
                if (cancellationTokenSource == null)
                    cancellationTokenSource = new CancellationTokenSource();

                baseTask = ApiConnection.Instance.RequestApiInfoJson<TResponse>(request, cancellationTokenSource.Token);
                baseTask.ContinueWith(task => Application.Current.Dispatcher.BeginInvoke(continuationFunction, task.Result));
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Unable to invoke task due to an exception.");
                Console.WriteLine(e.Message);
                return Error.General;
            }

            return Error.Ok;
        }
    }
}
