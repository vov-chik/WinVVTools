// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Prism.Mvvm;
using System.Collections.Generic;

namespace WinVVTools.CleanUp.Models
{
    internal class AnalyseProcessActivity : BindableBase
    {
        public AnalyseObject AnalyseObject
        {
            get { return _analyseObject; }
            set { SetProperty(ref _analyseObject, value); }
        }
        private AnalyseObject _analyseObject;
        
        public int SearchCount
        {
            get { return _searchCount; }
            set { SetProperty(ref _searchCount, value); }
        }
        private int _searchCount;
        
        public AnalyseState SearchState
        {
            get { return _searchState; }
            set { SetProperty(ref _searchState, value); }
        }
        private AnalyseState _searchState;

        public int CheckPointCount
        {
            get { return _checkPointCount; }
            set { SetProperty(ref _checkPointCount, value); }
        }
        private int _checkPointCount;

        public AnalyseState CheckPointState
        {
            get { return _checkPointState; }
            set { SetProperty(ref _checkPointState, value); }
        }
        private AnalyseState _checkPointState;

        public int? CompareCount
        {
            get { return _compareCount; }
            set { SetProperty(ref _compareCount, value); }
        }
        private int? _compareCount;

        public AnalyseState CompareState
        {
            get { return _compareState; }
            set { SetProperty(ref _compareState, value); }
        }
        private AnalyseState _compareState;

        public IEnumerable<string> CompareObjects { get; set; }

        public bool IsError
        {
            get { return _isError; }
            set { SetProperty(ref _isError, value); }
        }
        private bool _isError;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }
        private string _errorMessage;

        public AnalyseProcessActivity(AnalyseObject analyseObject)
        {
            AnalyseObject = analyseObject;
        }
    }
}
