using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System.Threading;
using TSAsyncTools;
using UnityEngine;

namespace Tests
{
    public class TimeAsyncRepeaterTest
    {
        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1.0f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1.0f;
        }

        [UnityTest]
        public IEnumerator インスタンス生成後にEnabledをtrueにしない場合は処理が開始しないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            yield return new WaitForSeconds(0.3f);

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
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return new WaitForSeconds(0.2f);

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledをtrueにした後3回分のループ待機した場合に処理が3回行われること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return new WaitForSeconds(0.41f);

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
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            target.Enabled = false;
            yield return new WaitForSeconds(0.3f);

            // then
            Assert.False(target.Enabled);
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledをtrueにした後の数ループ後にfalseにした場合に処理が停止すること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return new WaitForSeconds(0.41f);
            target.Enabled = false;
            yield return new WaitForSeconds(0.2f);

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
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return new WaitForSeconds(0.41f);
            target.Enabled = false;
            yield return new WaitForSeconds(0.2f);
            target.Enabled = true;
            yield return new WaitForSeconds(0.41f);
            yield return null;

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(6, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledを交互にtrueからfalseにして最終的にtrueにした場合に処理が重複しないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            target.Enabled = false;
            target.Enabled = true;
            target.Enabled = false;
            target.Enabled = true;
            yield return new WaitForSeconds(0.2f);

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
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            cts.Cancel();
            yield return new WaitForSeconds(0.5f);

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator 初回遅延秒数が指定通りに処理されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.5f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return new WaitForSeconds(0.49f);

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(0, callCount);

            yield return new WaitForSeconds(0.01f);  // 0.5秒後
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator ループ毎の遅延秒数が指定通りに処理されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.1f, 0.3f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return new WaitForSeconds(0.39f);

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(1, callCount);

            yield return new WaitForSeconds(0.02f);  // 0.1 + 0.3秒後
            Assert.AreEqual(2, callCount);
        }

        [UnityTest]
        public IEnumerator すでにキャンセル済みの場合に処理が実施されないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            cts.Cancel();
            target.Enabled = true;
            yield return new WaitForSeconds(0.5f);

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator Enabledをtrueにしてから数ループ後にキャンセルした場合に処理が停止すること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return new WaitForSeconds(0.5f);
            cts.Cancel();

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(3, callCount);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(3, callCount);
        }

        [UnityTest]
        public IEnumerator キャンセルした後にEnabledをfalseからtrueにしても処理が再開しないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;
            yield return new WaitForSeconds(0.5f);
            cts.Cancel();
            yield return new WaitForSeconds(0.3f);
            target.Enabled = false;
            target.Enabled = true;
            yield return new WaitForSeconds(0.5f);

            // then
            Assert.True(target.Enabled);
            Assert.AreEqual(3, callCount);
        }

        [UnityTest]
        public IEnumerator ignoreTimeScaleがfalseの場合にTimeScaleが考慮されること()
        {
            // setup
            Time.timeScale = 0.5f;
            yield return null;

            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;

            // then
            yield return new WaitForSecondsRealtime(0.38f);
            Assert.AreEqual(0, callCount);
            yield return new WaitForSecondsRealtime(0.02f);
            Assert.AreEqual(1, callCount);
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.AreEqual(3, callCount);
        }

        [UnityTest]
        public IEnumerator ignoreTimeScaleがtrueの場合にTimeScaleが無視されること()
        {
            // setup
            Time.timeScale = 0.5f;
            yield return null;

            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRepeater(action, 0.2f, 0.1f, true, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Enabled = true;

            // then
            yield return new WaitForSecondsRealtime(0.18f);
            Assert.AreEqual(0, callCount);
            yield return new WaitForSecondsRealtime(0.02f);
            Assert.AreEqual(1, callCount);
            yield return new WaitForSecondsRealtime(0.22f);
            Assert.AreEqual(3, callCount);
        }
    }
}
