using UnityEngine;

namespace Useful.Extensions
{
    public static class AudioSourceExtensions
    {
        public static void SetPlaying(this AudioSource source, bool state)
        {
            if (source.isPlaying == state)
                return;

            if (state)
                source.Play();
            else
                source.Stop();
        }
    }
}
