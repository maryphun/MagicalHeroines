using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using static NovelEditor.NovelData.ParagraphData;

namespace NovelEditor
{
    /// <summary>
    /// NovelPlayerのサウンドの再生を管理するクラス
    /// </summary>
    internal class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource _BGM;
        [SerializeField] private AudioSource _SE;
        [SerializeField] private AudioSource _VOICE;

        private float _SEVolume;
        private float _BGMVolume;
        private float _VOICEVolume;

        CancellationTokenSource BGMcancellation = new CancellationTokenSource();
        CancellationTokenSource SEcancellation = new CancellationTokenSource();
        CancellationTokenSource SELoopCancellation = new CancellationTokenSource();
        CancellationTokenSource VOICEcancellation = new CancellationTokenSource();

        bool _isFading = false;
        bool _isLoopingSE = false;

        AudioClip voiceToPlay = null;

        /// <summary>
        /// 初期化用関数
        /// </summary>
        /// <param name="bgmVolume">BGMの初期音量</param>
        /// <param name="seVolume">SEの初期音量</param>
        internal void Init(float bgmVolume, float seVolume, float voiceVolume)
        {
            _BGM = gameObject.AddComponent<AudioSource>();
            _BGM.playOnAwake = false;
            _BGM.loop = true;

            _SE = gameObject.AddComponent<AudioSource>();
            _SE.playOnAwake = false;
            _SE.loop = true;

            _VOICE = gameObject.AddComponent<AudioSource>();
            _VOICE.playOnAwake = false;
            _VOICE.loop = false;


            SetSEVolume(seVolume);
            SetBGMVolume(bgmVolume);
            SetVoiceVolume(voiceVolume);
        }

        /// <summary>
        /// Dialogueに応じてBGMとSEを設定する
        /// </summary>
        /// <param name="data">次のセリフのデータ</param>
        internal void SetSound(Dialogue data)
        {
            if (data.BGMStyle != SoundStyle.UnChange)
            {
                SetBGM(data);
            }

            if (data.SEStyle != SoundStyle.UnChange)
            {
                SetSE(data);
            }

            // try to load character voice
            voiceToPlay = Resources.Load<AudioClip>("Audio/VOICE/" + data.localizationID);

            _VOICE.Stop();
        }

        /// <summary>
        /// Dialogueに応じてボイスを設定する
        /// </summary>
        /// <param name="data">次のセリフのデータ</param>
        internal void SetVoiceData(Dialogue data)
        {
            if (!string.IsNullOrEmpty(data.localizationID))
            {
                SetVoice(data);
            }
        }

        /// <summary>
        /// サウンドをミュートにする
        /// </summary>
        /// <param name="flag">ミュートにするかどうか</param>
        internal void SetMute(bool flag)
        {
            _BGM.mute = flag;
            _SE.mute = flag;
            _VOICE.mute = flag;
        }

        /// <summary>
        /// 全てのサウンドの再生を停止する
        /// </summary>
        internal void AllStop()
        {
            BGMcancellation.Cancel();
            SEcancellation.Cancel();
            VOICEcancellation.Cancel();
            _BGM.Stop();
            _SE.Stop();
            _VOICE.Stop();
            _isLoopingSE = false;
        }

        /// <summary>
        /// BGMを設定する
        /// </summary>
        /// <param name="data">次のセリフのデータ</param>
        void SetBGM(Dialogue data)
        {
            switch (data.BGMStyle)
            {
                case SoundStyle.Play:
                    Stop(_BGM, BGMcancellation);
                    BGMcancellation = new CancellationTokenSource();
                    SoundData soundData = new SoundData(data.BGM, data.BGMLoop, data.BGMCount, data.BGMSecond, data.BGMFadeTime, data.BGMEndFadeTime);
                    var t = Play(soundData, _BGMVolume, _BGM, BGMcancellation.Token);
                    break;
                case SoundStyle.Stop:
                    Stop(_BGM, BGMcancellation);
                    break;
            }
        }

        /// <summary>
        /// SEを設定する
        /// </summary>
        /// <param name="data">次のセリフのデータ</param>
        void SetSE(Dialogue data)
        {
            switch (data.SEStyle)
            {
                case SoundStyle.Play:
                    Stop(_SE, SEcancellation);
                    SEcancellation = new CancellationTokenSource();
                    SoundData soundData = new SoundData(data.SE, data.SELoop, data.SECount, data.SESecond, data.SEFadeTime, data.SEEndFadeTime);
                    var t = PlaySE(soundData, _SEVolume, _SE, SEcancellation.Token);
                    break;
                case SoundStyle.Stop:
                    var s = FadeStop(_SE, SEcancellation.Token);
                    SELoopCancellation.Cancel();
                    _isLoopingSE = false;
                    break;
            }
        }

        void SetVoice(Dialogue data)
        {
            if (voiceToPlay != null)
            {
                _VOICE.volume = _VOICEVolume;
                _VOICE.clip = voiceToPlay;
                _VOICE.Play();
            }
        }

        /// <summary>
        /// 指定したAudioSourceの再生を停止する
        /// </summary>
        /// <param name="player">停止したいAudioSource</param>
        /// <param name="cancel">停止したいAudioSourceの非同期処理のCancellationTokenSource</param>
        void Stop(AudioSource player, CancellationTokenSource cancel)
        {
            cancel.Cancel();
            player.Stop();
        }

        async UniTask<bool> FadeStop(AudioSource player, CancellationToken token)
        {
            await FadeVolume(player.volume, 0, 0.5f, player, token);
            Stop(player, SEcancellation);
            return true;
        }

