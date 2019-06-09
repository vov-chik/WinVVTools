// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinVVTools.InternalShared.Interactions
{
    public class MessageDialogViewModel : BindableBase, IMessageDialog
    {
        #region Fields

        public MessageDialogState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }
        private MessageDialogState _state = MessageDialogState.Close;

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string _title = string.Empty;

        public string MessageText
        {
            get { return _messageText; }
            set { SetProperty(ref _messageText, value); }
        }
        private string _messageText = string.Empty;

        public bool MessageTextVisible
        {
            get { return _messageTextVisible; }
            set { SetProperty(ref _messageTextVisible, value); }
        }
        private bool _messageTextVisible = true;

        public bool IsInput
        {
            get { return _isInput; }
            set { SetProperty(ref _isInput, value); }
        }
        private bool _isInput = false;

        public string InputText
        {
            get { return _inputText; }
            set { SetProperty(ref _inputText, value); }
        }
        private string _inputText = string.Empty;

        public string AffirmativeButtonText
        {
            get { return _affirmativeButtonText; }
            set { SetProperty(ref _affirmativeButtonText, value); }
        }
        private string _affirmativeButtonText = string.Empty;

        public string NegativeButtonText
        {
            get { return _negativeButtonText; }
            set { SetProperty(ref _negativeButtonText, value); }
        }
        private string _negativeButtonText = string.Empty;

        public string FirstAuxiliaryButtonText
        {
            get { return _firstAuxiliaryButtonText; }
            set { SetProperty(ref _firstAuxiliaryButtonText, value); }
        }
        private string _firstAuxiliaryButtonText = string.Empty;

        public string SecondAuxiliaryButtonText
        {
            get { return _secondAuxiliaryButtonText; }
            set { SetProperty(ref _secondAuxiliaryButtonText, value); }
        }
        private string _secondAuxiliaryButtonText = string.Empty;

        public bool NegativeButtonVisible
        {
            get { return _negativeButtonVisible; }
            set { SetProperty(ref _negativeButtonVisible, value); }
        }
        private bool _negativeButtonVisible = true;

        public bool FirstAuxiliaryButtonVisible
        {
            get { return _firstAuxiliaryButtonVisible; }
            set { SetProperty(ref _firstAuxiliaryButtonVisible, value); }
        }
        private bool _firstAuxiliaryButtonVisible = false;

        public bool SecondAuxiliaryButtonVisible
        {
            get { return _secondAuxiliaryButtonVisible; }
            set { SetProperty(ref _secondAuxiliaryButtonVisible, value); }
        }
        private bool _secondAuxiliaryButtonVisible = false;

        private TaskCompletionSource<MessageDialogResult> _tcs;

        private Queue<(MessageDialogSettings settings, TaskCompletionSource<MessageDialogResult> tcs)> _messageQueue;

        #endregion


        #region Constructor

        public MessageDialogViewModel()
        {
            UpdateCommandCompletion(new TaskCompletionSource<MessageDialogResult>());
            _messageQueue = new Queue<(MessageDialogSettings, TaskCompletionSource<MessageDialogResult>)>();
        }

        public MessageDialogViewModel(MessageDialogSettings settings) : this()
        {
            UpdateDialogSettings(settings);
        }

        #endregion


        private void UpdateCommandCompletion(TaskCompletionSource<MessageDialogResult> tcs)
        {
            _tcs = tcs;

            AffirmativeButtonClick = new Action(() =>
            {
                _tcs.TrySetResult(MessageDialogResult.Affirmative);
            });

            NegativeButtonClick = new Action(() =>
            {
                _tcs.TrySetResult(MessageDialogResult.Negative);
            });

            FirstAuxiliaryButtonClick = new Action(() =>
            {
                _tcs.TrySetResult(MessageDialogResult.FirstAuxiliary);
            });

            SecondAuxiliaryButtonClick = new Action(() =>
            {
                _tcs.TrySetResult(MessageDialogResult.SecondAuxiliary);
            });
        }

        private void UpdateDialogSettings(MessageDialogSettings settings)
        {
            this.Title = settings.Title;
            this.MessageText = settings.MessageText;
            this.MessageTextVisible = settings.MessageTextVisible;

            this.InputText = string.Empty;
            this.IsInput = settings.IsInput;

            this.AffirmativeButtonText = settings.AffirmativeButtonText;
            this.NegativeButtonText = settings.NegativeButtonText;
            this.FirstAuxiliaryButtonText = settings.FirstAuxiliaryButtonText;
            this.SecondAuxiliaryButtonText = settings.SecondAuxiliaryButtonText;

            ChangeButtonVisible(settings.DialogtButtons);
        }

        private void ChangeButtonVisible(MessageDialogButtons dialogtButtons)
        {
            switch (dialogtButtons)
            {
                case MessageDialogButtons.Affirmative:
                    NegativeButtonVisible = false;
                    FirstAuxiliaryButtonVisible = false;
                    SecondAuxiliaryButtonVisible = false;
                    break;
                case MessageDialogButtons.AffirmativeAndNegativeAndSingleAuxiliary:
                    NegativeButtonVisible = true;
                    FirstAuxiliaryButtonVisible = true;
                    SecondAuxiliaryButtonVisible = false;
                    break;
                case MessageDialogButtons.AffirmativeAndNegativeAndDoubleAuxiliary:
                    NegativeButtonVisible = true;
                    FirstAuxiliaryButtonVisible = true;
                    SecondAuxiliaryButtonVisible = true;
                    break;
                default:
                    NegativeButtonVisible = true;
                    FirstAuxiliaryButtonVisible = false;
                    SecondAuxiliaryButtonVisible = false;
                    break;
            }
        }


        #region Commands

        private DelegateCommand _affirmativeButtonCommand;
        public DelegateCommand AffirmativeButtonCommand
        {
            get { return _affirmativeButtonCommand ?? (_affirmativeButtonCommand = new DelegateCommand(AffirmativeButtonClick)); }
        }
        private Action AffirmativeButtonClick;

        private DelegateCommand _negativeButtonCommand;
        public DelegateCommand NegativeButtonCommand
        {
            get { return _negativeButtonCommand ?? (_negativeButtonCommand = new DelegateCommand(NegativeButtonClick)); }
        }
        private Action NegativeButtonClick;

        private DelegateCommand _firstAuxiliaryButtonCommand;
        public DelegateCommand FirstAuxiliaryButtonCommand
        {
            get { return _firstAuxiliaryButtonCommand ?? (_firstAuxiliaryButtonCommand = new DelegateCommand(FirstAuxiliaryButtonClick)); }
        }
        private Action FirstAuxiliaryButtonClick;

        private DelegateCommand _secondAuxiliaryButtonCommand;
        public DelegateCommand SecondAuxiliaryButtonCommand
        {
            get { return _secondAuxiliaryButtonCommand ?? (_secondAuxiliaryButtonCommand = new DelegateCommand(SecondAuxiliaryButtonClick)); }
        }
        private Action SecondAuxiliaryButtonClick;

        #endregion


        public Task<(MessageDialogResult result, string inputText)> ShowInputDialog(MessageDialogSettings settings)
        {
            settings.IsInput = true;
            var tcs = new TaskCompletionSource<MessageDialogResult>();

            lock (_messageQueue)
            {
                if (State == MessageDialogState.Open)
                {
                    _messageQueue.Enqueue((settings, tcs));
                }
                else
                {
                    UpdateCommandCompletion(tcs);
                    UpdateDialogSettings(settings);
                    ShowDialog();
                }
            }

            return WaitForButtonPressAsync(tcs).ContinueWith(t =>
            {
                string text = InputText;
                lock (_messageQueue)
                {
                    if (_messageQueue.Count > 0)
                    {
                        DequeueMessage();
                    }
                    else
                    {
                        State = MessageDialogState.Closing;
                    }
                }
                return (t.Result, text);
            });
        }

        public Task<MessageDialogResult> ShowMessageBox(MessageDialogSettings settings)
        {
            settings.IsInput = false;
            var tcs = new TaskCompletionSource<MessageDialogResult>();

            lock (_messageQueue)
            {
                if (State == MessageDialogState.Open)
                {
                    _messageQueue.Enqueue((settings, tcs));
                }
                else
                {
                    UpdateCommandCompletion(tcs);
                    UpdateDialogSettings(settings);
                    ShowDialog();
                }
            }

            return WaitForButtonPressAsync(tcs).ContinueWith(t =>
            {
                lock (_messageQueue)
                {
                    if (_messageQueue.Count > 0)
                    {
                        DequeueMessage();
                    }
                    else
                    {
                        State = MessageDialogState.Closing;
                    }
                }
                return t.Result;
            });
        }

        private void DequeueMessage()
        {
            var (settings, tcs) = _messageQueue.Dequeue();

            UpdateCommandCompletion(tcs);
            UpdateDialogSettings(settings);
            ShowDialog();
        }

        private Task<MessageDialogResult> WaitForButtonPressAsync(TaskCompletionSource<MessageDialogResult> tcs)
        {
            return tcs.Task;
        }

        private void ShowDialog()
        {
            State = MessageDialogState.Open;
        }

        public void CloseDialog()
        {
            if (State == MessageDialogState.Closing)
                State = MessageDialogState.Close;
        }

    }
}
