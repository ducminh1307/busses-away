using UnityEngine;

namespace DucMinh
{
    public partial class AudioManager : SingletonBehavior<AudioManager>
    {
        private DelegatedPool<AudioSource> audioPool;

        public static void Init()
        {
            Instance.InitInternal();
        }
        private void InitInternal()
        {
            audioPool = new DelegatedPool<AudioSource>(CreateObjectPool, GetObjectPool, ReleaseObjectPool, DestroyObjectPool);
        }
    }
}