using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
namespace TapPatterns
{
    public class TapPatternManager
    {
        #region launch methods
        public void Launch()
        {
            LaunchMiningMethodLocalWithTasks();
        }

        public async Task LaunchAsync()
        {
            await LaunchMiningMethodWithCancellationAsync();
        }


        public void LaunchMiningMethod()
        {
            var result = RentTimeOnMiningServer("SecretToken", 4, out double elapsedSeconds);
            Console.WriteLine($"mining result:{result}");
            Console.WriteLine($"Elapsed seconds:{elapsedSeconds}");
        }

        public async Task LaunchMiningMethodAsync()
        {
            MiningResultDto result = await RentTimeOnMiningServerAsync("SecretToken", 4);
            Console.WriteLine($"mining result: {result.MiningText}");
            Console.WriteLine($"Elapsed seconds: {result.ElapsedSeconds:N}");
        }

        public void LaunchMiningMethodLocalWithTasks()
        {
            var localMiningTaskList = new List<Task<MiningResultDto>>();
            for (int i = 0; i < 3; i++)
            {
                Task<MiningResultDto> task = Task.Run(() => RentTimeOnLocalMiningServer("SecretToken", 4));
                localMiningTaskList.Add(task);
            }
            var localMiningArray = localMiningTaskList.ToArray();
            Task.WaitAll(localMiningArray);
            foreach (var task in localMiningArray)
            {
                Console.WriteLine($"mining result: {task.Result.MiningText}");
                Console.WriteLine($"Elapsed seconds: {task.Result.ElapsedSeconds:N}");
            }
        }

        public async Task LaunchMiningMethodLocalAsync()
        {
            MiningResultDto result = await RentTimeOnLocalMiningServerTask("SecretToken", 5);
            Console.WriteLine($"mining result: {result.MiningText}");
            Console.WriteLine($"Elapsed seconds: {result.ElapsedSeconds:N}");
        }

        public async Task LaunchMiningMethodWithCancellationAsync()
        {
            var cts = new CancellationTokenSource(1000);
            Task<MiningResultDto> asyncTask = RentTimeOnMiningServerAsync("SecretToken", 4, cts.Token);
            try
            {
                MiningResultDto result = await asyncTask;
                Console.WriteLine($"mining result: {result.MiningText}");
                Console.WriteLine($"Elapsed seconds: {result.ElapsedSeconds:N}");
                Console.WriteLine("end");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.GetType().ToString());
                Console.WriteLine($"Task status: {asyncTask.Status}");
            }
        }

        #endregion

        #region TAP advanced
        public async Task<MiningResultDto> RentTimeOnMiningServerAsync(string authToken, int requestedAmount, CancellationToken cancellationToken)
        {
            if (!AuthorizeTheToken(authToken))
            {
                throw new Exception("Failed Authorization");
            }
            Thread.Sleep(1500);
            var result = new MiningResultDto();
            var startTime = DateTime.UtcNow;
            var asyncTask = CallCoinServiceAsync(requestedAmount);
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            var coinResult = await asyncTask;
            var elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
            result.ElapsedSeconds = elapsedSeconds;
            result.MiningText = coinResult;
            return result;
        }

        #endregion

        #region Task 2 - introduction
        public string RentTimeOnMiningServer(string authToken, int requestedAmount, out Double elapsedSeconds)
        {
            elapsedSeconds = 0;
            if (!AuthorizeTheToken(authToken))
            {
                throw new Exception("Failed Authorization");
            }
            var startTime = DateTime.UtcNow;
            var coinResult = CallCoinService(requestedAmount);
            elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
            return coinResult;
        }

        public async Task<MiningResultDto> RentTimeOnMiningServerAsync(string authToken, int requestedAmount)
        {
            if (!AuthorizeTheToken(authToken))
            {
                throw new Exception("Failed Authorization");
            }
            var result = new MiningResultDto();
            var startTime = DateTime.UtcNow;
            var coinResult = await CallCoinServiceAsync(requestedAmount);
            var elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
            result.ElapsedSeconds = elapsedSeconds;
            result.MiningText = coinResult;
            return result;
        }

        public MiningResultDto RentTimeOnLocalMiningServer(string authToken, int requestedIterations)
        {
            if (!AuthorizeTheToken(authToken))
            {
                throw new Exception("Failed Authorization");
            }
            var result = new MiningResultDto();
            var startTime = DateTime.UtcNow;
            var coinAmount = MineAsyncCoinsWithNthRoot(requestedIterations);
            result.ElapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
            result.MiningText = $"You've got {coinAmount:N} AsyncCoin!";
            return result;
        }

        public Task<MiningResultDto> RentTimeOnLocalMiningServerTask(string authToken, int requestedIterations)
        {
            if (!AuthorizeTheToken(authToken))
            {
                throw new Exception("Failed Authorization");
            }
            var tcs = new TaskCompletionSource<MiningResultDto>();
            var result = new MiningResultDto();
            var startTime = DateTime.UtcNow;
            var localMiningTaskList = new List<Task<MiningResultDto>>();
            for (int i = 0; i < 3; i++)
            {
                Task<MiningResultDto> task = Task.Run(() => RentTimeOnLocalMiningServer("SecretToken", 4));
                localMiningTaskList.Add(task);
            }
            var localMiningArray = localMiningTaskList.ToArray();
            Task.WaitAll(localMiningArray);
            foreach (var task in localMiningArray)
            {
                result.MiningText += task.Result.MiningText + Environment.NewLine;
            }
            result.ElapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
            tcs.SetResult(result);
            return tcs.Task;
        }

        #endregion

        #region base methods
        private string CallCoinService(int howMany)
        {
            var uri = new Uri($"https://asynccoinfunction.azurewebsites.net/api/asynccoin/{howMany}");
            var webClient = new WebClient();
            var result = webClient.DownloadString(uri);
            return result;
        }

        private async Task<string> CallCoinServiceAsync(int howMany)
        {
            var uri = new Uri($"https://asynccoinfunction.azurewebsites.net/api/asynccoin/{howMany}");
            var webClient = new WebClient();
            var result = await webClient.DownloadStringTaskAsync(uri);
            return result;
        }

        private async Task CallCoinServiceNoResponseAsync(int howMany)
        {
            for (int i = 0; i < howMany; i++)
            {
                await Task.Delay(1000);
            }
        }

        private double MineAsyncCoinsWithNthRoot(int iterationMultiplier)
        {
            double allCoins = 0;
            for (int i = 1; i < iterationMultiplier * 2500; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    Math.Pow(i, 1.0 / j);
                    allCoins += .000001;
                }
            }
            return allCoins;
        }

        private Boolean AuthorizeTheToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}
