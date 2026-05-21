using Markdig;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace MarkdownEditorDiploma
{
    public partial class MainWindow
    {
        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSaveBeforeClose())
            {
                Editor.Text = "";
                _currentFilePath = string.Empty;
                _isDirty = false;
                UpdateTitle();
                UpdatePreview();
            }
        }

        private void OpenMd_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSaveBeforeClose()) return;

            var dialog = new OpenFileDialog
            {
                Title = "Открыть Markdown файл",
                Filter = "Markdown файлы (*.md;*.markdown)|*.md;*.markdown|Все файлы (*.*)|*.*",
                DefaultExt = ".md"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string content = File.ReadAllText(dialog.FileName);
                    Editor.Text = content;
                    _currentFilePath = dialog.FileName;
                    _isDirty = false;
                    UpdateTitle();
                    UpdatePreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditPreviewStyles_Click(object sender, RoutedEventArgs e)
        {
            string cssPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "preview-styles.css");

            if (File.Exists(cssPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = cssPath,
                    UseShellExecute = true
                });
            }
            else
            {
                LoadUserPreviewStyles(); // создаст файл
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = cssPath,
                    UseShellExecute = true
                });
            }
        }

        private void ReloadPreviewStyles_Click(object sender, RoutedEventArgs e)
        {
            LoadUserPreviewStyles();
            UpdatePreview();
            MessageBox.Show("Стили перезагружены", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadCssFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите CSS-файл со стилями",
                Filter = "CSS файлы (*.css)|*.css|Все файлы (*.*)|*.*",
                DefaultExt = ".css"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "preview-styles.css");
                    File.Copy(dialog.FileName, destPath, overwrite: true);

                    LoadUserPreviewStyles();

                    UpdatePreview();

                    MessageBox.Show($"Стиль загружен:\n{Path.GetFileName(dialog.FileName)}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке стиля:\n{ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void EditCurrentStyle_Click(object sender, RoutedEventArgs e)
        {
            string cssPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "preview-styles.css");

            // Убедимся, что файл существует
            if (!File.Exists(cssPath))
            {
                CreateDefaultCssFile();
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = cssPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetToDefaultStyle_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Сбросить стили на стандартные? Все пользовательские настройки будут удалены.",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string cssPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "preview-styles.css");

                if (File.Exists(cssPath))
                {
                    File.Delete(cssPath);
                }

                File.WriteAllText(cssPath, "/* Пользовательский стиль отключён */");

                _userPreviewCss = "";
                UpdatePreview();

                MessageBox.Show("Стили сброшены на стандартные", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportAsMd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Экспортировать как Markdown",
                Filter = "Markdown файлы (*.md)|*.md|Все файлы (*.*)|*.*",
                DefaultExt = ".md",
                FileName = string.IsNullOrEmpty(_currentFilePath)
                    ? "export.md"
                    : Path.GetFileNameWithoutExtension(_currentFilePath) + "_export.md"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, Editor.Text);
                    MessageBox.Show($"Файл экспортирован:\n{dialog.FileName}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте:\n{ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportAsHtml_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Экспортировать как HTML",
                Filter = "HTML файлы (*.html)|*.html|Все файлы (*.*)|*.*",
                DefaultExt = ".html",
                FileName = string.IsNullOrEmpty(_currentFilePath)
                    ? "export.html"
                    : Path.GetFileNameWithoutExtension(_currentFilePath) + "_export.html"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string markdownText = Editor.Text;
                    string htmlBody = Markdown.ToHtml(markdownText, _pipeline);

                    // Упрощённая светлая тема для экспорта (хорошо печатается)
                    string fullHtml = $@"<!DOCTYPE html>
                                        <html>
                                        <head>
                                            <meta charset='UTF-8'>
                                            <title>{Path.GetFileNameWithoutExtension(dialog.FileName)}</title>
                                            <style>
                                                body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 40px; line-height: 1.6; max-width: 1000px; margin-left: auto; margin-right: auto; background-color: white; color: #333; }}
                                                h1 {{ color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }}
                                                h2 {{ color: #34495e; border-bottom: 1px solid #bdc3c7; padding-bottom: 6px; }}
                                                code {{ background: #f4f4f4; padding: 2px 5px; border-radius: 3px; font-family: Consolas; color: #c0392b; }}
                                                pre {{ background: #f4f4f4; padding: 10px; border-radius: 5px; overflow-x: auto; border-left: 3px solid #3498db; }}
                                                blockquote {{ border-left: 3px solid #3498db; margin: 10px 0; padding-left: 15px; color: #666; background: #f9f9f9; padding: 10px 15px; }}
                                                table {{ border-collapse: collapse; width: 100%; margin: 15px 0; }}
                                                th, td {{ border: 1px solid #ddd; padding: 8px 12px; text-align: left; }}
                                                th {{ background: #f2f2f2; }}
                                                hr {{ border: none; border-top: 1px solid #ddd; margin: 20px 0; }}
                                            </style>
                                            <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github.min.css'>
                                            <script src='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js'></script>
                                            <script>hljs.highlightAll();</script>
                                        </head>
                                        <body>
                                            {htmlBody}
                                        </body>
                                        </html>";

                    File.WriteAllText(dialog.FileName, fullHtml);
                    MessageBox.Show($"HTML экспортирован:\n{dialog.FileName}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте HTML:\n{ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExportAsPdf_Click(object sender, RoutedEventArgs e)
        {
            if (Preview?.CoreWebView2 == null)
            {
                MessageBox.Show("Предпросмотр ещё не загружен. Подождите немного.",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Экспортировать как PDF",
                Filter = "PDF файлы (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = string.IsNullOrEmpty(_currentFilePath)
                    ? "export.pdf"
                    : Path.GetFileNameWithoutExtension(_currentFilePath) + "_export.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Используем упрощённую HTML-разметку для PDF (светлая тема)
                    string pdfHtml = GeneratePdfHtml(Editor.Text);

                    var tempWebView = new Microsoft.Web.WebView2.Wpf.WebView2();
                    await tempWebView.EnsureCoreWebView2Async();

                    var navigationCompleted = new TaskCompletionSource<bool>();
                    tempWebView.NavigationCompleted += (s, args) => navigationCompleted.TrySetResult(true);
                    tempWebView.NavigateToString(pdfHtml);
                    await navigationCompleted.Task;
                    await Task.Delay(500);

                    var settings = tempWebView.CoreWebView2.Environment.CreatePrintSettings();
                    settings.ShouldPrintBackgrounds = true;

                    bool success = await tempWebView.CoreWebView2.PrintToPdfAsync(dialog.FileName, settings);
                    tempWebView.Dispose();

                    if (success)
                    {
                        MessageBox.Show($"PDF экспортирован:\n{dialog.FileName}",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось создать PDF.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте PDF:\n{ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CopyHtmlToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string htmlBody = Markdown.ToHtml(Editor.Text, _pipeline);
                string fullHtml = $@"<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Doctus Export</title></head><body>{htmlBody}</body></html>";

                Clipboard.SetText(fullHtml);
                MessageBox.Show("HTML скопирован в буфер обмена",
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyMdToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Editor.Text);
                MessageBox.Show("Markdown скопирован в буфер обмена",
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void SendByEmail_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        // Сохраняем временный файл
        //        string tempFile = Path.GetTempFileName() + ".md";
        //        File.WriteAllText(tempFile, Editor.Text);

        //        // Открываем почтовый клиент
        //        string subject = Uri.EscapeDataString("Документ Doctus");
        //        string body = Uri.EscapeDataString("Отправляю документ, созданный в Doctus.");

        //        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        //        {
        //            FileName = $"mailto:?subject={subject}&body={body}",
        //            UseShellExecute = true
        //        });

        //        // Удаляем временный файл через 5 секунд
        //        Task.Delay(5000).ContinueWith(_ => { try { File.Delete(tempFile); } catch { } });
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при открытии почтового клиента:\n{ex.Message}",
        //            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void OpenHtml_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSaveBeforeClose()) return;

            var dialog = new OpenFileDialog
            {
                Title = "Открыть HTML файл",
                Filter = "HTML файлы (*.html;*.htm)|*.html;*.htm|Все файлы (*.*)|*.*",
                DefaultExt = ".html"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string htmlContent = File.ReadAllText(dialog.FileName);
                    string bodyContent = ExtractBodyContent(htmlContent);
                    Editor.Text = bodyContent;
                    _currentFilePath = string.Empty;
                    _isDirty = true;
                    UpdateTitle();
                    UpdatePreview();

                    MessageBox.Show(
                        "HTML файл открыт. Внимание: он содержит HTML-теги, а не Markdown-разметку.\n\n" +
                        "Если вы сохраните этот документ как MD, в нём останутся теги (<strong>, <u>, <table> и др.).\n\n" +
                        "Рекомендуется использовать эту функцию только для просмотра HTML-кода.",
                        "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии HTML:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string ExtractBodyContent(string html)
        {
            int start = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
            if (start == -1) return html;

            start = html.IndexOf('>', start);
            if (start == -1) return html;
            start++;

            int end = html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            if (end == -1) return html.Substring(start);

            return html.Substring(start, end - start).Trim();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
                SaveAsMd_Click(sender, e);
            else
                SaveToFile(_currentFilePath);
        }

        private void SaveAsMd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Сохранить как Markdown",
                Filter = "Markdown файлы (*.md)|*.md|Все файлы (*.*)|*.*",
                DefaultExt = ".md",
                FileName = string.IsNullOrEmpty(_currentFilePath) ? "document.md" : Path.GetFileNameWithoutExtension(_currentFilePath) + ".md"
            };

            if (dialog.ShowDialog() == true)
            {
                SaveToFile(dialog.FileName);
                _currentFilePath = dialog.FileName;
                UpdateTitle();
            }
        }

        private void SaveAsHtml_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Сохранить как HTML",
                Filter = "HTML файлы (*.html)|*.html|Все файлы (*.*)|*.*",
                DefaultExt = ".html",
                FileName = string.IsNullOrEmpty(_currentFilePath) ? "document.html" : Path.GetFileNameWithoutExtension(_currentFilePath) + ".html"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string markdownText = Editor.Text;
                    string htmlBody = Markdown.ToHtml(markdownText, _pipeline);

                    string fullHtml = $@"<!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <title>{Path.GetFileNameWithoutExtension(dialog.FileName)}</title>
                        <style>
                            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 40px; line-height: 1.6; max-width: 900px; margin-left: auto; margin-right: auto; background-color: #1E1E1E; color: #D4D4D4; }}
                            h1 {{ color: #D4D4D4; border-bottom: 2px solid #3E3E42; padding-bottom: 10px; }}
                            h2 {{ color: #D4D4D4; border-bottom: 1px solid #3E3E42; padding-bottom: 6px; }}
                            code {{ background: #2D2D2D; padding: 2px 5px; border-radius: 3px; font-family: Consolas; color: #CE9178; }}
                            pre {{ background: #2D2D2D; padding: 10px; border-radius: 5px; overflow-x: auto; }}
                            pre code {{ background: none; padding: 0; color: #D4D4D4; }}
                            blockquote {{ border-left: 3px solid #DB1A1A; margin: 10px 0; padding-left: 15px; color: #9CDCFE; background: #252525; }}
                            table {{ border-collapse: collapse; width: 100%; }}
                            th, td {{ border: 1px solid #3E3E42; padding: 8px; text-align: left; }}
                            th {{ background: #2D2D2D; }}
                            hr {{ border: none; border-top: 1px solid #3E3E42; margin: 20px 0; }}
                        </style>
                        <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/vs2015.min.css'>
                        <script src='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js'></script>
                        <script>hljs.highlightAll();</script>
                    </head>
                    <body>
                        {htmlBody}
                    </body>
                    </html>";

                    File.WriteAllText(dialog.FileName, fullHtml);
                    MessageBox.Show($"HTML сохранён:\n{dialog.FileName}", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении HTML:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SaveAsPdf_Click(object sender, RoutedEventArgs e)
        {
            if (Preview?.CoreWebView2 == null)
            {
                MessageBox.Show("Предпросмотр ещё не загружен. Подождите немного.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Сохранить как PDF",
                Filter = "PDF файлы (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = string.IsNullOrEmpty(_currentFilePath)
                    ? "document.pdf"
                    : Path.GetFileNameWithoutExtension(_currentFilePath) + ".pdf"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                string pdfHtml = GeneratePdfHtml(Editor.Text);

                string originalHtml = null;

                var navigationCompleted = new TaskCompletionSource<bool>();
                EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs> handler = null;
                handler = (s, args) =>
                {
                    Preview.CoreWebView2.NavigationCompleted -= handler;
                    navigationCompleted.TrySetResult(true);
                };
                Preview.CoreWebView2.NavigationCompleted += handler;

                Preview.NavigateToString(pdfHtml);
                await navigationCompleted.Task;

                await Task.Delay(1000);

                var settings = Preview.CoreWebView2.Environment.CreatePrintSettings();
                settings.ShouldPrintBackgrounds = true;

                bool success = await Preview.CoreWebView2.PrintToPdfAsync(dialog.FileName, settings);

                UpdatePreview();

                if (success)
                {
                    MessageBox.Show($"PDF сохранён:\n{dialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось создать PDF файл.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                try { UpdatePreview(); } catch { }
            }
        }
        private void SaveToFile(string filePath)
        {
            try
            {
                File.WriteAllText(filePath, Editor.Text);
                _isDirty = false;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Markdown Редактор Doctus\nВерсия 1.0\n\nРазработано в рамках дипломного проекта\n\nИспользуемые технологии:\n- .NET 8 / WPF\n- WebView2\n- Markdig",
                            "О программе",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private bool CheckSaveBeforeClose()
        {
            if (!_isDirty) return true;

            var result = MessageBox.Show("Документ был изменён. Сохранить изменения?", "Сохранение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Save_Click(this, null);
                return !_isDirty;
            }

            return result == MessageBoxResult.No;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSaveBeforeClose())
                Close();
        }
    }
}