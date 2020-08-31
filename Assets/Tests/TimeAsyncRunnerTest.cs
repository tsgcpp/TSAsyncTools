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
    public class TimeAsyncRunnerTest
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
        public IEnumerator Runをコールした後に指定した遅延秒後にActionが実施されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRunner(action, 0.3f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return new WaitForSeconds(0.3f);

            // then
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator Runをコールした後に指定した遅延秒前にはActionが実施されないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRunner(action, 0.3f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return new WaitForSeconds(0.2f);

            // then
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator Runをコールして即座にキャンセルした場合にActionが実施されないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRunner(action, 0.3f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            cts.Cancel();
            yield return new WaitForSeconds(0.3f);

            // then
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator すでにキャンセル済みの場合にRunをコールしてもActionが実施されないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRunner(action, 0.3f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            cts.Cancel();
            target.Run();
            yield return new WaitForSeconds(0.3f);

            // then
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator 指定遅延秒までに連続でRunをコールしてもActionは1回のみ実施されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRunner(action, 0.3f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return new WaitForSeconds(0.1f);
            target.Run();
            target.Run();
            yield return new WaitForSeconds(0.1f);
            target.Run();
            yield return new WaitForSeconds(0.2f);  // 余分に待機

            // then
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator Action完了直後に再度Runをコールした場合に再びActionが実施されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRunner(action, 0.3f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return new WaitForSeconds(0.3f);
            target.Run();
            yield return new WaitForSeconds(0.3f);
            target.Run();
            yield return new WaitForSeconds(0.3f);

            // then
            Assert.AreEqual(3, callCount);
        }

        [UnityTest]
        public IEnumerator コール後に間隔を空けてコールしても次点のActionが実施されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new TimeAsyncRunner(action, 0.3f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return new WaitForSeconds(0.5f);
            target.Run();
            yield return new WaitForSeconds(0.3f);

            // then
            Assert.AreEqual(2, callCount);
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
            var target = new TimeAsyncRunner(action, 0.3f, false, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();

            // then
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.AreEqual(0, callCount);
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.AreEqual(1, callCount);
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
            var target = new TimeAsyncRunner(action, 0.3f, true, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();

            // then
            yield return new WaitForSecondsRealtime(0.2f);  // WaitForSecondsRealtimeの精度が悪いため誤差を多めに確保
            Assert.AreEqual(0, callCount);
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.AreEqual(1, callCount);
        }
    }
}
