using UnityEngine;

namespace AssetLib233.Runtime
{
    /// <summary>
    /// 资源加载句柄。独立维护状态，后续 Provider 接入 AB / Raw / Scene 后填充结果。
    /// </summary>
    public sealed class AssetHandle233<TObject> where TObject : Object
    {
        private readonly string _groupName;
        private readonly string _location;
        private TObject _assetObject;
        private string _error;
        private float _progress;
        private bool _isDone;

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

        public string Error
        {
            get { return _error; }
        }

        public void SetResult(TObject assetObject)
        {
            _assetObject = assetObject;
            _progress = 1f;
            _isDone = true;
        }

        public void SetFailed(string error)
        {
            _error = error;
            _isDone = true;
        }

        public void Release()
        {
            _assetObject = null;
        }
    }
}
