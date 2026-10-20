using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class MainWindow : Window
    {
        public ObservableCollection<Versenyzo> Versenyzok { get; set; } = new ObservableCollection<Versenyzo>();
        private ICollectionView versenyzoView;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Gyűjtemény nézet beállítása a szűréshez
            versenyzoView = CollectionViewSource.GetDefaultView(Versenyzok);
            versenyzoView.Filter = SzuroFeltetel;

            FrissitLetszam();
        }

        private bool SzuroFeltetel(object item)
        {
            if (item is Versenyzo v)
            {
                if (cmbRekordSzuro == null || cmbRekordSzuro.SelectedItem == null) return true;

                string szo = ((ComboBoxItem)cmbRekordSzuro.SelectedItem).Content.ToString();
                if (szo.Contains("OR")) return v.OR;
                if (szo.Contains("világcsúcs")) return v.VR;
                if (szo.Contains("Európa-csúcs")) return v.ER;
            }
            return true;
        }

        private void Szuro_Valtozott(object sender, SelectionChangedEventArgs e)
        {
            if (versenyzoView != null)
            {
                versenyzoView.Refresh();
                FrissitLetszam();
            }
        }

        private void FrissitLetszam()
        {
            int lathato = 0;
            if (versenyzoView != null)
            {
                foreach (var item in versenyzoView)
                {
                    lathato++;
                }
            }
            txtLetszam.Text = $"Látható: {lathato} / Összesen: {Versenyzok.Count}";
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
            bool or = chkOR.IsChecked == true;
            bool vr = chkVR.IsChecked == true;
            bool er = chkER.IsChecked == true;

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
            if (Versenyzok.Any(v => v.Rajtszam == rajtszam))
            {
                MessageBox.Show("A kért rajtszám már foglalt. Adjon meg másikat, vagy hagyja üresen a mezőt.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            Versenyzo ujVersenyzo = new Versenyzo(rajtszam, nev, versenyszam, nem, eletkor, or, vr, er);
            Versenyzok.Add(ujVersenyzo);

            FrissitLetszam();

            MessageBox.Show($"A regisztráció sikeres. Kiosztott rajtszám: {rajtszam}", "Tájékoztatás", MessageBoxButton.OK, MessageBoxImage.Information);

            txtNev.Clear();
            txtRajtszam.Clear();
        }

        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "atletika";
            sfd.Filter = "CSV-fájl (*.csv)|*.csv|Szövegfájl (*.txt)|*.txt";
            sfd.FilterIndex = 1;

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    if (sfd.FilterIndex == 1)
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            sw.WriteLine("Rajtszam;Nev;Versenyszam;Nem;Eletkor;OR;VR;ER");
                            foreach (var v in Versenyzok)
                            {
                                sw.WriteLine($"{v.Rajtszam};{v.Nev};{v.Versenyszam};{v.Nem};{v.Eletkor};{(v.OR ? 1 : 0)};{(v.VR ? 1 : 0)};{(v.ER ? 1 : 0)}");
                            }
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            for (int i = 0; i < Versenyzok.Count; i++)
                            {
                                var v = Versenyzok[i];
                                sw.WriteLine(v.Nev);
                                string orSzoveg = v.OR ? "olimpiai csúccsal rendelkezik" : "olimpiai csúccsal nem rendelkezik";
                                string vrSzoveg = v.VR ? "világcsúccsal rendelkezik" : "világcsúccsal nem rendelkezik";
                                string erSzoveg = v.ER ? "Európa-csúccsal rendelkezik" : "Európa-csúccsal nem rendelkezik";

                                sw.WriteLine($"A {v.Eletkor} éves {(v.Nem == "F" ? "férfi" : "nő")} versenyző a(z) {v.Versenyszam} versenyszámban indul, rajtszáma {v.Rajtszam}. {orSzoveg}, {vrSzoveg}, {erSzoveg}.");

                                if (i < Versenyzok.Count - 1)
                                {
                                    sw.WriteLine(); 
                                }
                            }
                        }
                    }
                    MessageBox.Show("A mentés sikeresen megtörtént.", "Tájékoztatás", MessageBoxButton.OK, MessageBoxImage.Information);
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

                    using (StreamReader sr = new StreamReader(ofd.FileName, Encoding.UTF8))
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
                                    adatok[5] == "1",
                                    adatok[6] == "1",
                                    adatok[7] == "1"
                                );
                                ideiglenesLista.Add(v);
                            }
                        }
                    }

                    
                    Versenyzok.Clear();
                    foreach (var v in ideiglenesLista)
                    {
                        Versenyzok.Add(v);
                    }

                    FrissitLetszam();
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
            public bool OR { get; set; }
            public bool VR { get; set; }
            public bool ER { get; set; }

            public Versenyzo(int rajtszam, string nev, string versenyszam, string nem, int eletkor, bool or, bool vr, bool er)
            {
                Rajtszam = rajtszam;
                Nev = nev;
                Versenyszam = versenyszam;
                Nem = nem;
                Eletkor = eletkor;
                OR = or;
                VR = vr;
                ER = er;
            }
        }
    }
}