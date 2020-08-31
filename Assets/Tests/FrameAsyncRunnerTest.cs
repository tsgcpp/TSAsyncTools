using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System.Threading;
using TSAsyncTools;

namespace Tests
{
    public class FrameAsyncRunnerTest
    {
        [UnityTest]
        public IEnumerator Runをコールした後にActionが実施されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRunner(action, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return null;

            // then
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator Runをコールして即座にキャンセルした場合にActionが実施されないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRunner(action, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            cts.Cancel();
            yield return null;

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
            var target = new FrameAsyncRunner(action, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            cts.Cancel();
            target.Run();
            yield return null;

            // then
            Assert.AreEqual(0, callCount);
        }

        [UnityTest]
        public IEnumerator 同フレームに連続でRunをコールしてもActionは1回のみ実施されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRunner(action, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            target.Run();
            target.Run();
            yield return null;

            // then
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator Action完了後に再度Runをコールした場合に再びActionが実施されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRunner(action, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return null;
            target.Run();
            yield return null;
            target.Run();
            yield return null;

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
            var target = new FrameAsyncRunner(action, 0, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            for (int i = 0; i < 30; ++i) { yield return null; }
            target.Run();
            yield return null;

            // then
            Assert.AreEqual(2, callCount);
        }

        [UnityTest]
        public IEnumerator 遅延フレームを指定した場合に遅延フレーム後にActionが実施されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRunner(action, 3, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return null;
            yield return null;
            yield return null;

            // then
            Assert.AreEqual(1, callCount);
        }

        [UnityTest]
        public IEnumerator 遅延フレームを指定した場合に遅延フレーム前にActionが実施されないこと()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRunner(action, 3, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return null;
            yield return null;

            // then
            Assert.AreEqual(0, callCount);
        }


        [UnityTest]
        public IEnumerator 遅延フレームを指定してもAction完了後に再度Runをコールした場合に再びActionが実施されること()
        {
            // setup
            int callCount = 0;
            Action action = () => { callCount += 1; };
            var cts = new CancellationTokenSource();
            var target = new FrameAsyncRunner(action, 3, PlayerLoopTiming.PreUpdate, cts.Token);

            // when
            target.Run();
            yield return null;
            yield return null;
            yield return null;
            target.Run();
            yield return null;
            yield return null;
            yield return null;

            // then
            Assert.AreEqual(2, callCount);
        }
    }
}
