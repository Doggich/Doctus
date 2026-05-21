using Markdig;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;

namespace MarkdownEditorDiploma
{
    public partial class MainWindow
    {
        private async void UpdatePreview()
        {
            string markdownText = Editor.Text;
            string html = Markdown.ToHtml(markdownText, _pipeline);

            string userStylesBlock = string.IsNullOrWhiteSpace(_userPreviewCss)
                ? ""
                : $"<style>{_userPreviewCss}</style>";

            string fullHtml = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            margin: 20px;
                            line-height: 1.6;
                            background-color: #1E1E1E;
                            color: #D4D4D4;
                        }}
                        h1 {{ color: #D4D4D4; border-bottom: 2px solid #3E3E42; padding-bottom: 10px; }}
                        h2 {{ color: #D4D4D4; border-bottom: 1px solid #3E3E42; padding-bottom: 6px; }}
                        h3, h4, h5, h6 {{ color: #D4D4D4; }}
                        a {{ color: #4F9AC6; text-decoration: none; }}
                        a:hover {{ color: #6BB8E8; text-decoration: underline; }}
    
                        .code-block {{
                            position: relative;
                            margin: 15px 0;
                        }}
                        .code-block pre {{
                            background: #2D2D2D;
                            padding: 12px;
                            padding-right: 50px;
                            border-radius: 6px;
                            overflow-x: auto;
                            border-left: 3px solid #DB1A1A;
                            margin: 0;
                        }}
                        .code-block pre code {{
                            background: none;
                            padding: 0;
                            color: #D4D4D4;
                            font-family: 'Consolas', 'Courier New', monospace;
                            font-size: 13px;
                        }}
                        .copy-btn {{
                            position: absolute;
                            top: 8px;
                            right: 8px;
                            background: #3E3E42;
                            border: none;
                            border-radius: 4px;
                            padding: 4px 8px;
                            cursor: pointer;
                            font-size: 14px;
                            opacity: 0;
                            transition: opacity 0.2s;
                            color: #D4D4D4;
                        }}
                        .copy-btn:hover {{
                            background: #DB1A1A;
                            color: white;
                        }}
                        .code-block:hover .copy-btn {{
                            opacity: 1;
                        }}
                        .copy-btn.copied {{
                            background: #27AE60;
                            color: white;
                        }}
    
                        code {{
                            background: #2D2D2D;
                            padding: 2px 5px;
                            border-radius: 3px;
                            font-family: 'Consolas', 'Courier New', monospace;
                            color: #CE9178;
                        }}
                        blockquote {{
                            border-left: 3px solid #468A9A;
                            margin: 10px 0;
                            padding: 10px 15px;
                            color: #F6EFD2;
                            background: #252525;
                            border-radius: 0 8px 8px 0;
                            font-style: italic;
                            font-family: 'Georgia', 'Times New Roman', serif;
                        }}
                        blockquote::before {{
                            content: ""“"";
                            font-size: 24px;
                            font-style: normal;
                            color: #468A9A;
                            margin-right: 8px;
                            vertical-align: middle;
                        }}
                        blockquote::after {{
                            content: ""”"";
                            font-size: 24px;
                            font-style: normal;
                            color: #468A9A;
                            margin-left: 8px;
                            vertical-align: middle;
                        }}
                        table {{
                            border-collapse: collapse;
                            width: 100%;
                            margin: 15px 0;
                        }}
                        th, td {{
                            border: 1px solid #3E3E42;
                            padding: 8px 12px;
                            text-align: left;
                        }}
                        th {{
                            background: #2D2D2D;
                            color: #D4D4D4;
                        }}
                        td {{
                            background: #1E1E1E;
                        }}
                        hr {{
                            border: none;
                            border-top: 1px solid #3E3E42;
                            margin: 20px 0;
                        }}
                        ul, ol {{
                            padding-left: 25px;
                        }}
                        li {{
                            margin: 4px 0;
                        }}
                        strong {{ color: #C586C0; }}
                        em {{ color: #9CDCFE; }}
                        u {{ 
                            text-decoration: underline;
                            text-decoration-thickness: 2px;
                            text-underline-offset: 3px;
                            color: #6FCF97;
                        }}
                    </style>
                    {userStylesBlock}
                    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/vs2015.min.css'>
                    <script src='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js'></script>
                    <script>
                        function copyToCSharp(codeText) {{
                            if (window.chrome && window.chrome.webview) {{
                                window.chrome.webview.postMessage(JSON.stringify({{ type: 'copy', text: codeText }}));
                            }} else {{
                                console.error('WebView2 не инициализирован');
                            }}
                        }}
    
                        function addCopyButtons() {{
                            document.querySelectorAll('pre').forEach(pre => {{
                                if (pre.parentElement.classList.contains('code-block')) return;
                                var codeText = pre.querySelector('code') ? pre.querySelector('code').innerText : pre.innerText;
                                var wrapper = document.createElement('div');
                                wrapper.className = 'code-block';
                                var button = document.createElement('button');
                                button.className = 'copy-btn';
                                button.textContent = '📋';
                                button.onclick = () => {{
                                    copyToCSharp(codeText);
                                    button.textContent = '✓';
                                    button.classList.add('copied');
                                    setTimeout(() => {{
                                        button.textContent = '📋';
                                        button.classList.remove('copied');
                                    }}, 2000);
                                }};
                                pre.parentNode.insertBefore(wrapper, pre);
                                wrapper.appendChild(pre);
                                wrapper.appendChild(button);
                            }});
                        }}
    
                        document.addEventListener('DOMContentLoaded', () => {{
                            hljs.highlightAll();
                            addCopyButtons();
                        }});
                    </script>
                </head>
                <body>
                    {html}
                    <script>
                        hljs.highlightAll();
                        addCopyButtons();
                    </script>
                </body>
                </html>";

            Preview.NavigateToString(fullHtml);
        }

        private string GeneratePdfHtml(string markdownText)
        {
            string htmlBody = Markdown.ToHtml(markdownText, _pipeline);

            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <title>Doctus PDF Export</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        margin: 30px;
                        line-height: 1.5;
                        background-color: white;
                        color: black;
                    }}
                    h1 {{ color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 8px; }}
                    h2 {{ color: #34495e; border-bottom: 1px solid #bdc3c7; padding-bottom: 5px; }}
                    h3, h4, h5, h6 {{ color: #555; }}
                    a {{ color: #2980b9; text-decoration: none; }}
                    pre {{
                        background-color: #f4f4f4;
                        padding: 10px;
                        border-radius: 5px;
                        overflow-x: auto;
                        font-family: Consolas, monospace;
                        font-size: 13px;
                        border-left: 3px solid #3498db;
                    }}
                    code {{
                        background-color: #f4f4f4;
                        padding: 2px 4px;
                        border-radius: 3px;
                        font-family: Consolas, monospace;
                        color: #c0392b;
                    }}
                    blockquote {{
                        border-left: 3px solid #3498db;
                        margin: 10px 0;
                        padding: 8px 15px;
                        background-color: #f9f9f9;
                        font-style: italic;
                    }}
                    table {{
                        border-collapse: collapse;
                        width: 100%;
                        margin: 15px 0;
                    }}
                    th, td {{
                        border: 1px solid #ddd;
                        padding: 8px 12px;
                        text-align: left;
                    }}
                    th {{
                        background-color: #f2f2f2;
                        font-weight: bold;
                    }}
                    hr {{
                        border: none;
                        border-top: 1px solid #ddd;
                        margin: 20px 0;
                    }}
                    ul, ol {{
                        padding-left: 25px;
                    }}
                    li {{
                        margin: 3px 0;
                    }}
                    u {{
                        text-decoration: underline;
                    }}
                </style>
            </head>
            <body>
                {htmlBody}
            </body>
            </html>";
        }
        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.TryGetWebMessageAsString();
                var json = System.Text.Json.JsonDocument.Parse(message);
                string type = json.RootElement.GetProperty("type").GetString();
                if (type == "copy")
                {
                    string textToCopy = json.RootElement.GetProperty("text").GetString();
                    Clipboard.SetText(textToCopy);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        

        private void SubscribeWebView2Messages()
        {
            Preview.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }
    }
}