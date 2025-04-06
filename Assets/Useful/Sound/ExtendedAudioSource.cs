using System;
using UnityEngine;
using Useful.Collections;

namespace Useful.Sound
{
    public readonly struct PlayedCtx
    {
        readonly AudioSourceHandle _handle;
        readonly int _stamp;

        internal PlayedCtx(AudioSourceHandle handle)
        {
            _handle = handle;
            _stamp = handle.Stamp;
        }

        public bool Valid => _stamp == _handle.Stamp;

        public bool Playing
        {
            get
            {
                if (!Valid) return false;
                return _handle.Source.isPlaying;
            }
        }

        public float Pitch
        {
            get
            {
                if (!Valid) return float.NaN;
                return _handle.Source.pitch;
            }
            set
            {
                if (!Valid) return;
                _handle.Source.pitch = value;
            }
        }

        public void Stop()
        {
            if (!Valid) return;
            _handle.Source.Stop();
        }
    }

    public readonly struct PlayedLoopCtx
    {
        readonly AudioSourceHandle _handle;
        readonly int _stamp;

        internal PlayedLoopCtx(AudioSourceHandle handle)
        {
            _handle = handle;
            _stamp = handle.Stamp;
        }

        public bool Valid => _stamp == _handle.Stamp;

        public bool Playing
        {
            get
            {
                if (!Valid) return false;
                return _handle.Source.isPlaying;
            }
        }

        public float Pitch
        {
            get
            {
                if (!Valid) return float.NaN;
                return _handle.Source.pitch;
            }
            set
            {
                if (!Valid) return;
                _handle.Source.pitch = value;
            }
        }

        public void Stop()
        {
            if (!Valid) return;
            _handle.ExtendedSource.StopLooped(_handle);
        }
    }

    public readonly struct SfxContext
    {
        readonly AudioSourceHandle _handle;
        readonly ExtendedAudioSource _group;
        readonly int _stamp;

        internal SfxContext(AudioSourceHandle handle, ExtendedAudioSource group)
        {
            _handle = handle;
            _group = group;
            _stamp = handle.Stamp;
        }

        AudioSource GetSource()
        {
            if (_stamp != _handle.Stamp)
                throw new InvalidOperationException("This SfxContext has been invalidated and cannot be used.");
            return _handle.Source;
        }

        public SfxContext WithSeek(float seconds)
        {
            AudioSource source = GetSource();
            source.time = seconds;
            return this;
        }

        public SfxContext WithSeek(int samples)
        {
            AudioSource source = GetSource();
            source.timeSamples = samples;
            return this;
        }

        public SfxContext WithClip(AudioClip clip)
        {
            AudioSource source = GetSource();
            source.clip = clip;
            return this;
        }
        
        public PlayedLoopCtx PlayLooped()
        {
            _ = GetSource();
            _group.PlayLoopedAudioSource(_handle);
            return new(_handle);
        }

        public SfxContext WithPitch(float pitch)
        {
            AudioSource source = GetSource();
            source.pitch = pitch;
            return this;
        }

        public PlayedCtx Play()
        {
            AudioSource source = GetSource();
            _group.PlayAudioSource(_handle, source.clip.length);
            return new(_handle);
        }
    }
    
    class AudioSourceHandle
    {
        public int Stamp;
        public readonly AudioSource Source;
        public readonly ExtendedAudioSource ExtendedSource;

        public AudioSourceHandle(AudioSource source, ExtendedAudioSource extendedSource)
        {
            Source = source;
            Stamp = 0;
            ExtendedSource = extendedSource;
        }
    }

    public class ExtendedAudioSource : MonoBehaviour
    {
        PriorityQueue<AudioSourceHandle, float> _sources;
        [SerializeField] int initialCapacity = 32;
        [SerializeField] GameObject audioSourcePrototype;

        void Awake()
        {
            _sources = new(initialCapacity);

            for (int i = 0; i < initialCapacity; i++)
            {
                AudioSourceHandle src = CreateNewAudioSource();
                _sources.Enqueue(src, float.NegativeInfinity);
            }
        }

        AudioSourceHandle BorrowAudioSource()
        {
            if (_sources.TryPeek(out AudioSourceHandle selection, out _) && !selection.Source.isPlaying)
            {
                selection.Stamp++;
                return _sources.Dequeue();
            }
            
            foreach ((AudioSourceHandle handle, float _) in _sources.UnorderedItems)
            {
                if (handle.Source.isPlaying) continue;
                _sources.Remove(handle, out _, out _);
                handle.Stamp++;
                return handle;
            }

            Debug.LogWarning("Creating new Audio Source at runtime.");
            return CreateNewAudioSource();
        }

        public SfxContext Sfx()
        {
            AudioSourceHandle handle = BorrowAudioSource();
            return new(handle, this);
        }

        internal void PlayAudioSource(AudioSourceHandle handle, float length)
        {
            handle.Stamp++;
            handle.Source.Play();
            handle.Source.loop = false;
            _sources.Enqueue(handle, Time.time + length);
        }

        internal void PlayLoopedAudioSource(AudioSourceHandle handle)
        {
            handle.Stamp++;
            handle.Source.loop = true;
            handle.Source.Play();
        }

        internal void StopLooped(AudioSourceHandle handle)
        {
            handle.Stamp++;
            handle.Source.Stop();
            _sources.Enqueue(handle, float.NegativeInfinity);
        }
        
        AudioSourceHandle CreateNewAudioSource()
        {
            GameObject obj = Instantiate(audioSourcePrototype, transform, false);
            var source = obj.GetComponent<AudioSource>();
            source.playOnAwake = false;
            return new(source, this);
        }
    }
}
