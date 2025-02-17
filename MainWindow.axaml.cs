using System;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace AIWrite;

public partial class MainWindow : Window
{
    private Timer _typingTimer;
    public MainWindow()
    {
        InitializeComponent();
        
        var textBox = this.FindControl<TextBox>("TextEditor");
        
        if(textBox != null)
        {
            textBox.TextChanged += TextEditor_OnTextChanged;
        }
        
        // Initialize the timer
        _typingTimer = new Timer(1000); // 1 second
        _typingTimer.Elapsed += OnTypingTimerElapsed;
        _typingTimer.AutoReset = false; // Prevent it from firing multiple times
    }
    private void ClearText(object sender, RoutedEventArgs e)
    {
        TextEditor.Text = string.Empty;
    }

    private void TextEditor_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Restart the timer every time the user types
        _typingTimer.Stop();
        _typingTimer.Start();
    
        // Set the bottom TextArea's text
        TextEditorHints.Text = TextEditor.Text;
    }
    private void OnTypingTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Console.WriteLine("Hello");
        // Update UI on the main thread
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            TextEditorHints.Text = TextEditor.Text + " whats up bruh";
        });
    }
}