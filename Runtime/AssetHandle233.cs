using UnityEngine;

namespace AssetLib233.Runtime
{
    public interface IAssetHandle233
    {
        bool IsDone { get; }
        float Progress { get; }
        Object RawAssetObject { get; }
        string Error { get; }
        void Release();
    }

    /// <summary>
    /// 资源加载句柄。独立维护状态，后续 Provider 接入 AB / Raw / Scene 后填充结果。
    /// </summary>
    public sealed class AssetHandle233<TObject> : IAssetHandle233 where TObject : Object
    {
        private readonly string _groupName;
        private readonly string _location;
        private TObject _assetObject;
        private string _error;
        private float _progress;
        private bool _isDone;
        private bool _isReleased;
        private System.Action _releaseCallback;

        public AssetHandle233(string groupName, string location)
        {
            _groupName = groupName;
            _location = location;
        }

        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(_groupName) && !string.IsNullOrEmpty(_location); }
        }

        public bool IsDone
        {
            get { return _isDone; }
        }

        public float Progress
        {
            get { return _progress; }
        }

        public TObject AssetObject
        {
            get { return _assetObject; }
        }

        public Object RawAssetObject
        {
            get { return _assetObject; }
        }

        public string Error
        {
            get { return _error; }
        }

        internal void SetProgress(float progress)
        {
            if (progress < 0f)
            {
                _progress = 0f;
                return;
            }

            if (progress > 1f)
            {
                _progress = 1f;
                return;
            }

            _progress = progress;
        }

        internal void SetReleaseCallback(System.Action releaseCallback)
        {
            _releaseCallback = releaseCallback;
        }

        public void SetResult(TObject assetObject)
        {
            _assetObject = assetObject;
            _progress = 1f;
            _isDone = true;
            if (_assetObject != null)
            {
                AssetLib233.Instance.RetainAsset(_groupName, _location, _assetObject);
            }
        }

        public void SetFailed(string error)
        {
            _error = error;
            _progress = 1f;
            _isDone = true;
        }

        public void Release()
        {
            if (_isReleased)
            {
                return;
            }

            _isReleased = true;
            _releaseCallback?.Invoke();
            _releaseCallback = null;
            if (_assetObject != null)
            {
                AssetLib233.Instance.ReleaseAsset(_groupName, _location);
            }

            _assetObject = null;
        }
    }
}
