using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Melomania.Utils
{
    public static class Extensions
    {
        public static int RoundToNearestTen(this double number) =>
            ((int)Math.Round(number / 10.0)) * 10;

        public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            void Process_Exited(object sender, EventArgs e)
            {
                taskCompletionSource.TrySetResult(true);
            }

            process.EnableRaisingEvents = true;
            process.Exited += Process_Exited;

            try
            {
                if (process.HasExited)
                {
                    return;
                }

                using (cancellationToken.Register(() => taskCompletionSource.TrySetCanceled()))
                {
                    await taskCompletionSource.Task;
                }
            }
            finally
            {
                process.Exited -= Process_Exited;
            }
        }
    }
}
