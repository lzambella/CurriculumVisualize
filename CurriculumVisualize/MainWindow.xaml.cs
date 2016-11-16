using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuickGraph;
using QuickGraph.Graphviz;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace CurriculumVisualize
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Course> CourseList { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            CourseList = new List<Course>();
            CourseBox.ItemsSource = CourseList;
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
            var courseCount = CourseList.Where(course => course.CourseCode.Equals(courseCode)).Count();
            // If the course doesnt already exist
            if (courseCount == 0)
            {
                CourseList.Add(new Course(courseName, courseCode));
                CourseBox.ItemsSource = null;
                CourseBox.ItemsSource = CourseList;
            }
        }
        /// <summary>
        /// Method to handle adding a prerequisite
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedCourse = (Course) CourseBox.SelectedItem;

                var course = CourseList.Where(c => c.CourseCode.Equals(PrereqField.Text)).First();
                if (course == null)
                    return;
                selectedCourse.Prerequisites.Add(course);
                PrereqBox.ItemsSource = null;
                PrereqBox.ItemsSource = selectedCourse.Prerequisites;
            } catch (Exception ex)
            {

            }
        }

        private void PrereqBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        /// <summary>
        /// Gets the currently selected item in the Course Box and displays their requirements (if any) in the prerequisite box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CourseBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedCourse = (Course)CourseBox.SelectedItem;
                PrereqBox.ItemsSource = null;
                PrereqBox.ItemsSource = selectedCourse.Prerequisites;
                CodeField.Text = selectedCourse.CourseCode;
                NameField.Text = selectedCourse.Name;
            } catch (Exception ex)
            {

            } 
        }
        /// <summary>
        /// Construct a Graphviz dot file from the courses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                // The dictionary will use the CourseList index and then add edges from the other indexes
                var vertices = new Dictionary<string, string[]>(); // vertex -> target edges
                foreach (var course in CourseList)
                {
                    var mainCourseIndex = CourseList.IndexOf(course);
                    // Contains the indexes of the requirements from CourseList
                    var reqs = new string[course.Prerequisites.Count];
                    // Go through all the requirements and add them to the array
                    var index = 0;
                    foreach (var req in course.Prerequisites)
                    {
                        // get the index
                        var courseIndex = CourseList.IndexOf(req);
                        // Add the index
                        //reqs[index] = courseIndex;
                        reqs[index] = req.Name;
                        index++;
                    }
                    vertices.Add(course.Name, reqs);
                }
                var graph = vertices.ToVertexAndEdgeListGraph(kv => Array.ConvertAll(kv.Value, v => new SEquatableEdge<string>(kv.Key, v)));
                var graphviz = graph.ToGraphviz();
                StringReader reader = new StringReader(graphviz);
                var line = "";
                var graphOutput = "";
                while ((line = reader.ReadLine()) != null)
                {
                    graphOutput += line.PadLeft(25) + "\n";
                }
                // replace indexes in graph with their name
                for (int i = 0; i < CourseList.Count; i++)
                {
                    graphOutput = graphOutput.Replace($" {i} ", $"\"{CourseList[i].Name}\"");
                }
                // Output the graph
                textBox.Text = graphOutput;

            } catch (Exception ex)
            {
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
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(CourseList.GetType());
            try
            {
                
                var streamWriter = new System.IO.StreamWriter("list.xml", false, Encoding.Unicode, 8092);
                x.Serialize(streamWriter, CourseList);
                streamWriter.Close();
            } catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Open a course list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the XmlSerializer specifying type and namespace.
            XmlSerializer serializer = new XmlSerializer(typeof(List<Course>));

            // A FileStream is needed to read the XML document.
            FileStream fs = new FileStream("list.xml", FileMode.Open);
            XmlReader reader = XmlReader.Create(fs);

            // Use the Deserialize method to restore the object's state.
            CourseList = (List<Course>)serializer.Deserialize(reader);
            CourseBox.ItemsSource = null;
            CourseBox.ItemsSource = CourseList;
            fs.Close();
        }
        /// <summary>
        /// Delete the selected course from CourseBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CourseList.Remove((Course)CourseBox.SelectedItem);
            CourseBox.ItemsSource = null;
            CourseBox.ItemsSource = CourseList;
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
