using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Newtonsoft.Json.Linq;

namespace AIWrite;

public partial class MainWindow : Window
{
    private Timer _typingTimer;
    private static readonly HttpClient client = new HttpClient();
    public MainWindow()
    {
        InitializeComponent();
        
        TextEditor.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Tab)
            {
                int caretIndex = TextEditor.CaretIndex;

                if (!string.IsNullOrEmpty(TextEditorHints.Text) && !string.IsNullOrEmpty(TextEditor.Text))
                {
                    string hintText = TextEditorHints.Text;
                    string currentText = TextEditor.Text;
                    hintText = hintText.Substring(currentText.Trim().Length);
                    TextEditor.Text = currentText.Insert(caretIndex, hintText);
                    TextEditor.CaretIndex = caretIndex + hintText.Length;
                }
                else
                {
                    Console.WriteLine("HEY");
                    if (string.IsNullOrEmpty(TextEditor.Text))
                    {
                        Console.WriteLine("SSS");
                        TextEditor.Text = "\t";
                    }
                    else
                    {
                        Console.WriteLine("WEFWEF");
                        TextEditor.Text = TextEditor.Text.Insert(caretIndex, "\t");
                    }
                    TextEditor.CaretIndex = caretIndex + 1;
                }

                e.Handled = true; // Prevent default tab navigation behavior
            }
        };


        
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
        if (TextEditor != null)
            TextEditor.Text = string.Empty;
    }

    private void TextEditor_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Restart the timer every time the user types
        _typingTimer.Stop();
        _typingTimer.Start();
    
        // Set the bottom TextArea's text
        //TextEditorHints.Text = TextEditor.Text;
        TextEditorHints.Text = string.Empty;
    }
    private async void OnTypingTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        string text = "";
    
        // Ensure reading TextEditor.Text happens on UI thread
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            string fullText = TextEditor?.Text ?? "";

            // Find the last period in the text
            int lastPeriodIndex = fullText.LastIndexOf('.');

            // If a period is found, get the text after it; otherwise, take the full text
            text = (lastPeriodIndex != -1 && lastPeriodIndex < fullText.Length - 1)
                ? fullText.Substring(lastPeriodIndex + 1).Trim()
                : fullText;
        });

        string prompt = $"Complete the following sentence by predicting the next most likely word. Respond with only the predicted word and nothing else. Sentence: {text} - Next word:";
        string model = "llama3.2:1b";

        string response = GenerateResponse(model, prompt);
        Console.WriteLine(response);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (text.EndsWith(' '))
            {
                TextEditorHints.Text = text + response;
            }
            else
            {
                TextEditorHints.Text = text + " " + response;
            }
        });
    }


    public static string GenerateResponse(string model, string prompt)
    {
        string url = "http://localhost:11434/api/generate";
        string fullResponse = "";

        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";

            JObject requestJson = new JObject
            {
                ["model"] = model,
                ["prompt"] = prompt,
                ["stream"] = false
            };

            byte[] requestData = Encoding.UTF8.GetBytes(requestJson.ToString());
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(requestData, 0, requestData.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string responseText = reader.ReadToEnd();
                JObject responseJson = JObject.Parse(responseText);
                fullResponse = responseJson["response"]?.ToString();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }

        return string.IsNullOrEmpty(fullResponse) ? string.Empty : fullResponse;
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            Console.WriteLine("Tab key pressed!");
        }
    }
}