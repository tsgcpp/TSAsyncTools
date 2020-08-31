using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TSAsyncTools
{
    /// <summary>
    /// 遅延時間指定による非同期による処理
    /// </summary>
    public class TimeAsyncRunner : IRunner
    {
        private readonly Action _action;

        private readonly TimeSpan _delayTimeSpan;

        private readonly bool _ignoreTimeScale;

        private readonly PlayerLoopTiming _timing;

        private readonly CancellationToken _cancellationToken;

        private bool _isRunning = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="action">ループ毎の処理</param>
        /// <param name="delaySeconds">処理実行までの遅延秒数</param>
        /// <param name="ignoreTimeScale">Time.timeScaleの値を無視設定(trueであれば無視)</param>
        /// <param name="timing">ループ処理を実施するタイミング(UniTaskを参照)</param>
        /// <param name="cancellationToken">キャンセル処理用CancellationToken</param>
        /// <summary>
        public TimeAsyncRunner(
            Action action,
            float delaySeconds = 0f,
            bool ignoreTimeScale = false,
            PlayerLoopTiming timing = PlayerLoopTiming.PreUpdate,
            CancellationToken cancellationToken = default) {
            _action = action;
            _delayTimeSpan = TimeSpan.FromSeconds((double)delaySeconds);
            _ignoreTimeScale = ignoreTimeScale;
            _timing = timing;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// 処理の予約
        /// 
        /// 処理実施までに複数回コールしても、指定のタイミングに1回のみ実施される。
        /// </summary>
        public void Run()
        {
            if (_cancellationToken.IsCancellationRequested) {
                return;
            }

            if (_isRunning) {
                return;
            }
            _isRunning = true;

            RunAsync().Forget();
        }

        private async UniTask RunAsync()
        {
            try {
                await UniTask.Delay(
                    _delayTimeSpan,
                    _ignoreTimeScale,
                    _timing,
                    _cancellationToken);
                _action();
            } finally {
                _isRunning = false;
            }
        }
    }
}