        /// <summary>
        /// 指定したAudioSourceのサウンドを設定する
        /// </summary>
        /// <param name="data">サウンドのデータ</param>
        /// <param name="defaultVolume">再生したいAudioSourceの元々の音量</param>
        /// <param name="player">再生したいAudioSource</param>
        /// <param name="token">フェードの非同期処理に使用するCancellationToken</param>
        async UniTask<bool> Play(SoundData data, float defaultVolume, AudioSource player, CancellationToken token)
        {
            player.volume = defaultVolume;
            player.PlayOneShot(data.clip);
            await FadeVolume(0, defaultVolume, data.FadeTime, player, token);
            switch (data.Loop)
            {
                case LoopMode.Endless:
                    await UniTask.Delay((int)((data.clip.length - data.EndFadeTime  - data.FadeTime) * 1000), cancellationToken: token);
                    await FadeVolume(defaultVolume, 0, data.EndFadeTime, player, token);
                    SoundData newData = new SoundData(data.clip, data.Loop, data.Count, data.Second, 0.1f, data.EndFadeTime);
                    await Play(newData, defaultVolume, player, token);
                    break;
                case LoopMode.Count:
                    await UniTask.Delay((int)(data.clip.length * data.Count * 1000), cancellationToken: token);
                    await FadeVolume(defaultVolume, 0, data.EndFadeTime, player, token);
                    player.Stop();
                    break;
                case LoopMode.Second:
                    await UniTask.Delay((int)(data.Second * 1000), cancellationToken: token);
                    await FadeVolume(defaultVolume, 0, data.EndFadeTime, player, token);
                    player.Stop();
                    break;
            }
            return true;
        }


        /// <summary>
        /// 指定したAudioSourceのサウンドを設定する
        /// </summary>
        /// <param name="data">サウンドのデータ</param>
        /// <param name="defaultVolume">再生したいAudioSourceの元々の音量</param>
        /// <param name="player">再生したいAudioSource</param>
        /// <param name="token">フェードの非同期処理に使用するCancellationToken</param>
        async UniTask<bool> PlaySE(SoundData data, float defaultVolume, AudioSource player, CancellationToken token)
        {
            player.volume = defaultVolume;
            player.PlayOneShot(data.clip);

            switch (data.Loop)
            {
                case LoopMode.Endless:
                    // TODO: 機能していなさそう。要確認
                    _isLoopingSE = true;
                    SELoopCancellation = new CancellationTokenSource();
                    await UniTask.Delay((int)(data.clip.length * 1000), true, PlayerLoopTiming.Update, SELoopCancellation.Token);
                    if (_isLoopingSE)
                    {
                        await PlaySE(data, defaultVolume, player, token);
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// SEの音量を変更する
        /// </summary>
        /// <param name="seVolume">SEの新しい音量</param>
        internal async void SetSEVolume(float seVolume)
        {
            if (_isFading)
                await UniTask.WaitWhile(() => _isFading);
            _SEVolume = seVolume;
            _SE.volume = _SEVolume;
        }

        /// <summary>
        /// BGMの音量を変更する
        /// </summary>
        /// <param name="seVolume">BGMの新しい音量</param>
        internal async void SetBGMVolume(float bgmVolume)
        {
            if (_isFading)
                await UniTask.WaitWhile(() => _isFading);
            _BGMVolume = bgmVolume;
            _BGM.volume = _BGMVolume;
        }

        /// <summary>
        /// ボイスの音量を変更する
        /// </summary>
        /// <param name="voiceVolume">ボイスの新しい音量</param>
        internal void SetVoiceVolume(float voiceVolume)
        {
            Debug.Log("voice volume in audio player set to " + voiceVolume);
            _VOICEVolume = voiceVolume;
            _VOICE.volume = _VOICEVolume;
        }

        internal void PlayOneShot(AudioClip clip)
        {
            _SE.PlayOneShot(clip);
        }

        /// <summary>
        /// サウンドにフェードをかける
        /// </summary>
        /// <param name="from">最初の音量</param>
        /// <param name="dest">目標の音量</param>
        /// <param name="time">フェードにかける時間</param>
        /// <param name="player">再生したいAudioSource</param>
        /// <param name="token">フェードの非同期処理に使用するCancellationToken</param>
        /// <returns></returns>
        async UniTask<bool> FadeVolume(float from, float dest, float time, AudioSource player, CancellationToken token)
        {
            float value = 0;
            float volumeSpeed = 0.01f;

            if (time < 0.5)
            {
                volumeSpeed = 0.1f;
            }
            _isFading = true;
            player.volume = from;
            from = Mathf.Clamp(from, 0, 1);
            dest = Mathf.Clamp(dest, 0, 1);

            try
            {
                while (value < 1)
                {
                    player.volume = from + (dest - from) * value;
                    await UniTask.Delay(TimeSpan.FromSeconds(time * volumeSpeed), cancellationToken: token);
                    value += volumeSpeed;
                }
            }
            catch (OperationCanceledException)
            { return false; }
            _isFading = false;
            player.volume = dest;
            return true;
        }


        struct SoundData
        {
            public SoundData(AudioClip _clip, LoopMode _mode, int _count, float _second, float _fadeTime, float _endFadeTime)
            {
                clip = _clip;
                Loop = _mode;
                Count = _count;
                Second = _second;
                FadeTime = _fadeTime;
                EndFadeTime = _endFadeTime;
            }
            public AudioClip clip { get; private set; }
            public LoopMode Loop { get; private set; }
            public int Count { get; private set; }
            public float Second { get; private set; }
            public float FadeTime { get; private set; }
            public float EndFadeTime { get; private set; }
        }
    }
}