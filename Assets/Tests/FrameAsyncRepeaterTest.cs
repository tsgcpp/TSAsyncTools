using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System.Threading;
using TSAsyncTools;

namespace Tests
{
    public class FrameAsyncRepeaterTest
    {
        [UnityTest]
        public IEnumerator インスタンス生成後にEnabledをtrueにしない場合は処理が開始しないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            yield return null;

            // then
            Assert.False(target.Enabled);
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator インスタンス生成後にEnabledをtrueにした場合は処理が開始すること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return null;

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator ループ毎の遅延フレームが0でEnabledをtrueにした後に3フレーム後に処理が3回行われること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return null;
            yield return null;
            yield return null;

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(3, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledをtrueにしたあと即座にfalseにした場合に処理が行われないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            target.Enabled = false;
            yield return null;
            yield return null;  // 停止を確認したいため余分に待機

            // then
            Assert.False(target.Enabled);
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledをtrueにした数フレーム後にfalseにした場合に処理が停止すること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return null;
            yield return null;
            yield return null;
            target.Enabled = false;
            yield return null;

            // then
            Assert.False(target.Enabled);
            Assert.AreEqual(3, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledをtrueからfalseにして再度trueにした場合に処理が再開すること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return null;
            yield return null;
            yield return null;
            target.Enabled = false;
            yield return null;
            target.Enabled = true;
            yield return null;

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(4, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledを交互にtrueからfalseにして最終的にtrueにした場合に処理が重複しないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            target.Enabled = false;
            target.Enabled = true;
            target.Enabled = false;
            target.Enabled = true;
            yield return null;

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledをtrueにしてから即座にキャンセルした場合に処理が実施されないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            cts.Cancel();
            yield return null;
            yield return null;  // 停止を確認したいため余分に待機

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator 初回遅延フレーム数が指定通りに処理されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 5, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(0, callCount);

            yield return null;  // 5フレーム目
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator ループ毎の遅延フレーム数が指定通りに処理されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 3, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return null;
            yield return null;
            yield return null;

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(1, callCount);

            yield return null;  // 1 + 3フレーム目
            Assert.AreEqual(2, callCount);
        }

        [UnityTest]
        public IEnumerator すでにキャンセル済みの場合に処理が実施されないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            cts.Cancel();
            target.Enabled = true;
            yield return null;
            yield return null;  // 停止を確認したいため余分に待機

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledをtrueにしてから数フレーム後にキャンセルした場合に処理が停止すること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return null;
            yield return null;
            yield return null;
            cts.Cancel();

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(3, callCount);
            yield return null;
            yield return null;  // 停止を確認したいため余分に待機
            Assert.AreEqual(3, callCount);
        }

        [UnityTest]
        public IEnumerator キャンセルした後にEnabledをfalseからtrueにしても処理が再開しないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRepeater(action, 1, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return null;
            yield return null;
            yield return null;
            cts.Cancel();
            yield return null;
            target.Enabled = false;
            target.Enabled = true;
            yield return null;

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(3, callCount);
        }
    }
}
