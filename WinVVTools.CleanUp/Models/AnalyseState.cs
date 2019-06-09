// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using Prism.Events;

namespace WinVVTools.CleanUp.Models
{
    public enum AnalyseType
    {
        Off = 0,
        SearchAllObjects = 1,
        SearchNewObjects = 2
    }

    public enum AnalyseState
    {
        Off = 0,
        Started = 1,
        Processing = 2,
        Completed = 3,
        Interrupted = 4 //when canceling the analysis manually or if an error
    }

    public sealed class AnalyseEventMessage
    {
        public readonly AnalyseType Type;
        public readonly AnalyseState State;
        public readonly int Steps; //The total number of stages of analysis
        public readonly int CurrentStep; //Current (or completed) analysis stage

        public AnalyseEventMessage(AnalyseType type, 
                                   AnalyseState state)
        {
            Type = type;
            State = state;
        }

        public AnalyseEventMessage(AnalyseType type, 
                                   AnalyseState state, 
                                   int steps,
                                   int currentStep) : this(type, state)
        {
            Steps = steps;
            CurrentStep = currentStep;
        }
    }

    public sealed class AnalyseEvent : PubSubEvent<AnalyseEventMessage>
    {
    }
}
