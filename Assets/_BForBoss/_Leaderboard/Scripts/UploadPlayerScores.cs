using System;
using Sirenix.Utilities;
using UnityEngine;

namespace BForBoss
{
    public class UploadPlayerScores
    {
        public struct PlayerPrefKey
        {
            public const string UserName = "UserName";
            public const string Timer = "Timer";
            public const string Input = "Input";
            public const string ShouldUpload = "ShouldUpload";
        }

        private const int MaxNumberOfRetries = 3;
        private readonly ILeaderboardPostEndPoint _endpoint = null;
        private int _numberOfRetries = 0;

        private string _username = "";
        private float _time = float.MaxValue;
        private string _input = "";

        public event Action StartUploading;
        public event Action StopLoading;

        public event Action<float> OnTimeChanged;
        public event Action<string> OnInputChanged;

        public string UserName
        {
            get => _username;

            set
            {
                PlayerPrefs.SetString(PlayerPrefKey.UserName, value);
                _username = value;
            }
        }

        public float Time
        {
            get => _time;
            set
            {
                PlayerPrefs.SetFloat(PlayerPrefKey.Timer, value);
                _time = value;
                OnTimeChanged?.Invoke(_time);
            }
        }

        public string Input
        {
            get => _input;
            set
            {
                PlayerPrefs.SetString(PlayerPrefKey.Input, value);
                _input = value;
                OnInputChanged?.Invoke(value);
            }
        }
        private bool ShouldUploadScores => PlayerPrefs.GetInt(PlayerPrefKey.ShouldUpload, 0) == 1;

        public UploadPlayerScores(ILeaderboardPostEndPoint endpoint = null)
        {
            _endpoint = endpoint ?? new DreamloSendScoreEndPoint();
            _endpoint.OnSuccess += HandleEndPointOnSuccess;
            _endpoint.OnFail += HandleEndPointOnFail;

            UploadIfPossible();
        }

        public void UploadIfPossible()
        {
            if (CanUpload())
            {
                Upload();
                StartUploading?.Invoke();
            }
        }

        public void SetTime(float time)
        {
            Time = Mathf.Min(time, Time);
        }

        public void SetInput(string input)
        {
            Input = input;
        }

        private void SetupProperties()
        {
            _username = PlayerPrefs.GetString(PlayerPrefKey.UserName, "");
            if (ShouldUploadScores)
            {
                _time = PlayerPrefs.GetFloat(PlayerPrefKey.Timer, float.MaxValue);
                _input = PlayerPrefs.GetString(PlayerPrefKey.Input, "");
            }
        }

        private void Upload()
        {
            PlayerPrefs.SetInt(PlayerPrefKey.ShouldUpload, 1);
            _endpoint.SendScore(_username, _time, _input);
        }

        private bool CanUpload()
        {
            var isUserNameFilled = !_username.IsNullOrWhitespace();
            var isTimeHigher = _time > 0;
            var isInputFilled = !_input.IsNullOrWhitespace();
            return isUserNameFilled && isTimeHigher && isInputFilled;
        }

        private void HandleEndPointOnSuccess()
        {
            _numberOfRetries = 0;
            PlayerPrefs.DeleteKey(PlayerPrefKey.Timer);
            PlayerPrefs.DeleteKey(PlayerPrefKey.Input);
            PlayerPrefs.DeleteKey(PlayerPrefKey.ShouldUpload);
            StopLoading?.Invoke();
        }

        private void HandleEndPointOnFail()
        {
            _numberOfRetries++;
            var isNumberOfRetriesWithinLimit = _numberOfRetries < MaxNumberOfRetries;
            if (isNumberOfRetriesWithinLimit)
            {
                UploadIfPossible();
            }
            else
            {
                StopLoading?.Invoke();
            }
        }

        private void Awake()
        {
            SetupProperties();
        }

    }
}