using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StudentsForm
{
    public partial class ExpelledStudentsWindow : Window
    {
        public List<MainWindow.Student> ExpelledStudents { get; set; }
        private List<MainWindow.Student> mainStudents;

        public ExpelledStudentsWindow(List<MainWindow.Student> expelled, List<MainWindow.Student> mainStudents)
        {
            InitializeComponent();
            ExpelledStudents = new List<MainWindow.Student>(expelled);
            this.mainStudents = mainStudents;
            dgExpelledStudents.ItemsSource = ExpelledStudents;
        }

        private void RestoreSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudents = ExpelledStudents.Where(s => s.IsSelected).ToList();
            if (selectedStudents.Count > 0)
            {
                foreach (var student in selectedStudents)
                {
                    student.IsExpelled = false;
                    student.IsSelected = false;
                    mainStudents.Add(student);
                    ExpelledStudents.Remove(student);
                }
                dgExpelledStudents.Items.Refresh();
                MessageBox.Show($"{selectedStudents.Count} студентов восстановлено", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выберите студентов для восстановления", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudents = ExpelledStudents.Where(s => s.IsSelected).ToList();
            if (selectedStudents.Count > 0)
            {
                if (MessageBox.Show($"Удалить {selectedStudents.Count} студентов безвозвратно?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    foreach (var student in selectedStudents)
                    {
                        ExpelledStudents.Remove(student);
                    }
                    dgExpelledStudents.Items.Refresh();
                    MessageBox.Show($"{selectedStudents.Count} студентов удалено", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите студентов для удаления", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
    }
}