// Copyright © 2018-2019 Chikilev V.A. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WinVVTools.InternalShared.Interactions
{
    /// <summary>
    /// Interaction Logic for MessageDialogView.xaml
    /// </summary>
    public partial class MessageDialogView : UserControl
    {
        private Storyboard _showingOverlayStoryboard;
        private Storyboard _showingContentStoryboard;
        private Storyboard _closingOverlayStoryboard;
        private Storyboard _closingContentStoryboard;
        
        public static readonly DependencyProperty DialogVisibilityProperty =
                    DependencyProperty.Register("DialogVisibility", typeof(MessageDialogState), typeof(MessageDialogView));

        public MessageDialogState DialogVisibility
        {
            get { return (MessageDialogState)GetValue(DialogVisibilityProperty); }
            set { SetValue(DialogVisibilityProperty, value); }
        }

        public MessageDialogView()
        {
            InitializeComponent();

            _showingOverlayStoryboard = this.TryFindResource("DialogShownOverlayStoryboard") as Storyboard;
            _showingContentStoryboard = this.TryFindResource("DialogShownContentStoryboard") as Storyboard;
            _closingOverlayStoryboard = this.TryFindResource("DialogCloseOverlayStoryboard") as Storyboard;
            _closingContentStoryboard = this.TryFindResource("DialogCloseContentStoryboard") as Storyboard;

            PART_Overlay.Opacity = 0;
            PART_DialogContent.Opacity = 0;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_closingOverlayStoryboard != null)
                _closingOverlayStoryboard.Completed += (s2, e2) => CloseDialog();
            
            var pd = DependencyPropertyDescriptor.FromProperty(DialogVisibilityProperty, typeof(MessageDialogView));
            if (pd != null)
                pd.AddValueChanged(this, DialogVisibilityPropertyChanged);

            if (DialogVisibility == MessageDialogState.Open)
                Show();
        }
        
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_closingOverlayStoryboard != null)
                _closingOverlayStoryboard.Completed -= (s2, e2) => CloseDialog();

            var pd = DependencyPropertyDescriptor.FromProperty(DialogVisibilityProperty, typeof(MessageDialogView));
            if (pd != null)
                pd.RemoveValueChanged(this, DialogVisibilityPropertyChanged);
        }

        private void DialogVisibilityPropertyChanged(object sender, EventArgs e)
        {
            switch (DialogVisibility)
            {
                case MessageDialogState.Close:
                    {
                        //The window closes automatically by associating the Visibility property with DialogVisibility
                        break;
                    }
                case MessageDialogState.Closing:
                    {
                        if (_closingOverlayStoryboard != null)
                        {
                            _closingContentStoryboard.Begin(PART_DialogContent);
                            _closingOverlayStoryboard.Begin(PART_Overlay);
                        }
                        else
                            CloseDialog();

                        break;
                    }
                case MessageDialogState.Open:
                    {
                        _closingOverlayStoryboard.Stop();
                        _closingContentStoryboard.Stop();
                        Show();

                        break;
                    }
                default:
                    break;
            }
        }

        private void Show()
        {
            if (_showingOverlayStoryboard != null)
                _showingOverlayStoryboard.Begin(PART_Overlay);

            if (_showingContentStoryboard != null)
                _showingContentStoryboard.Begin(PART_DialogContent);
        }
        
        private void CloseDialog()
        {
            var dc = DataContext as IMessageDialog;
            dc.CloseDialog();
        }
    }
}
