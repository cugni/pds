using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;


namespace pds2.ServerSide
{
    /// <summary>
    /// Interaction logic for TastiRapidi.xaml
    /// </summary>
    public partial class TastiRapidi : UserControl
    {
        public TastiRapidi()
        {
            InitializeComponent();

        }
        private MainServerWindow _father;
        public void setFather(MainServerWindow father)
        {
            this._father = father;            
        }
        
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
          //  ComboBoxItem selected = (ComboBoxItem)comboBox1.SelectedValue;
            //string txt = "scelto-> " + selected.Content.ToString();
          //  KeyConverter con = new KeyConverter();
           //Key keystart = (Key)con.ConvertFromString(keyStart.Text);
           //Key keystop = (Key)con.ConvertFromString(keyStop.Text);
           // _father.kstart = keystart;
           // _father.kstop = keystop;
            IEnumerable l = EnumToList<Key>();
        }

        public static IEnumerable<T> EnumToList<T>()
        {
            Type enumType = typeof(T);

            // Can't use generic type constraints on value types,
            // so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            Array enumValArray = Enum.GetValues(enumType);
            List<T> enumValList = new List<T>(enumValArray.Length);

            foreach (int val in enumValArray)
            {
                enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
            }

            return enumValList;
        }



    }
}
