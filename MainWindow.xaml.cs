using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace StudentsForm
{
    public partial class MainWindow : Window
    {
        private List<Student> students;
        private List<Student> expelledStudents;
        private int currentIndex;
        private bool isDataChanged;

        public class Student
        {
            public string FullName { get; set; } = "";
            public string Group { get; set; } = "";
            public string Subject { get; set; } = "";
            public int Grade { get; set; }
            public bool IsExpelled { get; set; }
            public bool IsSelected { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            students = new List<Student>();
            expelledStudents = new List<Student>();
            currentIndex = -1;
            isDataChanged = false;

            // Привязка событий для кнопок
            btnFirst.Click += NavigateFirst_Click;
            btnPrev.Click += NavigatePrevious_Click;
            btnNext.Click += NavigateNext_Click;
            btnLast.Click += NavigateLast_Click;
            btnNew.Click += AddNewStudent_Click;
            btnSave.Click += SaveCurrentStudent_Click;
            btnClear.Click += ClearCurrentCard_Click;
            btnExpel.Click += ExpelCurrentStudent_Click;
            btnManageExpelled.Click += ManageExpelledStudents_Click;
            btnInputBrowse.Click += BrowseInputFile_Click;
            btnOutputBrowse.Click += BrowseOutputFile_Click;

            // Привязка событий изменения данных
            txtFullName.TextChanged += OnDataChanged;
            txtGroup.TextChanged += OnDataChanged;
            txtSubject.TextChanged += OnDataChanged;
            txtGrade.TextChanged += OnDataChanged;
            txtGrade.PreviewTextInput += NumberValidationTextBox;

            // Загрузка настроек и данных
            LoadSettings();
            LoadData();
            UpdateDisplay();
        }

        #region Файловые операции
        private void LoadData()
        {
            try
            {
                string inputPath = txtInputPath.Text;

                if (File.Exists(inputPath))
                {
                    string extension = Path.GetExtension(inputPath).ToLower();
                    bool loaded = false;

                    // Пробуем загрузить из основного файла
                    switch (extension)
                    {
                        case ".dat":
                            loaded = LoadFromBinaryFile(inputPath);
                            break;
                        case ".txt":
                            loaded = LoadFromTextFile(inputPath);
                            break;
                        case ".bin":
                            loaded = LoadFromBytesFile(inputPath);
                            break;
                    }

                    // Если не удалось загрузить, пробуем другие форматы
                    if (!loaded)
                    {
                        string basePath = Path.Combine(Path.GetDirectoryName(inputPath),
                                                     Path.GetFileNameWithoutExtension(inputPath));

                        if (File.Exists(basePath + ".dat"))
                            loaded = LoadFromBinaryFile(basePath + ".dat");
                        else if (File.Exists(basePath + ".txt"))
                            loaded = LoadFromTextFile(basePath + ".txt");
                        else if (File.Exists(basePath + ".bin"))
                            loaded = LoadFromBytesFile(basePath + ".bin");
                    }

                    if (loaded)
                    {
                        UpdateStatus("Данные успешно загружены");
                    }
                    else
                    {
                        CreateDefaultData();
                    }
                }
                else
                {
                    CreateDefaultData();
                }

                if (students.Count > 0)
                {
                    currentIndex = 0;
                    UpdateDisplay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                CreateDefaultData();
            }
        }

        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!SaveCurrentStudent()) return;

                string outputPath = txtOutputPath.Text;
                string extension = Path.GetExtension(outputPath).ToLower();
                bool saved = false;

                // Сохраняем в указанный формат
                switch (extension)
                {
                    case ".dat":
                        saved = SaveToBinaryFile(outputPath);
                        break;
                    case ".txt":
                        saved = SaveToTextFile(outputPath);
                        break;
                    case ".bin":
                        saved = SaveToBytesFile(outputPath);
                        break;
                }

                if (saved)
                {
                    // Создаем копии в других форматах
                    string basePath = Path.Combine(Path.GetDirectoryName(outputPath),
                                                 Path.GetFileNameWithoutExtension(outputPath));

                    SaveToBinaryFile(basePath + ".dat");
                    SaveToTextFile(basePath + ".txt");
                    SaveToBytesFile(basePath + ".bin");

                    SaveSettings();
                    isDataChanged = false;
                    UpdateStatus("Данные успешно сохранены во всех форматах");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateDefaultData()
        {
            students.Clear();

            students.Add(new Student
            {
                FullName = "Иванов Иван Иванович",
                Group = "ИТ-21",
                Subject = "Программирование",
                Grade = 5
            });

            students.Add(new Student
            {
                FullName = "Петрова Анна Сергеевна",
                Group = "ИТ-21",
                Subject = "Математика",
                Grade = 4
            });

            students.Add(new Student
            {
                FullName = "Сидоров Алексей Петрович",
                Group = "ИТ-22",
                Subject = "Физика",
                Grade = 3
            });

            UpdateStatus("Созданы тестовые данные");
        }

        private bool LoadFromBinaryFile(string path)
        {
            try
            {
                students.Clear();
                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        var student = new Student
                        {
                            FullName = reader.ReadString(),
                            Group = reader.ReadString(),
                            Subject = reader.ReadString(),
                            Grade = reader.ReadInt32(),
                            IsExpelled = reader.ReadBoolean()
                        };
                        students.Add(student);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки бинарного файла: {ex.Message}");
                return false;
            }
        }

        private bool SaveToBinaryFile(string path)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    foreach (var student in students)
                    {
                        writer.Write(student.FullName);
                        writer.Write(student.Group);
                        writer.Write(student.Subject);
                        writer.Write(student.Grade);
                        writer.Write(student.IsExpelled);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка сохранения бинарного файла: {ex.Message}");
                return false;
            }
        }

        private bool LoadFromTextFile(string path)
        {
            try
            {
                students.Clear();
                var lines = File.ReadAllLines(path, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length >= 4)
                    {
                        var student = new Student
                        {
                            FullName = parts[0],
                            Group = parts[1],
                            Subject = parts[2],
                            Grade = int.TryParse(parts[3], out int grade) ? grade : 0,
                            IsExpelled = parts.Length > 4 && bool.TryParse(parts[4], out bool expelled) && expelled
                        };
                        students.Add(student);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки текстового файла: {ex.Message}");
                return false;
            }
        }

        private bool SaveToTextFile(string path)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
                {
                    foreach (var student in students)
                    {
                        writer.WriteLine($"{student.FullName};{student.Group};{student.Subject};{student.Grade};{student.IsExpelled}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка сохранения текстового файла: {ex.Message}");
                return false;
            }
        }

        private bool LoadFromBytesFile(string path)
        {
            try
            {
                students.Clear();
                byte[] bytes = File.ReadAllBytes(path);
                string content = Encoding.UTF8.GetString(bytes);
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length >= 4)
                    {
                        var student = new Student
                        {
                            FullName = parts[0],
                            Group = parts[1],
                            Subject = parts[2],
                            Grade = int.TryParse(parts[3], out int grade) ? grade : 0,
                            IsExpelled = parts.Length > 4 && bool.TryParse(parts[4], out bool expelled) && expelled
                        };
                        students.Add(student);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки байтового файла: {ex.Message}");
                return false;
            }
        }

        private bool SaveToBytesFile(string path)
        {
            try
            {
                var content = new StringBuilder();
                foreach (var student in students)
                {
                    content.AppendLine($"{student.FullName};{student.Group};{student.Subject};{student.Grade};{student.IsExpelled}");
                }
                File.WriteAllBytes(path, Encoding.UTF8.GetBytes(content.ToString()));
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка сохранения байтового файла: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Навигация
        private void NavigateFirst_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigateAway()) return;
            if (students.Count > 0)
            {
                currentIndex = 0;
                UpdateDisplay();
            }
        }

        private void NavigatePrevious_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigateAway()) return;
            if (students.Count == 0) return;

            if (currentIndex > 0)
            {
                currentIndex--;
                UpdateDisplay();
            }
            else if (chkCycleNavigation.IsChecked == true)
            {
                currentIndex = students.Count - 1;
                UpdateDisplay();
            }
            // Если не циклическая навигация и это первый элемент - остаемся на месте
        }

        private void NavigateNext_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigateAway()) return;

            if (students.Count == 0) return;

            if (currentIndex < students.Count - 1)
            {
                currentIndex++;
                UpdateDisplay();
            }
            else if (chkCycleNavigation.IsChecked == true)
            {
                currentIndex = 0;
                UpdateDisplay();
            }
            // Если не циклическая навигация и это последний элемент - остаемся на месте
        }

        private void NavigateLast_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigateAway()) return;
            if (students.Count > 0)
            {
                currentIndex = students.Count - 1;
                UpdateDisplay();
            }
        }

        private bool CanNavigateAway()
        {
            // Если нет изменений - можно переключаться свободно
            if (!isDataChanged) return true;

            // Если есть изменения, но поля пустые - можно переключаться
            if (string.IsNullOrWhiteSpace(txtFullName.Text) &&
                string.IsNullOrWhiteSpace(txtGroup.Text) &&
                string.IsNullOrWhiteSpace(txtSubject.Text) &&
                string.IsNullOrWhiteSpace(txtGrade.Text))
            {
                return true;
            }

            // Если есть заполненные поля - пытаемся сохранить
            return SaveCurrentStudent();
        }
        #endregion

        #region Управление студентами
        private void AddNewStudent_Click(object sender, RoutedEventArgs e)
        {
            // Сначала сохраняем текущего студента (если есть изменения)
            if (!CanNavigateAway())
            {
                return; // Если сохранение не удалось, не создаем нового
            }

            string newName = txtFullName.Text.Trim();
            string newGroup = txtGroup.Text.Trim();

            // Проверка дубликатов только при создании нового студента
            if (chkAllowDuplicateNames.IsChecked != true &&
                !string.IsNullOrEmpty(newName) &&
                !string.IsNullOrEmpty(newGroup))
            {
                bool hasDuplicate = students.Any(s =>
                    s.FullName.Equals(newName, StringComparison.OrdinalIgnoreCase) &&
                    s.Group.Equals(newGroup, StringComparison.OrdinalIgnoreCase));

                if (hasDuplicate)
                {
                    MessageBox.Show("Студент с такой фамилией уже существует в этой группе!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var newStudent = new Student();
            students.Add(newStudent);
            currentIndex = students.Count - 1;
            isDataChanged = false; // Новая карточка еще не изменена
            UpdateDisplay();
            ClearCurrentCard();

            // Фокус на поле ФИО для удобства ввода
            txtFullName.Focus();
        }

        private void SaveCurrentStudent_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentStudent();
        }

        private bool SaveCurrentStudent()
        {
            if (currentIndex >= 0 && currentIndex < students.Count && isDataChanged)
            {
                // Если все поля пустые - просто сбрасываем флаг изменений
                if (string.IsNullOrWhiteSpace(txtFullName.Text) &&
                    string.IsNullOrWhiteSpace(txtGroup.Text) &&
                    string.IsNullOrWhiteSpace(txtSubject.Text) &&
                    string.IsNullOrWhiteSpace(txtGrade.Text))
                {
                    isDataChanged = false;
                    return true;
                }

                // Проверяем, что все обязательные поля заполнены
                if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                    string.IsNullOrWhiteSpace(txtGroup.Text) ||
                    string.IsNullOrWhiteSpace(txtSubject.Text))
                {
                    MessageBox.Show("Заполните все поля: ФИО, Группа, Предмет!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Проверка оценки (может быть пустой)
                int grade = 0;
                if (!string.IsNullOrWhiteSpace(txtGrade.Text))
                {
                    if (!int.TryParse(txtGrade.Text, out grade) || grade < 1 || grade > 5)
                    {
                        MessageBox.Show("Оценка должна быть числом от 1 до 5 или пустой!", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtGrade.Focus();
                        return false;
                    }
                }

                string currentName = txtFullName.Text.Trim();

                // Проверка дубликатов фамилий (только если включена проверка)
                if (chkAllowDuplicateNames.IsChecked != true)
                {
                    // Ищем дубликаты среди всех студентов, кроме текущего
                    bool hasDuplicate = students
                        .Where((student, index) => index != currentIndex) // Исключаем текущего студента
                        .Any(student =>
                            student.FullName.Equals(currentName, StringComparison.OrdinalIgnoreCase) &&
                            student.Group.Equals(txtGroup.Text.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (hasDuplicate)
                    {
                        MessageBox.Show("Студент с такой фамилией уже существует в этой группе!", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtFullName.Focus();
                        return false;
                    }
                }

                students[currentIndex].FullName = currentName;
                students[currentIndex].Group = txtGroup.Text.Trim();
                students[currentIndex].Subject = txtSubject.Text.Trim();
                students[currentIndex].Grade = grade;

                isDataChanged = false;
                UpdateStatus("Данные сохранены");
                return true;
            }
            return true; // Если не было изменений, считаем успешным
        }

        private void ClearCurrentCard_Click(object sender, RoutedEventArgs e)
        {
            ClearCurrentCard();
        }

        private void ClearCurrentCard()
        {
            txtFullName.Text = "";
            txtGroup.Text = "";
            txtSubject.Text = "";
            txtGrade.Text = "";
            isDataChanged = true; // Помечаем как измененную, но не сохраняем автоматически
        }

        private void DeleteCurrentStudent_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex >= 0 && currentIndex < students.Count)
            {
                if (MessageBox.Show("Удалить текущего студента?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    students.RemoveAt(currentIndex);
                    if (currentIndex >= students.Count) currentIndex = students.Count - 1;
                    UpdateDisplay();
                    isDataChanged = false;
                    UpdateStatus("Студент удален");
                }
            }
        }

        private void ExpelCurrentStudent_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex >= 0 && currentIndex < students.Count)
            {
                var student = students[currentIndex];
                if (MessageBox.Show($"Отчислить студента {student.FullName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    student.IsExpelled = true;
                    expelledStudents.Add(student);
                    students.RemoveAt(currentIndex);

                    if (currentIndex >= students.Count) currentIndex = students.Count - 1;
                    UpdateDisplay();
                    isDataChanged = false;
                    UpdateStatus($"Студент {student.FullName} отчислен");
                }
            }
        }
        #endregion

        #region Вспомогательные методы
        private void UpdateDisplay()
        {
            if (students.Count == 0 || currentIndex < 0 || currentIndex >= students.Count)
            {
                txtFullName.Text = "";
                txtGroup.Text = "";
                txtSubject.Text = "";
                txtGrade.Text = "";
                txtPosition.Text = "0/0";
                txtStatus.Text = "Нет данных";
                isDataChanged = false;
                return;
            }

            var student = students[currentIndex];
            txtFullName.Text = student.FullName;
            txtGroup.Text = student.Group;
            txtSubject.Text = student.Subject;
            txtGrade.Text = student.Grade > 0 ? student.Grade.ToString() : "";
            txtPosition.Text = $"{currentIndex + 1}/{students.Count}";
            txtStatus.Text = student.IsExpelled ? "Отчислен" : "Активный";
            isDataChanged = false; // Сбрасываем флаг изменений при отображении

            // Скрываем отчисленных если не включен соответствующий чекбокс
            if (student.IsExpelled && chkShowExpelled.IsChecked != true)
            {
                // Ищем следующего неотчисленного студента
                var nextStudent = students
                    .Select((s, index) => new { Student = s, Index = index })
                    .Skip(currentIndex + 1)
                    .FirstOrDefault(x => !x.Student.IsExpelled);

                if (nextStudent != null)
                {
                    currentIndex = nextStudent.Index;
                    UpdateDisplay();
                }
                else
                {
                    // Если неотчисленных студентов больше нет, показываем сообщение
                    students.RemoveAt(currentIndex);
                    if (students.Count > 0)
                    {
                        currentIndex = Math.Min(currentIndex, students.Count - 1);
                        UpdateDisplay();
                    }
                    else
                    {
                        currentIndex = -1;
                        UpdateDisplay();
                    }
                }
            }
        }

        private void UpdateStatus(string message)
        {
            txtStatusBar.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void OnDataChanged(object sender, TextChangedEventArgs e)
        {
            isDataChanged = true;
        }
        #endregion

        #region Диалоги и настройки
        private void BrowseInputFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Все файлы (*.dat;*.txt;*.bin)|*.dat;*.txt;*.bin|DAT files (*.dat)|*.dat|Text files (*.txt)|*.txt|Binary files (*.bin)|*.bin",
                Title = "Выберите входной файл"
            };

            if (dialog.ShowDialog() == true)
            {
                txtInputPath.Text = dialog.FileName;
                LoadData();
            }
        }

        private void BrowseOutputFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "DAT files (*.dat)|*.dat|Text files (*.txt)|*.txt|Binary files (*.bin)|*.bin",
                Title = "Выберите выходной файл"
            };

            if (dialog.ShowDialog() == true)
            {
                txtOutputPath.Text = dialog.FileName;
                UpdateStatus("Выходной файл выбран");
            }
        }

        private void ManageExpelledStudents_Click(object sender, RoutedEventArgs e)
        {
            var expelledWindow = new ExpelledStudentsWindow(expelledStudents, students);
            if (expelledWindow.ShowDialog() == true)
            {
                expelledStudents = expelledWindow.ExpelledStudents;
                UpdateDisplay();
                UpdateStatus("Список отчисленных обновлен");
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists("settings.cfg"))
                {
                    var lines = File.ReadAllLines("settings.cfg");
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            switch (parts[0])
                            {
                                case "SaveState": chkSaveState.IsChecked = bool.Parse(parts[1]); break;
                                case "CycleNavigation": chkCycleNavigation.IsChecked = bool.Parse(parts[1]); break;
                                case "AllowDuplicateNames": chkAllowDuplicateNames.IsChecked = bool.Parse(parts[1]); break;
                                case "ShowExpelled": chkShowExpelled.IsChecked = bool.Parse(parts[1]); break;
                                case "InputPath": txtInputPath.Text = parts[1]; break;
                                case "OutputPath": txtOutputPath.Text = parts[1]; break;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Используем значения по умолчанию
            }
        }

        private void SaveSettings()
        {
            if (chkSaveState.IsChecked == true)
            {
                try
                {
                    var settings = new List<string>
                    {
                        $"SaveState={chkSaveState.IsChecked}",
                        $"CycleNavigation={chkCycleNavigation.IsChecked}",
                        $"AllowDuplicateNames={chkAllowDuplicateNames.IsChecked}",
                        $"ShowExpelled={chkShowExpelled.IsChecked}",
                        $"InputPath={txtInputPath.Text}",
                        $"OutputPath={txtOutputPath.Text}"
                    };
                    File.WriteAllLines("settings.cfg", settings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        #endregion

        #region Обработчики меню
        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            SaveData_Click(sender, e);
            Close();
        }

        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Менеджер студентов\nВерсия 1.0\n\nФункции:\n- Управление списком студентов\n- Работа с тремя типами файлов\n- Навигация по карточкам\n- Управление отчисленными студентами",
                          "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveData_Click(null, null);
            base.OnClosing(e);
        }
    }
}