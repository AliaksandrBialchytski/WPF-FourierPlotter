using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WpfFourierPlotter
{
    [XmlRoot("CircleList")]
    [XmlInclude(typeof(CircleOwn))] 
    public class CircleOwnList
    {
        [XmlArray("CircleList")]
        [XmlArrayItem("Circle")]
        public ObservableCollection<CircleOwn> Circles = new ObservableCollection<CircleOwn>();

        [XmlElement("Listname")]
        public string Listname { get; set; }
        public CircleOwnList() { }

        public CircleOwnList(string name)
        {
            this.Listname = name;
        }

        public void AddPerson(CircleOwn person)
        {
            Circles.Add(person);
        }
    }
    [XmlType("Circle")]
    public class CircleOwn
    {

        [XmlElement("radius")]
        public int Radius { get; set; }

        [XmlElement("frequency")]
        public int Frequency { get; set; }
        public int[,] tab;
        public CircleOwn()
        {
            Radius = 0;
            Frequency = 0;
        }
        public CircleOwn(int height, int width)
        {
            Radius = height;
            Frequency = width;
        }

    }
}
