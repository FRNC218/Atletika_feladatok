using Microsoft.Win32;
using System.IO;
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

namespace Atletika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Eletkor_Valtozott(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtEletkor != null)
            {
                txtEletkor.Text = $"{sldEletkor.Value} év";
            }
        }

        private void Regisztral_Click(object sender, RoutedEventArgs e)
        {
            string nev = txtNev.Text.Trim();
            if (string.IsNullOrEmpty(nev))
            {
                MessageBox.Show("A versenyző nevének megadása kötelező.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (nev.Contains(';') || nev.Contains('|'))
            {
                MessageBox.Show("A névben pontosvessző és függőleges vonal nem használható.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbVersenyszam.SelectedItem == null)
            {
                MessageBox.Show("Válasszon versenyszámot.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string versenyszam = ((ComboBoxItem)cmbVersenyszam.SelectedItem).Content.ToString();

            string nem = "";
            if (rbFerfi.IsChecked == true) nem = "F";
            else if (rbNo.IsChecked == true) nem = "N";

            if (string.IsNullOrEmpty(nem))
            {
                MessageBox.Show("Adja meg a versenyző nemét.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int eletkor = (int)sldEletkor.Value;
            int olimpiai = chkolimpiai.IsChecked == true ? 1 : 0;
            int vb = chkVB.IsChecked == true ? 1 : 0;
            int eb = chkEB.IsChecked == true ? 1 : 0;

            List<int> foglaltRajtszamok = new List<int>();
            foreach (var item in lstVersenyzok.Items)
            {
                string[] részek = item.ToString().Split('|');
                if (int.TryParse(részek[0].Trim(), out int rsz))
                {
                    foglaltRajtszamok.Add(rsz);
                }
            }

            int rajtszam = 0;
            string rajtszamSzoveg = txtRajtszam.Text.Trim();


            if (!int.TryParse(rajtszamSzoveg, out rajtszam) || rajtszam < 1 || rajtszam > 9999)
            {
                MessageBox.Show("A rajtszám 1 és 9999 közötti egész szám lehet.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (rajtszamSzoveg == "")
            {
                MessageBox.Show("A rajtszám megadása kötelező.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (foglaltRajtszamok.Contains(rajtszam))
            {
                MessageBox.Show("A kért rajtszám már foglalt. Adjon meg másikat, vagy hagyja üresen a mezőt.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            Versenyzo ujVersenyzo = new Versenyzo(rajtszam, nev, versenyszam, nem, eletkor, olimpiai, vb, eb);
            lstVersenyzok.Items.Add(ujVersenyzo);

            txtLetszam.Text = $"Regisztrált versenyzők: {lstVersenyzok.Items.Count}";

            txtNev.Clear();
            txtRajtszam.Clear();
        }

        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "atletika.csv";
            sfd.Filter = "CSV-fájl (*.csv)|*.csv";
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("Rajtszam;Nev;Versenyszam;Nem;Eletkor;OlimpiaiCsucs;VBCsucs;EBCsucs");
                        foreach (var item in lstVersenyzok.Items)
                        {
                            string[] részek = item.ToString().Split('|');
                            sw.WriteLine($"{részek[0].Trim()};{részek[1].Trim()};{részek[2].Trim()};{részek[3].Trim()};{részek[4].Trim()};{részek[5].Trim()};{részek[6].Trim()};{részek[7].Trim()}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba a fájl mentése során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Betoltes_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV-fájl (*.csv)|*.csv";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    List<Versenyzo> ideiglenesLista = new List<Versenyzo>();
                    List<int> ellenorzoRajtszamok = new List<int>();

                    using (StreamReader sr = new StreamReader(ofd.FileName, System.Text.Encoding.UTF8))
                    {
                        string fejlec = sr.ReadLine();
                        while (!sr.EndOfStream)
                        {
                            string sor = sr.ReadLine();
                            if (string.IsNullOrWhiteSpace(sor)) continue;

                            string[] adatok = sor.Split(';');
                            if (adatok.Length >= 8)
                            {
                                int rsz = int.Parse(adatok[0]);
                                if (ellenorzoRajtszamok.Contains(rsz))
                                {
                                    MessageBox.Show($"Hiba: A(z) {rsz} rajtszám többször is szerepel a fájlban!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                ellenorzoRajtszamok.Add(rsz);

                                Versenyzo v = new Versenyzo(
                                    rsz,
                                    adatok[1],
                                    adatok[2],
                                    adatok[3],
                                    int.Parse(adatok[4]),
                                    int.Parse(adatok[5]),
                                    int.Parse(adatok[6]),
                                    int.Parse(adatok[7])
                                );
                                ideiglenesLista.Add(v);
                            }
                        }
                    }

                    
                    lstVersenyzok.Items.Clear();
                    foreach (var v in ideiglenesLista)
                    {
                        lstVersenyzok.Items.Add(v);
                    }
                    txtLetszam.Text = $"Regisztrált versenyzők: {lstVersenyzok.Items.Count}";
                    MessageBox.Show("A fájl betöltése sikeres volt.", "Tájékoztatás", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba a fájl betöltése során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public class Versenyzo
        {
            public int Rajtszam { get; set; }
            public string Nev { get; set; }
            public string Versenyszam { get; set; }
            public string Nem { get; set; }
            public int Eletkor { get; set; }
            public int OlimpiaiCsucs { get; set; }
            public int VBCsucs { get; set; }
            public int EBCsucs { get; set; }

            public Versenyzo(int rajtszam, string nev, string versenyszam, string nem, int eletkor, int olimpiaiCsucs, int vbCsucs, int ebCsucs)
            {
                Rajtszam = rajtszam;
                Nev = nev;
                Versenyszam = versenyszam;
                Nem = nem;
                Eletkor = eletkor;
                OlimpiaiCsucs = olimpiaiCsucs;
                VBCsucs = vbCsucs;
                EBCsucs = ebCsucs;
            }

            public override string ToString()
            {
                return $"{Rajtszam} | {Nev} | {Versenyszam} | {Nem} | {Eletkor}|{OlimpiaiCsucs}|{VBCsucs}|{EBCsucs}";
            }
        }
    }
}