using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TSAsyncTools
{
    /// <summary>
    /// 遅延フレーム指定による非同期による処理
    /// </summary>
    public class FrameAsyncRunner : IRunner
    {
        private readonly Action _action;

        private readonly int _delayFrameCount;

        private readonly PlayerLoopTiming _timing;

        private readonly CancellationToken _cancellationToken;

        private bool _isRunning = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="action">ループ毎の処理</param>
        /// <param name="delayFrameCount">処理実行までの遅延フレーム数</param>
        /// <param name="timing">ループ処理を実施するタイミング(UniTaskを参照)</param>
        /// <param name="cancellationToken">キャンセル処理用CancellationToken</param>
        /// <summary>
        public FrameAsyncRunner(
            Action action,
            int delayFrameCount = 0,
            PlayerLoopTiming timing = PlayerLoopTiming.PreUpdate,
            CancellationToken cancellationToken = default) {
            _action = action;
            _delayFrameCount = delayFrameCount;
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
                await UniTask.DelayFrame(_delayFrameCount, _timing, _cancellationToken);
                _action();
            } finally {
                _isRunning = false;
            }
        }
    }
}