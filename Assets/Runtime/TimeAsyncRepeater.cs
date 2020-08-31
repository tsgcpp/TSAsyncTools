using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TSAsyncTools
{
    /// <summary>
    /// 遅延時間指定による非同期によるループ処理
    /// </summary>
    public class TimeAsyncRepeater : IActivatable
    {
        private readonly Action _action;

        private readonly TimeSpan _firstDelaySeconds;

        private readonly TimeSpan _intervalDelaySeconds;

        private readonly bool _ignoreTimeScale;

        private readonly PlayerLoopTiming _timing;

        private readonly CancellationToken _cancellationToken;

        private bool _isRunning = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="action">ループ毎の処理</param>
        /// <param name="firstDelaySeconds">ループ開始時の遅延秒数</param>
        /// <param name="intervalDelaySeconds">ループ毎の遅延秒数</param>
        /// <param name="ignoreTimeScale">Time.timeScaleの値を無視設定(trueであれば無視)</param>
        /// <param name="timing">ループ処理を実施するタイミング(UniTaskを参照)</param>
        /// <param name="cancellationToken">キャンセル処理用CancellationToken</param>
        public TimeAsyncRepeater(
            Action action,
            float firstDelaySeconds = 0f,
            float intervalDelaySeconds = 0f,
            bool ignoreTimeScale = false,
            PlayerLoopTiming timing = PlayerLoopTiming.PreUpdate,
            CancellationToken cancellationToken = default) {
            _action = action;
            _firstDelaySeconds = TimeSpan.FromSeconds((double)firstDelaySeconds);
            _intervalDelaySeconds = TimeSpan.FromSeconds((double)intervalDelaySeconds);
            _ignoreTimeScale = ignoreTimeScale;
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
                await UniTask.Delay(
                    _firstDelaySeconds,
                    _ignoreTimeScale,
                    _timing,
                    _cancellationToken);

                while (_enabled) {
                    _action();
                    await UniTask.Delay(
                        _intervalDelaySeconds,
                        _ignoreTimeScale,
                        _timing,
                        _cancellationToken);
                }
            } finally {
                _isRunning = false;
            }
        }
    }
}