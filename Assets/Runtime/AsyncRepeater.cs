using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TSAsyncTools
{
    /// <summary>
    /// 非同期によるループ処理
    /// </summary>
    public class AsyncRepeater : IActivatable
    {
        private readonly Action _action;

        private readonly int _firstDelayFrameCount;

        private readonly int _intervalDelayFrameCount;

        private readonly PlayerLoopTiming _timing;

        private readonly CancellationToken _cancellationToken;

        private bool _isRunning = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="action">ループ毎の処理</param>
        /// <param name="firstDelayFrameCount">ループ開始時の遅延フレーム数</param>
        /// <param name="intervalDelayFrameCount">ループ毎の遅延フレーム数</param>
        /// <param name="timing">ループ処理を実施するタイミング(UniTaskを参照)</param>
        /// <param name="cancellationToken">キャンセル処理用CancellationToken</param>
        public AsyncRepeater(
            Action action,
            int firstDelayFrameCount = 0,
            int intervalDelayFrameCount = 0,
            PlayerLoopTiming timing = PlayerLoopTiming.PreUpdate,
            CancellationToken cancellationToken = default) {
            _action = action;
            _firstDelayFrameCount = firstDelayFrameCount;
            _intervalDelayFrameCount = intervalDelayFrameCount;
            _timing = timing;
            _cancellationToken = cancellationToken;
        }

        private bool _enabled = false;
        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled == value) {
                    return;
                }

                _enabled = value;

                Loop();
            }
        }

        private void Loop()
        {
            if (_cancellationToken.IsCancellationRequested) {
                return;
            }

            if (_isRunning) {
                return;
            }
            _isRunning = true;

            LoopAsync().Forget();
        }

        private async UniTask LoopAsync()
        {
            try {
                await UniTask.DelayFrame(_firstDelayFrameCount, _timing, _cancellationToken);

                while (_enabled) {
                    _action();
                    await UniTask.DelayFrame(_intervalDelayFrameCount, _timing, _cancellationToken);
                }
            } finally {
                _isRunning = false;
            }
        }
    }
}