using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Shields.GraphViz.Components;
using Shields.GraphViz.Models;
using Shields.GraphViz.Services;
using System.Collections.Immutable;
using Microsoft.Win32;

namespace CurriculumVisualize
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Course> CourseList { get; set; }
        private List<Course> AvailableCourses { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            CourseList = new List<Course>();
            AvailableCourses = new List<Course>();
            listBox.ItemsSource = CourseList;
            CourseBox.ItemsSource = AvailableCourses;
        }
        /// <summary>
        /// Method to add a course to the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var courseName = NameField.Text;
            var courseCode = CodeField.Text.ToUpper();
            NameField.Text = "";
            CodeField.Text = "";
            var courseCount = CourseList.Count(course => course.CourseCode.Equals(courseCode));
            // If the course doesnt already exist
            if (courseCount == 0)
            {
                CourseList.Add(new Course(courseName, courseCode));
                listBox.ItemsSource = null;
                listBox.ItemsSource = CourseList;
            }
        }

        private void PrereqBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CourseBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        /// <summary>
        /// Construct a Graphviz dot file from the courses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                // New graphing function;
                Graph g = Graph.Undirected;
                for (var course = 0; course < CourseList.Count(); course++)
                {
                    // If course has no prerequisites then add a single node
                    if (!CourseList[course].Prerequisites.Any())
                    {
                        g = g.Add(NodeStatement.For($"{CourseList[course].Name}"));
                        continue;
                    }
                    for (var prereq = 0; prereq < CourseList[course].Prerequisites.Count(); prereq++)
                    {
                        g = g.Add(EdgeStatement.For($"{CourseList[course].Name}", $"{CourseList[course].Prerequisites[prereq].Name}"));
                    }
                }
                IRenderer renderer = new Renderer($"{Directory.GetCurrentDirectory()}\\graphviz");
                Console.WriteLine(Directory.GetCurrentDirectory());
                using (Stream file = File.Create("graph.png"))
                {
                    await renderer.RunAsync(g, file, RendererLayouts.Dot, RendererFormats.Png, System.Threading.CancellationToken.None);
                    file.Close();

                }

            } catch (Exception ex)
            {
                //textBox.Text = "An error has ocurred!\n" + ex;
                Console.WriteLine(ex);
                throw;
            }
        }
        /// <summary>
        /// Save a course list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            XmlSerializer x = new System.Xml.Serialization.XmlSerializer(CourseList.GetType());
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };
                if (saveDialog.ShowDialog() == true)
                {
                    var streamWriter = new StreamWriter(saveDialog.FileName, false, Encoding.Unicode, 8092);
                    x.Serialize(streamWriter, CourseList);
                    streamWriter.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        /// <summary>
        /// Open a course list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                if (openDialog.ShowDialog() == true)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Course>));

                    FileStream fs = new FileStream(openDialog.FileName, FileMode.Open);
                    XmlReader reader = XmlReader.Create(fs);

                    CourseList = (List<Course>)serializer.Deserialize(reader);
                    listBox.ItemsSource = null;
                    listBox.ItemsSource = CourseList;
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        /// <summary>
        /// Delete the selected course from CourseBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CourseList.Remove((Course)listBox.SelectedItem);
                listBox.ItemsSource = null;
                listBox.ItemsSource = CourseList;
                RefreshCourses();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void CourseBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (CourseBox.Items.Count == 0)
                return;
        }
        /// <summary>
        /// Add the selected prerequisite 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var prereqCourse = (Course)CourseBox.SelectedItem;
                var mainCourse = (Course)listBox.SelectedItem;
                mainCourse.Prerequisites.Add(prereqCourse);

                // Update the course
                PrereqBox.ItemsSource = null;
                PrereqBox.ItemsSource = mainCourse.Prerequisites;
                RefreshCourses();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
        /// <summary>
        /// Remove the selected prerequisite
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Remove the prerequisite
                var prerequisite = (Course)PrereqBox.SelectedItem;
                Course course = (Course)listBox.SelectedItem;
                course.Prerequisites.Remove(prerequisite);

                // Update prerequisites
                PrereqBox.ItemsSource = null;
                PrereqBox.ItemsSource = course.Prerequisites;
                RefreshCourses();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        /// <summary>
        /// When the selection in the top main listBox is changed, update the courses that can be moved to the prerequisites
        /// in the CourseBox. THe box should contain all the courses except for itself and the prerequisites
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var course = (Course)listBox.SelectedItem;
                PrereqBox.ItemsSource = null;
                PrereqBox.ItemsSource = course.Prerequisites;
                RefreshCourses();
            } catch (NullReferenceException n)
            {
                listBox.SelectedIndex = 0;
                RefreshCourses();
            }
        }
        /// <summary>
        /// Refreshes the courses that can be added to the prerequisites (left box)
        /// </summary>
        private void RefreshCourses()
        {
            try
            {
                // get the available courses
                AvailableCourses = new List<Course>(CourseList);
                // Remove the selected course from the main course box from this list
                var selectedCourse = (Course)listBox.SelectedItem;
                AvailableCourses.Remove(AvailableCourses.First(course => course.Name.Equals(selectedCourse.Name)));

                // Remove courses from availablecourses if they exist in the prequisites
                foreach (var course in selectedCourse.Prerequisites)
                {
                    try
                    {
                        // Find the course in the prerequisites and remove it from that othr box
                        var x = AvailableCourses.First(c => c.CourseCode.Equals(course.CourseCode));
                        AvailableCourses.Remove(x);
                    }
                    catch (Exception
                  ex)
                    {
                        // no results
                    }
                }
                // Update the Course List
                CourseBox.ItemsSource = null;
                CourseBox.ItemsSource = AvailableCourses;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
    /// <summary>
    /// Course object
    /// </summary>
    public class Course
    {
        public string Name { get; set; }
        public string CourseCode { get; set; }
        public List<Course> Prerequisites { get; set; }
        public Course(string name, string code)
        {
            Name = name;
            CourseCode = code;
            Prerequisites = new List<Course>();
        }
        public Course()
        {
            Name = "";
            CourseCode = "";
            Prerequisites = new List<Course>();
        }
    }
}
